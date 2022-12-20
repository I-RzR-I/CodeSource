// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.WebAppCore
//  Author           : RzR
//  Created On       : 2022-12-13 08:56
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:46
// ***********************************************************************
//  <copyright file="Index.cshtml.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using CodeSource;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

#endregion

namespace WebAppCore.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        [CodeSource("link-Index", "Me", null, comment: "OnGet", appliedOn: "2022-12-16")]
        public void OnGet()
        {
        }
    }
}