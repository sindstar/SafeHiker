﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Storage.impl
{
    public class AzureDataStorageService : IDataStorageService
    {
        public CloudTableClient Client { get; set; }
        public CloudTable Table { get; set; }

        public AzureDataStorageService(string storageAccountName, string tableName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[storageAccountName].ConnectionString;
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            Client = storageAccount.CreateCloudTableClient();
            InitializeTable(tableName);
        }

        private void InitializeTable(string tableName)
        {
            CloudTable table = Client.GetTableReference(tableName);
            table.CreateIfNotExists();
            Table = table;
        }

        public bool InsertEntity<T>(T entity)
        {
            bool result = false;
            var tableEntity = entity as TableEntity;
            if (!HasEntity(tableEntity))
            {
                result = InsertEntity(tableEntity);
            }

            return result;
        }

        public bool UpdateEntity<T>(T entity)
        {
            var tableEntity = entity as TableEntity;
            return UpdateEntity(tableEntity);
        }

        public bool DeleteEntity<T>(T entity)
        {
            bool result = false;
            var tableEntity = entity as TableEntity;
            if (HasEntity(tableEntity))
            {
                result = DeleteEntity(tableEntity);
            }

            return result;
        }

        public T GetEntity<T>(T entity) where T : TableEntity
        {
            if (Table.Exists())
            {
                TableOperation op = TableOperation.Retrieve<T>(entity.PartitionKey, entity.RowKey);
                TableResult retrivedResult = Table.Execute(op);
                return (T)retrivedResult.Result;
            }
            return null;
        }

        public List<T> GetEntities<T>(string partitionKey) where T : TableEntity, new()
        {
            if (Table.Exists())
            {
                var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));
                return Table.ExecuteQuery(query).ToList();
            }
            return null;
        }

        public List<T> GetAllTableEntities<T>() where T : TableEntity, new()
        {
            if (Table.Exists())
            {
                return Table.ExecuteQuery(new TableQuery<T>()).ToList();
            }
            return null;
        }

        private bool HasEntity(TableEntity tableEntity)
        {
            var entity = GetEntity(tableEntity);
            return (entity != null);
        }

        public bool HasEntity<T>(string partitionKey, string rowKey = null) where T : TableEntity
        {
            T result = default(T);
            if (rowKey == null)
            {
                rowKey = partitionKey;
            }
            if (Table.Exists())
            {
                TableOperation op = TableOperation.Retrieve<T>(partitionKey, rowKey);
                TableResult retrivedResult = Table.Execute(op);
                result = (T)retrivedResult.Result;
            }
            return (result != null);
        }

        private bool InsertEntity(TableEntity e)
        {
            TableOperation op = TableOperation.Insert(e);
            var entity = Table.Execute(op);
            return (entity.Result != null);
        }

        private bool UpdateEntity(TableEntity e)
        {
            TableOperation op = TableOperation.Replace(e);
            var entity = Table.Execute(op);
            return (entity.Result != null);
        }

        private bool DeleteEntity(TableEntity e)
        {
            e.ETag = "*";
            TableOperation op = TableOperation.Delete(e);
            TableResult entity = Table.Execute(op);
            return (entity.Result != null);
        }

        public void DeleteTable()
        {
            Table.DeleteIfExists();
        }
    }
}