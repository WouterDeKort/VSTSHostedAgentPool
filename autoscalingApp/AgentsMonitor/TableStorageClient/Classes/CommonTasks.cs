using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace TableStorageClient.Classes
{
    public static class CommonTasks
    {
        public static async Task<CloudTable> GetOrCreateTableAsync(string tableName, string storageConnectionString)
        {
            var storageAccount = CreateStorageAccountFromConnectionString(storageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            var table = tableClient.GetTableReference(tableName);

            try
            {
                //if table does not exist - create it 
                await table.CreateIfNotExistsAsync();
            }
            catch (Exception ex)
            {
                //empty catch is useful for debugging
                throw;
            }

            return table;
        }

        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            return CloudStorageAccount.Parse(storageConnectionString);
        }
    }
}