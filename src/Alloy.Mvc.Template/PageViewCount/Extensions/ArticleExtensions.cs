using System.Linq;
using AlloyTemplates.Models.Pages;
using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;
using PageViewCount.DataStore;
using PageViewCount.Models;

namespace PageViewCount.Extensions
{
    public static class  ArticleExtensions
    {
        private static Injected<DynamicDataStoreFactory> DynamicDataStore { get; set; }

        /// <summary>
        /// Getting the Views form Custom EPi Data store
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        public static PageViews GetPageViews(this SitePageData article)
        {
            if (article is SitePageData page)
            {
                var customTableInsightPageViewsData = DynamicDataStore.Service.CreateStore(typeof(CustomTableInsightPageViewsData))
                    .Items<CustomTableInsightPageViewsData>().FirstOrDefault(x => x.PageId.Equals(page.ContentLink.ID) && x.LanguageCode.Equals(page.Language.Name));

                return customTableInsightPageViewsData == null
                    ? new PageViews()
                    : new PageViews
                    {
                        TotalViews = customTableInsightPageViewsData.ViewsCount,
                        LastViewDateTime = customTableInsightPageViewsData.LastViewdDateTime
                    };

            }
            return new PageViews();

        }


    }
}
