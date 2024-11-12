using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace ABC_Retail.Pages
{
    public class CustomerProfilesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CustomerProfilesModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _functionUrl = "https://abc-retail-function-st10088758.azurewebsites.net/api/AddCustomerToTable?code=zGDk4CZIKs09bXmeX2KFHVOleSAVFrZRvY3a0zHhHzrOAzFuNDeRmA%3D%3D";

        public CustomerProfilesModel(IHttpClientFactory httpClientFactory, ILogger<CustomerProfilesModel> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
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

            // Call Azure Function
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(customer), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(_functionUrl, jsonContent);

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Azure Function Response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to add customer via Azure Function. Status Code: {response.StatusCode}, Response: {responseContent}");
                ModelState.AddModelError(string.Empty, "Failed to add customer to Azure Function.");
                return Page();
            }

            // Add Customer to SQL Database
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    _logger.LogInformation("Connection to SQL database opened.");

                    // Exclude CustomerID if it's auto-increment
                    string query = "INSERT INTO Customers (Name, Email, Phone) VALUES (@Name, @Email, @Phone)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", Name);
                        cmd.Parameters.AddWithValue("@Email", Email);
                        cmd.Parameters.AddWithValue("@Phone", Phone);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        _logger.LogInformation($"Rows affected: {rowsAffected}");
                    }
                }
                _logger.LogInformation("Customer added to SQL Database successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add customer to SQL Database. Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Failed to add customer to SQL Database.");
                return Page();
            }

            _logger.LogInformation("Customer added successfully to both Azure Function and SQL Database.");
            return RedirectToPage("/Index");
        }
    }
}