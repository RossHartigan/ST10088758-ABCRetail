using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ABC_Retail.Pages
{
    public class UploadMediaModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<UploadMediaModel> _logger;
        private readonly string _functionUrl = "https://abc-retail-functions.azurewebsites.net/api/UploadProductImage?code=skMdb_aG_-RXHVsiL0kKfJ2OM18D1dj_PBTftTlhuEc3AzFu3Y0gqg%3D%3D";

        public UploadMediaModel(IHttpClientFactory httpClientFactory, ILogger<UploadMediaModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        public new IFormFile File { get; set; } = default!;

        [BindProperty]
        public string ContainerName { get; set; } = "product-images";

        public string UploadSuccess { get; set; } = string.Empty;
        public string UploadError { get; set; } = string.Empty;


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
                    await File.CopyToAsync(memoryStream);
                    var content = new ByteArrayContent(memoryStream.ToArray());

                    var httpClient = _httpClientFactory.CreateClient();
                    var formData = new MultipartFormDataContent();
                    formData.Add(content, "file", File.FileName);

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
