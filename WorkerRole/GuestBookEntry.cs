using System;
using System.Data.Services.Common;


namespace WorkerRole
{
    [DataServiceKey("PartitionKey", "RowKey")]
    public class GuestBookEntry : Microsoft.WindowsAzure.Storage.Table.TableEntity
    {

        public GuestBookEntry()
        {
            PartitionKey = DateTime.UtcNow.ToString("MMddyyy");

            RowKey = string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid());
        }

        public string Message { get; set; }

        public string GuestName { get; set; }

        public string PhotoUrl { get; set; }

        public string ThumbnailUrl { get; set; }
    }
}