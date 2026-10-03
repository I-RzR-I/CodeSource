using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RzR.Core.CodeSource;

namespace WebAppCore.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    [CodeSource("link", "Me", null, version: "1")]
    public class ErrorModel : PageModel
    {
        [CodeSource("link", "Me", null, comment: "CTOR")]
        public ErrorModel()
        {
        }

        public DateTime Date => DateTime.Now;
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        [CodeSource("link", "Me", null, comment: "OnGet")]
        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }
    }
}