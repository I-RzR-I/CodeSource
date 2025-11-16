// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
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
using CodeSource.Abstractions;
using CodeSource.Helpers;
using CodeSource.Models;

#endregion

namespace CodeSource.Services
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
#pragma warning disable CS0618 // Type or member is obsolete
            return CodeSourceHelper.GetCodeSourceAssembly(assembly.FullName);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <inheritdoc />
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(string assemblyName)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return CodeSourceHelper.GetCodeSourceAssembly(assemblyName);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <inheritdoc />
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(IEnumerable<Assembly> assemblies)
        {
            return CodeSourceHelper.GetCodeSourceAssembly(assemblies);
        }
    }
}