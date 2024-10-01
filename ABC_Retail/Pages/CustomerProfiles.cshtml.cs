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
    public class CustomerProfilesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CustomerProfilesModel> _logger;
        private readonly string _functionUrl = "https://abc-retail-functions.azurewebsites.net/api/AddCustomerToTable?code=WHI98kAh8q6iq7SdWu10Vg6rSVSFJex51aqIrOHgpybTAzFuGUwq8w%3D%3D";

        public CustomerProfilesModel(IHttpClientFactory httpClientFactory, ILogger<CustomerProfilesModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [BindProperty]
        [Required]
        public string CustomerId { get; set; }

        [BindProperty]
        [Required]
        public string Name { get; set; }

        [BindProperty]
        [Required]
        public string Email { get; set; }

        [BindProperty]
        [Required]
        public string Phone { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Form submitted with values: CustomerId={CustomerId}, Name={Name}, Email={Email}, Phone={Phone}", CustomerId, Name, Email, Phone);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model state is invalid.");
                return Page();
            }

            // Create an object to send to the Azure Function
            var customer = new
            {
                PartitionKey = CustomerId,
                Name = Name,
                Email = Email,
                Phone = Phone
            };

            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");

            // Send POST request to Azure Function
            var response = await httpClient.PostAsync(_functionUrl, jsonContent);

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Azure Function Response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to add customer. Status Code: {response.StatusCode}, Response: {responseContent}");
                ModelState.AddModelError(string.Empty, "Failed to add customer.");
                return Page();
            }

            _logger.LogInformation("Customer added successfully.");
            return RedirectToPage("/Index");
        }
    }
}
