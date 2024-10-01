using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ABC_Retail.Pages
{
    public class UploadMediaModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UploadMediaModel> _logger;
        private readonly string _functionUrl = "http://localhost:7071/api/UploadProductImage";  // Change to Azure URL in production

        public UploadMediaModel(IHttpClientFactory httpClientFactory, ILogger<UploadMediaModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public IFormFile Image { get; set; } = default!; // Match this name with the expected field name in the Azure Function

        [BindProperty]
        public string ContainerName { get; set; } = "product-images";

        public string UploadSuccess { get; set; } = string.Empty;
        public string UploadError { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            if (Image == null || Image.Length == 0)  // Ensure we check the correct property name
            {
                UploadError = "No file selected.";
                return Page();
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Image.CopyToAsync(memoryStream);
                    var content = new ByteArrayContent(memoryStream.ToArray());

                    var httpClient = _httpClientFactory.CreateClient();
                    var formData = new MultipartFormDataContent();

                    formData.Add(content, "image", Image.FileName);

                    var response = await httpClient.PostAsync(_functionUrl, formData);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation($"Azure Function Response: {responseContent}");

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
                UploadError = $"Error uploading file: {ex.Message}";
            }

            return Page();
        }
    }
}
