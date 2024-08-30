using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;
using ABC_Retail.Services;
using System.Threading.Tasks;

namespace ABC_Retail.Pages
{
    public class ProductInformationModel : PageModel
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;

        public ProductInformationModel(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
            _tableClient = _tableServiceClient.GetTableClient("ProductInformation");
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

        public List<ProductInformation> Products { get; set; } = new List<ProductInformation>();

        public void OnGet()
        {
            
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var productEntity = new TableEntity(ProductId, ProductId)
            {
                {"Name", Name },
                {"Description", Description},
                {"Price", Price}
            };

            await _tableClient.AddEntityAsync(productEntity);

            return RedirectToPage("/Index");
        }
    }
}
