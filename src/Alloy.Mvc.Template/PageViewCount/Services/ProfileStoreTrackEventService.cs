using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AlloyTemplates.Models.Pages;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data.Dynamic;
using EPiServer.Find;
using EPiServer.ServiceLocation;
using PageViewCount.DataStore;
using PageViewCount.Services.Interfaces;

namespace PageViewCount.Services
{
    [ServiceConfiguration(ServiceType = typeof(IProfileStoreTrackEventService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ProfileStoreTrackEventService: IProfileStoreTrackEventService
    {
        private readonly DynamicDataStoreFactory _dynamicDataStoreFactory;
        private readonly IClient _client;
        private readonly IContentLoader _contentLoader;

        public ProfileStoreTrackEventService(DynamicDataStoreFactory dynamicDataStoreFactory,
            IClient client,
            IContentLoader contentLoader)
        {
            _dynamicDataStoreFactory = dynamicDataStoreFactory;
            _client = client;
            _contentLoader = contentLoader;
        }
        /// <summary>
        /// Update the page views
        /// </summary>
    
        public void UpdatePageViews()
        {
            var historyItems = _dynamicDataStoreFactory.CreateStore(typeof(CustomTableInsightPageViewsDataHistory)).Items<CustomTableInsightPageViewsDataHistory>().ToList();

            var historyItemsCount = historyItems.Count();
            var batchSize = 100;
            var pageNumber = 0;

            do
            {
                    pageNumber = pageNumber + 1;
                var filteredItems = historyItemsCount > batchSize * pageNumber ? historyItems.Skip(pageNumber * batchSize).Take(batchSize).ToList() : historyItems.ToList(); ;
                ProcessPageViews(filteredItems);


            } while (historyItemsCount > batchSize * pageNumber);


        }
        /// <summary>
        /// Index the pages in batches
        /// </summary>

        public void UpdateEpiFindPageViews()
        {
            //Get all items to index

            var items = _dynamicDataStoreFactory.CreateStore(typeof(CustomTableInsightPageViewsData)).Items<CustomTableInsightPageViewsData>();

            var itemsCount = items.Count();
            var batchSize = 100;
            var pageNumber = 0;
            do
            {
                pageNumber = pageNumber + 1;
                var filteredItems = itemsCount > batchSize * pageNumber?items.Skip(pageNumber * batchSize).Take(batchSize).ToList(): items.ToList();

                var pageList=new List<SitePageData>();

                foreach (var filteredItem in filteredItems)
                {
                    var page = _contentLoader.Get<SitePageData>(new ContentReference(filteredItem.PageId),new CultureInfo(filteredItem.LanguageCode));
                    pageList.Add(page);
                    
                }
                _client.Index(pageList);

            } while (itemsCount > batchSize * pageNumber);

        }

        /// <summary>
        /// Process page views by batches
        /// </summary>
        /// <param name="historyItems"></param>
        public void ProcessPageViews(List<CustomTableInsightPageViewsDataHistory> historyItems)
        {
            var pageViewItemStore = _dynamicDataStoreFactory.CreateStore(typeof(CustomTableInsightPageViewsData));
            var pageViewItems = pageViewItemStore.Items<CustomTableInsightPageViewsData>().ToList();
            var historyItemsGroupById = historyItems.GroupBy(x => x.PageId);

           //Delete all information older than 30days
            var pageViewOldItems = pageViewItems
                .Where(x => x.LastViewdDateTime <= DateTime.UtcNow.AddDays(-30));

            foreach (var pageViewOldItem in pageViewOldItems)
            {
                pageViewItemStore.Delete(pageViewOldItem.Id);

            }

            foreach (var item in historyItemsGroupById)
            {
                var pageViewHistoryItemsGroupByLangCode = item.ToList().GroupBy(x => x.LanguageCode);

                foreach (var historyItem in pageViewHistoryItemsGroupByLangCode)
                {
                    var pageViewItem = historyItem.ToList().OrderByDescending(x => x.LastViewdDateTime)
                        .FirstOrDefault();
                    if (pageViewItem == null) continue;

                    //check the Item availability if not create the new item in the store.

                    if (pageViewItems.Any(x => x.PageId.Equals(item.Key)&&x.LanguageCode.Equals(historyItem.Key) ))
                    {

                        //pull the existing Item.
                        var existingItem = pageViewItems.FirstOrDefault(x =>
                            x.PageId.Equals(item.Key) && x.LanguageCode.Equals(historyItem.Key));

                        if (existingItem == null ) continue;
                        
                        //update the last viewed date and view count
                   
                        existingItem.ViewsCount =  existingItem.ViewsCount+ historyItem.Where(x=> existingItem.LastViewdDateTime<x.LastViewdDateTime).ToList().Sum(x => x.ViewsCount);
                        existingItem.LastViewdDateTime = pageViewItem.LastViewdDateTime;
                        pageViewItemStore.Save(existingItem);
                    }

                    else
                    {
                            var customTableInsightPageViewsDataHistory = new CustomTableInsightPageViewsDataHistory
                            {
                                PageId = pageViewItem.PageId,
                                LanguageCode = historyItem.Key,
                                ViewsCount = historyItem.ToList().Sum(x => x.ViewsCount),
                                LastViewdDateTime = pageViewItem.LastViewdDateTime
                            };
                            pageViewItemStore.Save(customTableInsightPageViewsDataHistory);
                        

                    }



                }

                
            }


        }


     

    }
}
