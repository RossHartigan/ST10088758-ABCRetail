using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace ABC_Retail.Pages
{
    public class CustomerProfilesModel : PageModel
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly TableClient _tableClient;

        public CustomerProfilesModel(TableServiceClient tableServiceClient)
        {
            _tableServiceClient = tableServiceClient;
            _tableClient = _tableServiceClient.GetTableClient("CustomerProfiles");
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
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var customerEntity = new TableEntity(CustomerId, CustomerId)
            {
                {"Name", Name},
                {"Email", Email},
                {"Phone", Phone}
            };

            await _tableClient.AddEntityAsync(customerEntity);

            return RedirectToPage("/Index"); // Redirect to a page or confirmation view after successful submission
        }
    }
}
