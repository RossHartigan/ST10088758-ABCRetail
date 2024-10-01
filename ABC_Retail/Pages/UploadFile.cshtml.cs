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
        private readonly string _functionUrl = "https://abc-retail-functions.azurewebsites.net/api/UploadContract?code=uR-5HDSvD_AD30nhUD2SHOIfVjCXxhOG4x53Ef_wwIBEAzFuTAkayg%3D%3D";

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
