using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using EPiServer.Data;
using EPiServer.Data.Dynamic;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Microsoft.SqlServer.Server;
using PageViewCount.DataStore;

namespace AlloyTemplates.PageViewCount.Initializations
{
    [InitializableModule]
    [ModuleDependency(typeof(DataInitialization))]
    public class CustomBigTableInitializer : IInitializableModule
    {
        private Injected<IDatabaseExecutor> DatabaseHandler { get; set; }
        private Injected<DataAccessOptions> DataaccessOptions { get; set; }
     
        private const string StoreName = "InsightPageViewsDataStore";
        private const string TableName = "tblInsightPageViewsDataStoreBigTable";

        private const string HistoryStoreName = "InsightPageViewsHistoryDataStore";
        private const string HistoryTableName = "tblInsightPageViewsDataHistoryStoreBigTable";
        private static readonly Type ObjectType = typeof(CustomTableInsightPageViewsData);
        private static readonly Type HistoryObjectType = typeof(CustomTableInsightPageViewsDataHistory);

        private static readonly string PageViewsDataSqlCreateColumns =
            @"[PageId] int null,  
             [ViewsCount] int null,
             [LastViewdDateTime] datetime null,
             [LanguageCode] nvarchar(10) null";

        private static readonly IEnumerable<string> PageViewsDataSqlCreateIndexes = new[] { "PageId" };


        public void Initialize(InitializationEngine initializationEngine)
        {
            CreatePageViewTable(TableName);
            CreatePageViewTable(HistoryTableName);
            AssignTableToStore(ObjectType, StoreName, TableName);
            AssignTableToStore(HistoryObjectType, HistoryStoreName, HistoryTableName);
            ObjectType.GetOrCreateStore();
            HistoryObjectType.GetOrCreateStore();
        }


        public void Uninitialize(InitializationEngine initializationEngine)
        {
        }

        private void CreatePageViewTable(string tableName)
        {

            //Create InsightPageViewsDataStore  and InsightPageViewsHistoryDataStore

            var tableUpdater = new DynamicDataStoreSqlProvider();
            string sqlCreateTable = tableUpdater.GetCreateTableSql(
                tableName,
                PageViewsDataSqlCreateColumns,
                StoreName,
                PageViewsDataSqlCreateIndexes);

           // DatabaseHandler.Service.CreateCommand(sqlCreateTable, CommandType.Text);

            using (var connection = DatabaseHandler.Service.DbFactory.CreateConnection())
            {
                if (connection != null)
                {
                    connection.ConnectionString = ConfigurationManager.ConnectionStrings[0].ToString();

                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sqlCreateTable;
                        command.ExecuteNonQuery();
                    }

                    connection.Close();

                    
                }
            }

            //using (var connection = new SqlConnection(DatabaseHandler.Service..ConnectionString))
            //{
            //    connection.Open();

            //    using (SqlCommand command = connection.CreateCommand())
            //    {
            //        command.CommandText = sqlCreateTable;
            //        command.ExecuteNonQuery();
            //    }
            //}


        }

        private void AssignTableToStore(Type objectType,string storeName, string tableName)
        {
            if (GlobalTypeToStoreMap.Instance.ContainsKey(objectType))
            {
                GlobalTypeToStoreMap.Instance.Remove(objectType);
            }

            GlobalTypeToStoreMap.Instance.Add(objectType, storeName);

            var parameters = new StoreDefinitionParameters
            {
                TableName = tableName,
            };

            GlobalStoreDefinitionParametersMap.Instance.Add(storeName, parameters);
        }
    }
}
