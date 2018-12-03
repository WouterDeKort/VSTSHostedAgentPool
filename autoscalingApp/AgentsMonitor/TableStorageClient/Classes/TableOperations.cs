using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using TableStorageClient.Interfaces;

namespace TableStorageClient.Classes
{
    public class TableOperations<T>:ITableOperations<T> where T: TableEntity
    {
        public CloudTable Table { get; set; }

        public TableOperations(string tableName, string connectionString)
        {
            Table = CommonTasks.GetOrCreateTableAsync(tableName, connectionString).Result;
        }

        public async Task InsertOrReplaceEntityAsync(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);
            await Table.ExecuteAsync(insertOrReplaceOperation);
        }
    }
}