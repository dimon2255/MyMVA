using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyMVA.Pages
{
    [Authorize(Policy = "CanadiansorAdmin")]
    public class PrivacyModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}