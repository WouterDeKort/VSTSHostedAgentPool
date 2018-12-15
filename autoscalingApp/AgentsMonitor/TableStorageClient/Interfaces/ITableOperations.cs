using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace TableStorageClient.Interfaces
{
    public interface ITableOperations<in T> where T : TableEntity
    {
        Task InsertOrReplaceEntityAsync(T entity);
    }
}