using EPiServer.Data;
using EPiServer.Data.Dynamic;
using System;

namespace PageViewCount.DataStore
{
    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class CustomTableInsightPageViewsData
    {
        public Identity Id { get; set; }

        [EPiServerDataIndex]
        [EPiServerDataColumn(ColumnName = "PageId")]
        public int PageId { get; set; }

        [EPiServerDataColumn(ColumnName = "LanguageCode")]
        public string LanguageCode { get; set; }

        [EPiServerDataColumn(ColumnName = "ViewsCount")]
        public int ViewsCount { get; set; }

        [EPiServerDataColumn(ColumnName = "LastViewdDateTime")]
        public DateTime LastViewdDateTime { get; set; }

       
    }


    [EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
    public class CustomTableInsightPageViewsDataHistory
    {
        public Identity Id { get; set; }

        [EPiServerDataIndex]
        [EPiServerDataColumn(ColumnName = "PageId")]
        public int PageId { get; set; }

        [EPiServerDataColumn(ColumnName = "LanguageCode")]
        public string LanguageCode { get; set; }

        [EPiServerDataColumn(ColumnName = "ViewsCount")]
        public int ViewsCount { get; set; }

        [EPiServerDataColumn(ColumnName = "LastViewdDateTime")]
        public DateTime LastViewdDateTime { get; set; }


    }
}
