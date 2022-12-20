﻿// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.WebAppCore
//  Author           : RzR
//  Created On       : 2022-12-13 08:56
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:46
// ***********************************************************************
//  <copyright file="Privacy.cshtml.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

#endregion

namespace WebAppCore.Pages
{
    public class PrivacyModel : PageModel
    {
        private readonly ILogger<PrivacyModel> _logger;

        public PrivacyModel(ILogger<PrivacyModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}