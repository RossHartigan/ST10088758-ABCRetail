using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Files.Shares;
using Microsoft.Extensions.Configuration;
using Azure;

namespace ABC_Retail.Pages
{
    public class UploadFileModel : PageModel
    {
        private readonly ShareServiceClient _shareServiceClient;

        public UploadFileModel(ShareServiceClient shareServiceClient)
        {
            _shareServiceClient = shareServiceClient;
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
                var shareClient = _shareServiceClient.GetShareClient("contracts-and-logs");
                var directoryClient = shareClient.GetDirectoryClient("");
                var fileClient = directoryClient.GetFileClient(File.FileName);

                using (var stream = File.OpenReadStream())
                {
                    await fileClient.CreateAsync(stream.Length);
                    await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);
                }

                UploadSuccess = "File uploaded successfully!";
            }
            catch (Exception ex)
            {
                UploadError = $"Error uploading file: {ex.Message}";
            }

            return Page();
        }
    }
}
