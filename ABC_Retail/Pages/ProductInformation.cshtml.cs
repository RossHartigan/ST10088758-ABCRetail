using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace ABC_Retail.Pages
{
    public class ProductInformationModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductInformationModel> _logger;
        private readonly string _functionUrl = "https://abc-retail-functions.azurewebsites.net/api/AddProductToTable?code=DLqGJ-oRTyaxYDmv35gipu2NnALi15_eday6YSxvixRkAzFu-y6dUg%3D%3D";

        public ProductInformationModel(IHttpClientFactory httpClientFactory, ILogger<ProductInformationModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        [Required]
        public string ProductId { get; set; }

        [BindProperty]
        [Required]
        public string Name { get; set; }

        [BindProperty]
        [Required]
        public string Description { get; set; }

        [BindProperty]
        [Required]
        public double Price { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var product = new
            {
                PartitionKey = ProductId,
                Name = Name,
                Description = Description,
                Price = Price
            };

            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_functionUrl, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Azure Function Response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to add product.");
                ModelState.AddModelError(string.Empty, "Failed to add product.");
                return Page();
            }

            return RedirectToPage("/Index");
        }
    }
}
