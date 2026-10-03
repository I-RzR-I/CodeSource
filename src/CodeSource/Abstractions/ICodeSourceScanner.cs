// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="ICodeSourceScanner.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System.Collections.Generic;
using System.Reflection;
using RzR.Core.CodeSource.Models;

#endregion

namespace RzR.Core.CodeSource.Abstractions
{
    /// <summary>
    ///     Interface for code source scanner.
    /// </summary>
    public interface ICodeSourceScanner
    {
        /// <summary>
        ///     Finds the annotations in the given assembly and in its referenced assemblies.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        IEnumerable<CodeSourceObjectsResult> FindAnnotations(Assembly assembly);

        /// <summary>
        ///     Finds the annotations in the assembly loaded by name and in its referenced assemblies.
        /// </summary>
        /// <param name="assemblyName">Name of the assembly.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        IEnumerable<CodeSourceObjectsResult> FindAnnotations(string assemblyName);

        /// <summary>
        ///     Finds the annotations in exactly the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        IEnumerable<CodeSourceObjectsResult> FindAnnotations(IEnumerable<Assembly> assemblies);
    }
}