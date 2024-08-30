using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace ABC_Retail.Pages
{
    public class UploadQueueMessageModel : PageModel
    {
        private readonly QueueServiceClient _queueServiceClient;

        public UploadQueueMessageModel(QueueServiceClient queueServiceClient)
        {
            _queueServiceClient = queueServiceClient;
        }

        [BindProperty]
        public string Message { get; set; }

        public string UploadSuccess { get; set; }
        public string UploadError { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Message))
            {
                UploadError = "Message cannot be empty.";
                return Page();
            }

            try
            {
                var queueClient = _queueServiceClient.GetQueueClient("order-processing");
                await queueClient.CreateIfNotExistsAsync();

                // Send the message to the queue
                await queueClient.SendMessageAsync(Message);

                UploadSuccess = "Message uploaded successfully.";
            }
            catch (Exception ex)
            {
                UploadError = $"Error uploading message: {ex.Message}";
            }

            return Page();
        }
    }
}
