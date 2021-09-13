using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureCustomerTable
{
    public interface ITableStorage<T> where T: TableEntity, new()
    {
        Task InsertOrUpdate(T Entity);
    }
}
