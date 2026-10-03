using Microsoft.AspNetCore.Mvc.RazorPages;
using RzR.Core.CodeSource;

namespace WebAppCore.Pages
{
    public class IndexModel : PageModel
    {
        [CodeSource("link-Index", "Me", null, comment: "OnGet", appliedOn: "2022-12-16")]
        public void OnGet()
        {
        }
    }
}