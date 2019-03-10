using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AlloyTemplates.PageViewCount.DelegatingHandlers;
using AlloyTemplates.PageViewCount.Services.Interfaces;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.DataAbstraction;
using EPiServer.Find;
using EPiServer.Logging;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using PageViewCount.DataStore;
using PageViewCount.Models;
using PageViewCount.Services.Interfaces;

namespace AlloyTemplates.PageViewCount.Jobs
{
    [ScheduledPlugIn(DisplayName = "Index Recent Hits", GUID = "d6619008-3e76-4886-b3c7-9a025a0c2888")]
    public class IndexRecentHits : ScheduledJobBase
    {
        //Settings
        private readonly string _apiRootUrl = ConfigurationManager.AppSettings["episerver:profiles.ProfileApiBaseUrl"];
        private readonly string _appKey = ConfigurationManager.AppSettings["episerver:profiles.ProfileApiSubscriptionKey"];
        private readonly string _eventUrl = "/api/v1.0/trackevents/";
        private readonly int _resultsPerPage = 100;
        private Dictionary<string, int> _recentHits = new Dictionary<string, int>();
        private bool _stopSignaled;
        private static IContentLoader _contentLoader;
        private readonly IClient _client;
        private readonly DynamicDataStoreFactory _dataStoreFactory;
        private readonly IHttpClientService _httpClientService;
        private readonly IScheduledJobExecutor _scheduledJobExecutor;
        private readonly IScheduledJobRepository _scheduledJobRepository;
        private readonly IProfileStoreTrackEventService _profileStoreTrackEventService;
        private ILogger _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IndexRecentHits(IContentLoader contentLoader,
            IClient client,
            IProfileStoreTrackEventService profileStoreTrackEventService,
            DynamicDataStoreFactory dataStoreFactory,
            IHttpClientService httpClientService,
            IScheduledJobExecutor scheduledJobExecutor,
            IScheduledJobRepository scheduledJobRepository
           )
        {
            _contentLoader = contentLoader;
            _client = client;
            _dataStoreFactory = dataStoreFactory;
            _httpClientService = httpClientService;
            _scheduledJobExecutor = scheduledJobExecutor;
            _scheduledJobRepository = scheduledJobRepository;
            _profileStoreTrackEventService = profileStoreTrackEventService;
        }

        public IndexRecentHits()
        {
            IsStoppable = true;
        }

        /// <summary>
        /// Called when a user clicks on Stop for a manually started job, or when ASP.NET shuts down.
        /// </summary>
        public override void Stop()
        {
            _stopSignaled = true;
        }

        /// <summary>
        /// Called when a scheduled job executes
        /// </summary>
        /// <returns>A status message to be stored in the database log and visible from admin mode</returns>
        public override string Execute()
        {
            try
            {
            //Call OnStatusChanged to periodically notify progress of job for manually started jobs
            OnStatusChanged(String.Format("Beginning processing of recent hits"));

            var totalProcessed = 0;
            var errorCount = 0;

            var fromDate = _scheduledJobRepository.Get(ScheduledJobId).LastExecution.ToUniversalTime().ToString("s");
         
            // Gather the data from Profile Store
              ProcessEventResults($"EventType eq epiPageView and EventTime gt {fromDate}", _resultsPerPage);

            if (_stopSignaled)
            {
                return "Execution was cancelled by user";
            }
           
            //create a PageView Data History store and  Update the store with Profile store information
            var store = _dataStoreFactory.CreateStore(typeof(CustomTableInsightPageViewsDataHistory));

            //Delete all information older than 30days
            var pageViewHistoryItems = store.Items<CustomTableInsightPageViewsDataHistory>()
                .Where(x => x.LastViewdDateTime <= DateTime.UtcNow.AddDays(-30));

            foreach (var pageViewHistoryItem in pageViewHistoryItems)
            {
                store.Delete(pageViewHistoryItem.Id);
               
            }
            //Update the PageView Data History store
            foreach (var hit in _recentHits)
            {
                if (_stopSignaled)
                {
                    return "Execution was cancelled by user";
                }
                try
                {
                    var keyParts = hit.Key.Split('_');
                    var page = _contentLoader.Get<PageData>(new Guid(keyParts.FirstOrDefault() ?? Guid.Empty.ToString()));
                    var customTableInsightPageViewsDataHistory = new CustomTableInsightPageViewsDataHistory
                    {
                        PageId = page.ContentLink.ID,
                        LanguageCode = keyParts.LastOrDefault() ?? "en",
                        ViewsCount = hit.Value,
                        LastViewdDateTime = DateTime.Now.ToUniversalTime()
                    };
                    store.Save(customTableInsightPageViewsDataHistory);
                }
                catch (Exception)
                {
                    errorCount++;
                }
                totalProcessed++;
                if (totalProcessed.ToString().EndsWith("0"))
                {
                    OnStatusChanged($"Indexed {totalProcessed} of {_recentHits.Count} with {errorCount} errors");
                }
            }

            _profileStoreTrackEventService.UpdatePageViews();

            _profileStoreTrackEventService.UpdateEpiFindPageViews();

            return $"Reindexed {totalProcessed} pages with {errorCount} errors";
            }
            catch (Exception ex)
            {
                _log.Error(ex.ToString());
                return $"Reindex of pages failed";
            }
        }
        #region Private Methods
        /// <summary>
        /// Makes a request to ProfileStore and processes results
        /// </summary>
        private void ProcessEventResults(string filter, int resultsPerPage=100, int pageNumber=1)
        {
            

            OnStatusChanged($"Fetching hits page {pageNumber}");
            if (_stopSignaled)
            {
                return;
            }

            //Handle pagination
           

            // Execute the request to get the events matching the filter
            var eventResponseObject = GetTrackingResponse(filter, resultsPerPage, pageNumber).Result;
            foreach (var result in eventResponseObject.Items)
            {
                //Add/update the hit count per event
                var key = $"{result.Payload.epi.contentGuid}_{result.Payload.epi.language}";
                if (_recentHits.ContainsKey(key))
                {
                    _recentHits[key]++;
                }
                else
                {
                    _recentHits.Add(key, 1);
                }
            }

            //Repeat until all pages of results have been processed
            if (eventResponseObject.Total > _resultsPerPage * pageNumber)
            {
                ProcessEventResults(filter,100, pageNumber + 1);
            }

        }

        /// <summary>
        /// Builds the ProfileStore request
        /// </summary>
        private async Task<TrackingObjectResponse> GetTrackingResponse(string filter, int resultsPerPage, int pageNumber)
        {
            var path = $"?$filter={filter}&$top={resultsPerPage}&appKey={ _appKey}&{(pageNumber - 1) * _resultsPerPage}";
            var result = await _httpClientService.GetResult<ProfileStoreDelegatingHandlers, TrackingObjectResponse>($"{_apiRootUrl}{_eventUrl}", path);
            return result;
        }

        #endregion

    }
}
