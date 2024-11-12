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
    public class ProductInformationModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductInformationModel> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _functionUrl = "https://abc-retail-function-st10088758.azurewebsites.net/api/AddProductToTable?code=gk9ELbjsRgAjRV_ZHpOZpwl8Yir37jp8m1pJPcWNUR0WAzFuo32lFw%3D%3D";

        public ProductInformationModel(IHttpClientFactory httpClientFactory, ILogger<ProductInformationModel> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
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
                _logger.LogWarning("Model state is invalid.");
                return Page();
            }

            // Create an object to send to the Azure Function
            var product = new
            {
                PartitionKey = ProductId,
                Name = Name,
                Description = Description,
                Price = Price
            };

            // Call Azure Function
            var httpClient = _httpClientFactory.CreateClient();
            var jsonContent = new StringContent(JsonConvert.SerializeObject(product), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(_functionUrl, jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"Azure Function Response: {responseContent}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Failed to add product via Azure Function. Status Code: {response.StatusCode}, Response: {responseContent}");
                ModelState.AddModelError(string.Empty, "Failed to add product to Azure Function.");
                return Page();
            }

            // Add Product to SQL Database
            try
            {
                string connectionString = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();
                    _logger.LogInformation("Connection to SQL database opened.");

                    // Exclude ProductID as it is auto-increment
                    string query = "INSERT INTO Products (Name, Description, Price) VALUES (@Name, @Description, @Price)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", Name);
                        cmd.Parameters.AddWithValue("@Description", Description);
                        cmd.Parameters.AddWithValue("@Price", Price);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();
                        _logger.LogInformation($"Rows affected: {rowsAffected}");
                    }
                }
                _logger.LogInformation("Product added to SQL Database successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to add product to SQL Database. Error: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Failed to add product to SQL Database.");
                return Page();
            }

            _logger.LogInformation("Product added successfully to both Azure Function and SQL Database.");
            return RedirectToPage("/Index");
        }
    }
}