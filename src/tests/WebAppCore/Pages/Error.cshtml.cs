// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.WebAppCore
//  Author           : RzR
//  Created On       : 2022-12-13 08:56
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:46
// ***********************************************************************
//  <copyright file="Error.cshtml.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Diagnostics;
using CodeSource;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

#endregion

namespace WebAppCore.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    [CodeSource("link", "Me", null, version: 1)]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        [CodeSource("link", "Me", null, comment: "CTOR")]
        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
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