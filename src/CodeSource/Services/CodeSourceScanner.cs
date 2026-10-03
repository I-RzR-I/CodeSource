// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:22
//  ***********************************************************************
//  <copyright file="CodeSourceScanner.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Collections.Generic;
using System.Reflection;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Extensions.Internal;
using RzR.Core.CodeSource.Helpers;
using RzR.Core.CodeSource.Models;

#endregion

namespace RzR.Core.CodeSource.Services
{
    /// <inheritdoc cref="ICodeSourceScanner" />
    public class CodeSourceScanner : ICodeSourceScanner
    {
        /// <summary>
        ///     (Immutable)
        ///     The code source annotation scanner instance.
        /// </summary>
        public static readonly CodeSourceScanner Instance = new();

        /// <summary>
        ///     Finds the annotations in the given assembly and in its referenced assemblies.
        /// </summary>
        /// <remarks>
        ///     Assemblies loaded from bytes, from a path or in a plugin load context are scanned
        ///     directly, without being reloaded by name. A reflection-only or MetadataLoadContext
        ///     assembly is re-resolved by its full name into the execution context first. A dynamic
        ///     assembly yields no annotations and is reported as
        ///     <see cref="CodeSourceScanStage.TypeEnumeration" />. Custom <see cref="Assembly" />
        ///     subclasses that do not implement the reflection APIs are not supported. Each referenced
        ///     assembly is loaded by name and scanned once. On netstandard1.0 only the given assembly is
        ///     scanned.
        ///     <para>
        ///         Without an error callback, a referenced assembly that cannot be loaded, and any other
        ///         recoverable failure, is skipped silently: the scan returns fewer results and gives no
        ///         signal. Use <see cref="FindAnnotations(Assembly, CodeSourceScanOptions)" />
        ///         with <see cref="CodeSourceScanOptions.OnError" /> to observe these failures.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="assembly" /> is null.
        /// </exception>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(Assembly assembly)
        {
            return FindAnnotations(assembly, null);
        }

        /// <summary>
        ///     Finds the annotations in the assembly loaded by name and in its referenced assemblies.
        /// </summary>
        /// <remarks>
        ///     A failure to load the named assembly itself propagates. Each referenced assembly is
        ///     loaded by name and scanned once. On netstandard1.0 only the named assembly is scanned.
        ///     <para>
        ///         Without an error callback, a referenced assembly that cannot be loaded, and any other
        ///         recoverable failure, is skipped silently: the scan returns fewer results and gives no
        ///         signal. Use <see cref="FindAnnotations(string, CodeSourceScanOptions)" />
        ///         with <see cref="CodeSourceScanOptions.OnError" /> to observe these failures.
        ///     </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="assemblyName" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="assemblyName" /> is empty or whitespace.
        /// </exception>
        /// <param name="assemblyName">The assembly display name.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(string assemblyName)
        {
            return FindAnnotations(assemblyName, null);
        }

        /// <summary>
        ///     Finds the annotations in exactly the given assemblies.
        /// </summary>
        /// <remarks>
        ///     Referenced assemblies are not scanned, duplicates are not removed and null elements are
        ///     skipped. Recoverable failures are skipped silently; use
        ///     <see cref="FindAnnotations(IEnumerable{Assembly}, CodeSourceScanOptions)" /> to observe
        ///     them.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="assemblies" /> is null.
        /// </exception>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(IEnumerable<Assembly> assemblies)
        {
            return FindAnnotations(assemblies, null);
        }

        /// <summary>
        ///     Finds the annotations in the given assembly and in its referenced assemblies, reporting
        ///     recoverable failures through <paramref name="options" />.
        /// </summary>
        /// <remarks>
        ///     Assemblies loaded from bytes, from a path or in a plugin load context are scanned
        ///     directly, without being reloaded by name. A reflection-only or MetadataLoadContext
        ///     assembly is re-resolved by its full name into the execution context first. A dynamic
        ///     assembly yields no annotations and is reported as
        ///     <see cref="CodeSourceScanStage.TypeEnumeration" />. Custom <see cref="Assembly" />
        ///     subclasses that do not implement the reflection APIs are not supported. Each referenced
        ///     assembly is loaded by name and scanned once. On netstandard1.0 only the given assembly is
        ///     scanned. An exception thrown by <see cref="CodeSourceScanOptions.OnError" /> aborts the
        ///     scan and propagates.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="assembly" /> is null.
        /// </exception>
        /// <param name="assembly">The assembly.</param>
        /// <param name="options">
        ///     The scan options, or null to skip recoverable failures silently.
        /// </param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(Assembly assembly, CodeSourceScanOptions options)
        {
            if (assembly.IsNull())
                throw new ArgumentNullException(nameof(assembly));

            return CodeSourceHelper.GetCodeSourceAssembly(assembly, options);
        }

        /// <summary>
        ///     Finds the annotations in the assembly loaded by name and in its referenced assemblies,
        ///     reporting recoverable failures through <paramref name="options" />.
        /// </summary>
        /// <remarks>
        ///     A failure to load the named assembly itself propagates. Each referenced assembly is
        ///     loaded by name and scanned once. On netstandard1.0 only the named assembly is scanned. An
        ///     exception thrown by <see cref="CodeSourceScanOptions.OnError" /> aborts the scan and
        ///     propagates.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="assemblyName" /> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     Thrown when <paramref name="assemblyName" /> is empty or whitespace.
        /// </exception>
        /// <param name="assemblyName">The assembly display name.</param>
        /// <param name="options">
        ///     The scan options, or null to skip recoverable failures silently.
        /// </param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(string assemblyName, CodeSourceScanOptions options)
        {
            if (assemblyName.IsNull())
                throw new ArgumentNullException(nameof(assemblyName));

            if (assemblyName.IsMissing())
                throw new ArgumentException("The assembly name must not be empty or whitespace.",
                    nameof(assemblyName));

            return CodeSourceHelper.GetCodeSourceAssembly(assemblyName, options);
        }

        /// <summary>
        ///     Finds the annotations in exactly the given assemblies, reporting recoverable failures
        ///     through <paramref name="options" />.
        /// </summary>
        /// <remarks>
        ///     Referenced assemblies are not scanned, duplicates are not removed and null elements are
        ///     skipped. An exception thrown by <see cref="CodeSourceScanOptions.OnError" /> aborts the
        ///     scan and propagates.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="assemblies" /> is null.
        /// </exception>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="options">
        ///     The scan options, or null to skip recoverable failures silently.
        /// </param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the annotations in this
        ///     collection.
        /// </returns>
        public IEnumerable<CodeSourceObjectsResult> FindAnnotations(IEnumerable<Assembly> assemblies,
            CodeSourceScanOptions options)
        {
            if (assemblies.IsNull())
                throw new ArgumentNullException(nameof(assemblies));

            return CodeSourceHelper.GetCodeSourceAssembly(assemblies, options);
        }
    }
}