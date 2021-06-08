using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
namespace WorkerRole
{
        public class GuestBookDataSource
        {

            private static CloudStorageAccount storageAccount;

            static GuestBookDataSource()
            {
                storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = tableClient.GetTableReference("GuestBookEntry");

                table.CreateIfNotExistsAsync();
            }

            public GuestBookDataSource() { }

            public IEnumerable<GuestBookEntry> GetGuestBookEntries()
            {
                CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
                CloudTable table = cloudTableClient.GetTableReference("GuestBookEntry");

                TableQuery<GuestBookEntry> query =
                    (new TableQuery<GuestBookEntry>()).Where(
                        TableQuery.GenerateFilterCondition("PartitionKey",
                        QueryComparisons.Equal,
                        DateTime.UtcNow.ToString("MMddyyyy")));

                var result = table.ExecuteQuery(query).ToList<GuestBookEntry>(); ;
                return result;
            }


            public void AddGuestBookEntry(GuestBookEntry newItem)
            {
                CloudTable table = storageAccount.CreateCloudTableClient().GetTableReference("GuestBookEntry");
                table.Execute(TableOperation.Insert(newItem));
            }


            public void UpdateImageThumbnail(string partitionKey, string rowKey, string thumbnailUrl)
            {
                CloudTable table = storageAccount.CreateCloudTableClient().GetTableReference("GuestBookEntry");
                GuestBookEntry entry =
                    table.Execute(TableOperation.Retrieve<GuestBookEntry>(partitionKey, rowKey))
                    .Result as GuestBookEntry;
                entry.ThumbnailUrl = thumbnailUrl;
                table.Execute(TableOperation.Replace(entry));
            }
        }
}
