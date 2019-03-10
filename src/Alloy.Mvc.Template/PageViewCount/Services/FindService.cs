using System;
using System.Collections.Generic;
using AlloyTemplates.Models.Pages;
using AlloyTemplates.PageViewCount.Services.Interfaces;
using EPiServer.Core;
using EPiServer.Find;
using EPiServer.Find.Cms;
using EPiServer.ServiceLocation;
using PageViewCount.Extensions;

namespace AlloyTemplates.PageViewCount.Services
{
    [ServiceConfiguration(ServiceType = typeof(IFindService), Lifecycle = ServiceInstanceScope.Singleton)]
    public class FindService : IFindService
    {
        private readonly IClient _client;

        public FindService(IClient client)
        {
            _client = client;
        }
        /// <summary>
        /// Find Pages with All matching categories
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootPage"></param>
        /// <param name="categories"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="language"></param>
        /// <returns></returns>

        public IContentResult<T> FindPagesMatchingAllCategories<T>(ContentReference rootPage, CategoryList categories,int pageNumber=1,int pageSize=30, string language = null) where T : PageData
        {
            var result = _client.Search<T>()
                .CurrentlyPublished()
                .FilterOnCurrentSite()
                .FilterForVisitor(language)
                .Filter(x => x.Ancestors().Match(rootPage.ID.ToString()))
               
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .StaticallyCacheFor(TimeSpan.FromHours(1))
                .GetContentResult();

            return result;
        }

        /// <summary>
        /// Find pages with any of the matching categories
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootPage"></param>
        /// <param name="categories"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public IContentResult<T> FindPagesMatchingAnyCategory<T>(ContentReference rootPage, CategoryList categories,int pageNumber = 1, int pageSize = 30, string language = null) where T : PageData
        {
            var result = _client.Search<T>()
                .CurrentlyPublished()
                .FilterOnCurrentSite()
                .FilterForVisitor(language)
                .Filter(x => x.Ancestors().Match(rootPage.ID.ToString()))
               
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .StaticallyCacheFor(TimeSpan.FromHours(1))
                .GetContentResult();

            return result;
        }


        /// <summary>
        /// Find pages with any of the matching categories
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootPage"></param>
        /// <param name="categories"></param>
        /// <param name="excludeReferences"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public IContentResult<T> FindArticlesMatchingAnyCategory<T>(ContentReference rootPage, CategoryList categories,List<ContentReference> excludeReferences, int pageNumber = 1, int pageSize = 30, string language = null) where T : SitePageData
        {
            var result = _client.Search<T>()
                .CurrentlyPublished()
                .FilterOnCurrentSite()
                .FilterForVisitor(language)
                .Filter(x => x.Ancestors().Match(rootPage.ID.ToString()))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .StaticallyCacheFor(TimeSpan.FromHours(1))
               
                .GetContentResult();

            return result;
        }


        /// <summary>
        /// Find recent pages
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootPage"></param>
        /// <param name="categories"></param>
        /// <param name="itemToTake"></param>
        /// <returns></returns>
        public IContentResult<T> FindRecentPage<T>(ContentReference rootPage, CategoryList categories, int itemToTake) where T : PageData
        {
            var result = _client.Search<T>()
                .CurrentlyPublished()
                .Filter(x => x.Ancestors().Match(rootPage.ID.ToString()))
              
                .StaticallyCacheFor(TimeSpan.FromHours(1))
              
                .Take(itemToTake)
                .GetContentResult();
               
            return result;
        }

        /// <summary>
        /// Find related articles
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootPage"></param>
        /// <param name="categories"></param>
        /// <param name="excludeContentReference"></param>
        /// <param name="itemToTake"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public IContentResult<T> FindRelatedArticles<T>(ContentReference rootPage, CategoryList categories,ContentReference excludeContentReference, int itemToTake=3, string language = null) where T : SitePageData
        {
            //Build the query
            var query = _client.Search<T>()
                .CurrentlyPublished()
                .Filter(x => x.Ancestors().Match(rootPage.ID.ToString()))
                .Filter(x=>!x.ContentLink.Match(excludeContentReference))
                .StaticallyCacheFor(TimeSpan.FromHours(1))
                .Take(itemToTake);

            //Add Display Category match filter
           

            //Get the result
            var result = query
                .Take(itemToTake)
                .GetContentResult();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rootPage"></param>
        /// <param name="categories"></param>
        /// <param name="excludeContentReference"></param>
        /// <param name="itemToTake"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public IContentResult<T> FindPopularArticles<T>(ContentReference rootPage, CategoryList categories, ContentReference excludeContentReference, int itemToTake=4, string language = null) where T : SitePageData
        {
            //Build the query
            var query = _client.Search<T>()
                .CurrentlyPublished()
                .Filter(x => x.Ancestors().Match(rootPage.ID.ToString()))
                .Filter(x => !x.ContentLink.Match(excludeContentReference))
                .StaticallyCacheFor(TimeSpan.FromHours(1))
                .Take(itemToTake);

            //Add Display Category match filter
           

            //Add Display Category match filter
            query = PageViewsFilter<T>(query, categories);

            //Get the result
            var result = query
                .OrderByDescending(x=>x.GetPageViews().TotalViews)
                .Take(itemToTake)
                .GetContentResult();
            return result;
        }

        /// <summary>
        /// PageView Filter for EPi FInd Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="search"></param>
        /// <param name="categoryList"></param>
        /// <returns></returns>
        private ITypeSearch<T> PageViewsFilter<T>(
            ITypeSearch<T> search, CategoryList categoryList) where T : SitePageData
        {
            var pageViewFilter = _client.BuildFilter<T>();
            pageViewFilter = pageViewFilter.And(x => x.GetPageViews().TotalViews.GreaterThan(0));
            pageViewFilter = pageViewFilter.And(x => x.GetPageViews().LastViewDateTime.InRange(DateTime.Now.AddDays(-30),DateTime.Now));
            search = search.Filter(pageViewFilter);
            return search;
        }

      


      
      
     
    }
}
