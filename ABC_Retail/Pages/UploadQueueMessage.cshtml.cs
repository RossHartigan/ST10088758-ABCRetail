using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ABC_Retail.Pages
{
    public class UploadQueueMessageModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UploadQueueMessageModel> _logger;
        private readonly string _functionUrl = "http://localhost:7071/api/QueueOrder";

        public UploadQueueMessageModel(IHttpClientFactory httpClientFactory, ILogger<UploadQueueMessageModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public string Message { get; set; }

        public string UploadSuccess { get; set; }
        public string UploadError { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrEmpty(Message))
            {
                UploadError = "Message cannot be empty.";
                return Page();
            }

            var message = new { Message };

            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_functionUrl, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"Azure Function Response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                UploadError = $"Error uploading message: {responseContent}";
                return Page();
            }

            UploadSuccess = "Message uploaded successfully.";
            return Page();
        }
    }
}
