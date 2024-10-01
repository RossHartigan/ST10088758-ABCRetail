using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Logging;

namespace ABC_Retail.Pages
{
    public class UploadFileModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UploadFileModel> _logger;
        // Use localhost during local testing. Replace with Azure Function URL in production.
        private readonly string _functionUrl = "http://localhost:7071/api/UploadContract";  // For production: use the Azure Function URL here

        public UploadFileModel(IHttpClientFactory httpClientFactory, ILogger<UploadFileModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public IFormFile File { get; set; }

        public string UploadSuccess { get; set; }
        public string UploadError { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (File == null || File.Length == 0)
            {
                UploadError = "No file selected.";
                return Page();
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Copy the uploaded file content into memory
                    await File.CopyToAsync(memoryStream);
                    var content = new ByteArrayContent(memoryStream.ToArray());

                    // Create a HttpClient to send the request
                    var httpClient = _httpClientFactory.CreateClient();
                    var formData = new MultipartFormDataContent();

                    formData.Add(content, "File", File.FileName);

                    // Post the form data to the Azure Function endpoint
                    var response = await httpClient.PostAsync(_functionUrl, formData);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"Azure Function Response: {responseContent}");

                    // Check the status code of the response
                    if (!response.IsSuccessStatusCode)
                    {
                        UploadError = $"Error uploading file: {responseContent}";
                        return Page();
                    }

                    UploadSuccess = "File uploaded successfully!";
                }
            }
            catch (Exception ex)
            {
                // Log and show the error in case of exceptions
                UploadError = $"Error uploading file: {ex.Message}";
            }

            return Page();
        }
    }
}
