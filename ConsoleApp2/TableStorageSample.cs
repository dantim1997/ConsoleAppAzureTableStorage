using ConsoleAppAzureTableStorage.Domain;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace AzureCustomerTable
{
    public class TableStorageSample : ITableStorage<TableEntity>
    {
        public CloudTable _CloudTable;
        public async Task RunDemo()
        {
            CloudStorageAccount cloudStorageAccountc = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);//// oh dear me...

            CloudTableClient tableClient = cloudStorageAccountc.CreateCloudTableClient();

            string tableName = "DEMO3";
            _CloudTable = tableClient.GetTableReference(tableName);

            await CreateNewTable(_CloudTable);
            await InsertRecordToTable(_CloudTable, new Customer());
            var custemers = GetAllCustomers(_CloudTable);
            //await UpdateRecordInTable(_CloudTable);
            await InsertOrUpdate(new Customer() { CustomerId = "69", customerFirstName = "tim", customerLastName = "test", enumCustomerType = EnumCustomerType.gold });

            var allcustemers = GetAllCustomers(_CloudTable);
            // await DeleteRecordinTable(cloudTable);
        }

        public List<Customer> GetAllCustomers(CloudTable cloudTable)
        {
            List<Customer> lstCus = new List<Customer>();

            TableQuery<Customer> query = new TableQuery<Customer>()
                   .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "124345"));

            foreach (Customer customer in cloudTable.ExecuteQuerySegmentedAsync(query, null).Result)
            {
                lstCus.Add(customer);
            }
            return lstCus;
        }


        public static async Task CreateNewTable(CloudTable table)
        {
            try
            {
                var result = await table.CreateIfNotExistsAsync();
            }
            catch (StorageException ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
        }

        public static async Task<TableEntity> InsertRecordToTable(CloudTable table, Customer customer)
        {

            Customer customerEntity = new Customer("124345", "Pieterse", "Sjaak", EnumCustomerType.gold);

            try
            {
                TableOperation tableOperation = TableOperation.Insert(customerEntity);
                TableResult result = await table.ExecuteAsync(tableOperation);
                return result.Result as TableEntity;
            }
            catch (StorageException e)
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }
        private static async Task UpdateRecordInTable(CloudTable table)
        {
            string customerID = "124345";
            string customerLastName = "Pieterse";
            string customerFirstName = "Sjaak";

            Customer customerEntity = await RetrieveRecord(table, customerID, customerLastName + customerFirstName);
            if (customerEntity is not null)
            {
                customerEntity.customerFirstName = "Peters";
                TableOperation tableOperation = TableOperation.Replace(customerEntity);
                var result = await table.ExecuteAsync(tableOperation);
            }
        }
        public static async Task DeleteRecordinTable(CloudTable table)
        {
            string customerID = "124345";
            string customerLastName = "Pieterse";
            string customerFirstName = "Sjaak";

            Customer customerEntity = await RetrieveRecord(table, customerID, customerLastName + customerFirstName);
            if (customerEntity is not null)
            {
                TableOperation tableOperation = TableOperation.Delete(customerEntity);
                var result = await table.ExecuteAsync(tableOperation);
            }
        }
        public static async Task<Customer> RetrieveRecord(CloudTable table, string partitionKey, string rowKey)
        {
            TableOperation tableOperation = TableOperation.Retrieve<Customer>(partitionKey, rowKey);
            TableResult tableResult = await table.ExecuteAsync(tableOperation);
            return tableResult.Result as Customer;
        }

        public async Task InsertOrUpdate(TableEntity Entity)
        {
            if (Entity.PartitionKey is not null)
            {
                ((Customer)Entity).customerFirstName = "Peters";
                TableOperation tableOperation = TableOperation.Replace(Entity);
                var result = await _CloudTable.ExecuteAsync(tableOperation);
            }
            else
            {
                Customer customerEntity2 = new Customer("69", "Horst", "Wans", EnumCustomerType.lead);

                try
                {
                    TableOperation tableOperation = TableOperation.Insert(customerEntity2);
                    TableResult result = await _CloudTable.ExecuteAsync(tableOperation);
                    Customer insertedCustomer = result.Result as Customer;
                }
                catch (StorageException e)
                {
                    Console.WriteLine(e.RequestInformation);
                }


                Entity.PartitionKey = "124345";
                Entity.Timestamp = DateTimeOffset.Now;
                try
                {
                    //TableOperation tableOperation = TableOperation.Insert(Entity);
                    //try
                    //{
                    //    TableResult result = await _CloudTable.ExecuteAsync(tableOperation);
                    //    Customer insertedCustomer = result.Result as Customer;

                    //}
                    //catch (Exception e)
                    //{

                    //    throw;
                    //}
                }
                catch (StorageException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}
