using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure;
using Microsoft.Azure;
using System.IO;
using WorkerRole;

namespace WebRole1
{
    public partial class Default : System.Web.UI.Page
    {
        private static bool storage = false;
        private static object gate = new object();
        private static CloudBlobClient cloudBlobClient;
        private static CloudQueueClient cloudQueueClient;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                this.Timer1.Enabled = true;
            }

        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (this.FileUpload1.HasFile)
            {
                this.initializeStorage();
                string blobName = string.Format("guestbook/image_{0}{1}", Guid.NewGuid().ToString(), Path.GetExtension(this.FileUpload1.FileName));
                CloudBlockBlob blob = cloudBlobClient.GetContainerReference("guestbook").GetBlockBlobReference(blobName); //TODO: reference??
                blob.Properties.ContentType = this.FileUpload1.PostedFile.ContentType;
                blob.UploadFromStream(this.FileUpload1.FileContent);
                System.Diagnostics.Trace.TraceInformation("Uploaded image {0} to blob storage as a {1} file. ", this.FileUpload1.FileName, blobName);

                GuestBookEntry entry = new GuestBookEntry()
                {
                    GuestName = this.TextBox2.Text,
                    Message = this.TextBox1.Text,
                    PhotoUrl = blob.Uri.ToString(),
                    ThumbnailUrl = blob.Uri.ToString()
                };
                GuestBookDataSource dataSource1 = new GuestBookDataSource();
                dataSource1.AddGuestBookEntry(entry);
                System.Diagnostics.Trace.TraceInformation("Added entry {0} {1} in table storage for guest {2} ",
                    entry.PartitionKey, entry.RowKey, entry.GuestName);

                //Queue

                var queue = cloudQueueClient.GetQueueReference("queue");
                var msg = new CloudQueueMessage(string.Format("{0},{1},{2}", blobName, entry.PartitionKey, entry.RowKey));
                queue.AddMessage(msg);
                System.Diagnostics.Trace.TraceInformation("Queued message to process blob {0} ", blobName);



            }
            this.TextBox1.Text = string.Empty;
            this.TextBox2.Text = string.Empty;

            this.DataList1.DataBind();
        }
        protected void Timer1_Tick(object sender, EventArgs e)
        {
            this.DataList1.DataBind();
        }

        private void initializeStorage()
        {
            if (storage)
            {
                return;
            }
            lock (gate)
            {
                if (storage)
                {
                    return;
                }
            }
            try
            {
                var storageAcc = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));
                cloudBlobClient = storageAcc.CreateCloudBlobClient();
                CloudBlobContainer container = cloudBlobClient.GetContainerReference("guestbook");
                container.CreateIfNotExists();

                var permissions = container.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(permissions);

                cloudQueueClient = storageAcc.CreateCloudQueueClient();
                CloudQueue queue = cloudQueueClient.GetQueueReference("queue");
                queue.CreateIfNotExists();
            }
            catch (System.Net.WebException)
            {
                throw new System.Net.WebException("Error");

            }

            storage = true;
        }

        protected void ObjectDataSource1_Selecting(object sender, ObjectDataSourceSelectingEventArgs e)
        {

        }
    }
}