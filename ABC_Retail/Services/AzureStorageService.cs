namespace ABC_Retail.Services
{
    using Azure;
    using Azure.Data.Tables;
    using Azure.Storage.Blobs;
    using Azure.Storage.Blobs.Models;
    using Azure.Storage.Queues;
    using Azure.Storage.Files.Shares;

    public class AzureStorageService
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly QueueServiceClient _queueServiceClient;
        private readonly ShareServiceClient _shareServiceClient;

        public AzureStorageService(string connectionString)
        {
            _tableServiceClient = new TableServiceClient(connectionString);
            _blobServiceClient = new BlobServiceClient(connectionString);
            _queueServiceClient = new QueueServiceClient(connectionString);
            _shareServiceClient = new ShareServiceClient(connectionString);
        }

        public async Task<TableClient> GetTableClientAsync(string tableName)
        {
            var tableClient = _tableServiceClient.GetTableClient(tableName);
            await tableClient.CreateIfNotExistsAsync();
            return tableClient;
        }

        public async Task AddCustomerProfileAsync(CustomerProfile profile)
        {
            var tableClient = await GetTableClientAsync("CustomerProfiles");
            await tableClient.AddEntityAsync(profile);
        }

        public async Task AddProductInformationAsync(ProductInformation product)
        {
            var tableClient = await GetTableClientAsync("ProductInformation");
            await tableClient.AddEntityAsync(product);
        }

        public async Task UploadImageAsync(string blobName, Stream content)
        {
            const string containerName = "product-images"; // Use your existing container name

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

            // Ensure container exists (it won't be created if it already exists)
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            // Upload the file
            await blobClient.UploadAsync(content, overwrite: true);

            // Log successful upload
            Console.WriteLine($"File uploaded to {blobName} in container {containerName}.");
        }

        public async Task AddMessageToQueueAsync(string queueName, string message)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(message);
        }

        public async Task<string> ProcessQueueMessageAsync(string queueName)
        {
            var queueClient = _queueServiceClient.GetQueueClient(queueName);
            var message = await queueClient.ReceiveMessageAsync();
            if (message.Value != null)
            {
                await queueClient.DeleteMessageAsync(message.Value.MessageId, message.Value.PopReceipt);
                return message.Value.MessageText;
            }
            return null;
        }

        public async Task UploadFileAsync(string shareName, string directoryName, string fileName, Stream content)
        {
            var shareClient = _shareServiceClient.GetShareClient(shareName);
            await shareClient.CreateIfNotExistsAsync();
            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            await directoryClient.CreateIfNotExistsAsync();
            var fileClient = directoryClient.GetFileClient(fileName);
            await fileClient.CreateAsync(content.Length);
            await fileClient.UploadRangeAsync(new HttpRange(0, content.Length), content);
        }

        public async Task<Stream> DownloadFileAsync(string shareName, string directoryName, string fileName)
        {
            var shareClient = _shareServiceClient.GetShareClient(shareName);
            var directoryClient = shareClient.GetDirectoryClient(directoryName);
            var fileClient = directoryClient.GetFileClient(fileName);
            var downloadResponse = await fileClient.DownloadAsync();
            return downloadResponse.Value.Content;
        }

    }

    public class CustomerProfile : ITableEntity
    {
        public string PartitionKey { get; set; } = "Customer";
        public string RowKey { get; set; } // This could be the customer ID
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }

    public class ProductInformation : ITableEntity
    {
        public string PartitionKey { get; set; } = "Product";
        public string RowKey { get; set; } // This could be the product ID
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }


}
