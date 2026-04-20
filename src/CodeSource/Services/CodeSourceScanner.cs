// ***********************************************************************
//  Assembly         : RzR.Core.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-13 18:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-14 19:04
// ***********************************************************************
//  <copyright file="CodeSourceScanner.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;
using System.Reflection;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Helpers;
using RzR.Core.CodeSource.Models;

#endregion

namespace RzR.Core.CodeSource.Services
{
    /// <inheritdoc cref="ICodeSourceScanner" />
    public class CodeSourceScanner : ICodeSourceScanner
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     The code source annotation scanner instance.
        /// </summary>
        /// =================================================================================================
        public static CodeSourceScanner Instance = new CodeSourceScanner();

        /// <inheritdoc />
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(Assembly assembly)
        {
            return CodeSourceHelper.GetCodeSourceAssembly(assembly.FullName);
        }

        /// <inheritdoc />
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(string assemblyName)
        {
            return CodeSourceHelper.GetCodeSourceAssembly(assemblyName);
        }

        /// <inheritdoc />
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(IEnumerable<Assembly> assemblies)
        {
            return CodeSourceHelper.GetCodeSourceAssembly(assemblies);
        }
    }
}