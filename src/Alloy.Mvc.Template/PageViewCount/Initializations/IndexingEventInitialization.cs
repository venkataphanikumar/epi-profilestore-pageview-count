using AlloyTemplates.Models.Pages;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.ClientConventions;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using PageViewCount.Extensions;

namespace PageViewCount.Initializations
{
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    public class IndexingEventInitialization : IInitializableModule
    {
      
        private Injected<IClient> Client { get; set; }
        private Injected<IContentEvents> ContentEvents { get; set; }

        /// <summary>
        /// Add EPi FInd Client conventions to get the popular Article Items
        /// </summary>
        /// <param name="context"></param>
        public void Initialize(InitializationEngine context)
        {
           Client.Service.Conventions.ForInstancesOf<SitePageData>()
                .IncludeField(x =>x.GetPageViews());
         

        }
      
        public void Uninitialize(InitializationEngine context)
        {
            //Add uninitialization logic
         
        }
    }
}
