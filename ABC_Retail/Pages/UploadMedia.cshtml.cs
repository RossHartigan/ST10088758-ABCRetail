using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;


namespace ABC_Retail.Pages
{
    public class UploadMediaModel : PageModel
    {
        private readonly BlobServiceClient _blobServiceClient;

        public UploadMediaModel(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        [BindProperty]
        public IFormFile File { get; set; }

        [BindProperty]
        public string ContainerName { get; set; } = "product-images";

        public string UploadSuccess { get; set; }
        public string UploadError { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (File == null || File.Length == 0)
            {
                UploadError = "No file selected.";
                return Page();
            }

            try
            {
                using (var stream = File.OpenReadStream())
                {
                    await UploadImageAsync(File.FileName, stream);
                }
                UploadSuccess = "File uploaded successfully.";
            }
            catch (Exception ex)
            {
                UploadError = $"Error uploading file: {ex.Message}";
            }

            return RedirectToPage("/Index");
        }

        private async Task UploadImageAsync(string blobName, Stream content)
        {
            const string containerName = "product-images";

            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(); // Ensure container exists

            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(content, overwrite: true);
        }
    }
}
