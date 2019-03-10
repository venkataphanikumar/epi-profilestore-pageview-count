using System.Collections.Generic;
using System.Text;

namespace PageViewCount.DataStore
{
    public class DynamicDataStoreSqlProvider
    {
        private const string CreateTableSql = @"if OBJECT_ID('dbo.{0}', 'U') is null 
            create table [dbo].[{0}] 
            ([pkId] bigint not null, 
            [Row] int not null default(1) constraint CH_{0} check ([Row]>=1), 
            [StoreName] nvarchar(128) not null, 
            [ItemType] nvarchar(512) not null, 
            {1}
            constraint [PK_{0}] primary key clustered([pkId],[Row]), 
            constraint [FK_{0}_tblBigTableIdentity] foreign key ([pkId])
            references [tblBigTableIdentity]([pkId])); ";

        public string GetCreateTableSql(string tableName, string sqlTableColumns, string storageName, IEnumerable<string> indexColumns)
        {
            return string.Format(CreateTableSql, tableName, sqlTableColumns) + GetCreateIndexSql(tableName, indexColumns);
        }

        private string GetCreateIndexSql(string tableName, IEnumerable<string> indexColumns)
        {
            var stringBuilder = new StringBuilder();
            foreach (string indexColumn in indexColumns)
            {
                stringBuilder.Append(GetIndexCreationQuery(tableName, indexColumn));
            }

            return stringBuilder.ToString();
        }

        private string GetIndexCreationQuery(string tableStorageName, string columnName)
        {
            return GetIndexCreationQueryWithReadyColumnsNames(tableStorageName, columnName);
        }

        private string GetIndexCreationQueryWithReadyColumnsNames(string tableStorageName, string columnNamesForIndexName)
        {
            return string.Format(
                @" IF NOT EXISTS(SELECT * FROM sys.indexes WHERE Name = 'IDX_{0}_{1}')
                    CREATE NONCLUSTERED INDEX [IDX_{0}_{1}] 
                    ON [dbo].[{0}]([{1}]) 
                    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON); ",
                tableStorageName,
                columnNamesForIndexName);
        }
    }
}
