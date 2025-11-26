// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2025-11-13 18:11
// 
//  Last Modified By : RzR
//  Last Modified On : 2025-11-14 19:00
// ***********************************************************************
//  <copyright file="ICodeSourceScanner.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;
using System.Reflection;
using CodeSource.Models;

#endregion

namespace CodeSource.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Interface for code source scanner.
    /// </summary>
    /// =================================================================================================
    public interface ICodeSourceScanner
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Finds the annotations in this collection.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        /// =================================================================================================
        IEnumerable<CodeSourceObjectsResult> FindAnnotations(Assembly assembly);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Finds the annotations in this collection.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        /// =================================================================================================
        IEnumerable<CodeSourceObjectsResult> FindAnnotations(string assemblyName);

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Finds the annotations in their collections.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        /// =================================================================================================
        IEnumerable<CodeSourceObjectsResult> FindAnnotations(IEnumerable<Assembly> assemblies);
    }
}