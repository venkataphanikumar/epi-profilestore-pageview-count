using System.Collections.Generic;
using AlloyTemplates.Models.Pages;
using EPiServer.Core;
using EPiServer.Find.Cms;

namespace AlloyTemplates.PageViewCount.Services.Interfaces
{
    public interface IFindService
    {
        IContentResult<T> FindPagesMatchingAllCategories<T>(ContentReference rootPage, CategoryList categories,  int pageNumber = 1, int pageSize = 30,
            string language = null) where T : PageData;

        IContentResult<T> FindPagesMatchingAnyCategory<T>(ContentReference rootPage, CategoryList categories,  int pageNumber = 1, int pageSize = 30,
            string language = null) where T : PageData;

     

        IContentResult<T> FindRecentPage<T>(ContentReference rootPage, CategoryList categories, int itemToTake)
            where T : PageData;

        IContentResult<T> FindRelatedArticles<T>(ContentReference rootPage, CategoryList categories,ContentReference excludeContentReference, int itemToTake=3,
            string language = null) where T : SitePageData;

        IContentResult<T> FindPopularArticles<T>(ContentReference rootPage, CategoryList categories, ContentReference excludeContentReference, int itemToTake = 4,
            string language = null) where T : SitePageData;

        IContentResult<T> FindArticlesMatchingAnyCategory<T>(ContentReference rootPage, CategoryList categories,List<ContentReference> excludeReferences,
            int pageNumber = 1, int pageSize = 30, string language = null) where T : SitePageData;





    }
}
