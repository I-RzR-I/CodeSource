// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="CodeSourceHelper.cs" company="RzR SOFT & TECH">
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
using System.Linq;
using System.Reflection;
using RzR.Core.CodeSource.Extensions.Internal;
using RzR.Core.CodeSource.Models;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable RedundantAssignment

#endregion

namespace RzR.Core.CodeSource.Helpers
{
    /// <summary>
    ///     Code source helper methods.
    /// </summary>
    internal static class CodeSourceHelper
    {
        /// <summary>
        ///     Get the code source of the assembly loaded by name, and of its referenced assemblies.
        /// </summary>
        /// <param name="assemblyName">The assembly display name.</param>
        /// <param name="options">
        ///     The scan options, or null to skip recoverable failures silently.
        /// </param>
        /// <returns>
        ///     The annotated types.
        /// </returns>
        internal static List<CodeSourceObjectsResult> GetCodeSourceAssembly(string assemblyName,
            CodeSourceScanOptions options)
        {
            var mainAssembly = Assembly.Load(new AssemblyName(assemblyName));

            return GetCodeSourceAssembly(WithReferences(mainAssembly, options), options);
        }

        /// <summary>
        ///     Get the code source of the given assembly, and of its referenced assemblies.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="options">
        ///     The scan options, or null to skip recoverable failures silently.
        /// </param>
        /// <returns>
        ///     The annotated types.
        /// </returns>
        internal static List<CodeSourceObjectsResult> GetCodeSourceAssembly(Assembly assembly,
            CodeSourceScanOptions options)
        {
#if !NETSTANDARD1_0 && !NETSTANDARD1_5
            if (IsReflectionOnly(assembly))
                assembly = Assembly.Load(new AssemblyName(assembly.FullName));
#endif

            return GetCodeSourceAssembly(WithReferences(assembly, options), options);
        }

#if !NETSTANDARD1_0 && !NETSTANDARD1_5

        /// <summary>
        ///     Query if the assembly was loaded for reflection only (including metadata-only contexts).
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        ///     True if reflection-only, false if not.
        /// </returns>
        private static bool IsReflectionOnly(Assembly assembly)
        {
            try
            {
                return assembly.ReflectionOnly;
            }
            catch (NotImplementedException)
            {
                return false;
            }
        }
#endif

        /// <summary>
        ///     Get the code source of exactly the given assemblies.
        /// </summary>
        /// <param name="assemblies">The assemblies.</param>
        /// <param name="options">
        ///     The scan options, or null to skip recoverable failures silently.
        /// </param>
        /// <returns>
        ///     The annotated types, in assembly and type order.
        /// </returns>
        internal static List<CodeSourceObjectsResult> GetCodeSourceAssembly(IEnumerable<Assembly> assemblies,
            CodeSourceScanOptions options)
        {
            var codeSource = new List<CodeSourceObjectsResult>();
            foreach (var assembly in assemblies)
            {
                if (assembly.IsNull())
                    continue;

                var assemblyName = assembly.FullName;
                foreach (var type in GetExportedTypes(assembly, options))
                {
                    var result = GetTypeCodeSource(assemblyName, type, options);
                    if (result.IsNotNull())
                        codeSource.Add(result);
                }
            }

            return codeSource;
        }

        /// <summary>
        ///     Gets the root assembly followed by its referenced assemblies, each listed at most once.
        /// </summary>
        /// <param name="root">The root assembly.</param>
        /// <param name="options">The scan options.</param>
        /// <returns>
        ///     The assemblies to scan, root first.
        /// </returns>
        private static List<Assembly> WithReferences(Assembly root, CodeSourceScanOptions options)
        {
            var assemblies = new List<Assembly> { root };

#if !NETSTANDARD1_0
            var seen = new HashSet<string>(StringComparer.Ordinal) { root.FullName };
            foreach (var reference in GetReferencedAssemblies(root, options))
            {
                if (!seen.Add(reference.FullName))
                    continue;

                var loaded = LoadReference(reference, options);

                if (loaded.IsNotNull() && (loaded.FullName == reference.FullName || seen.Add(loaded.FullName)))
                    assemblies.Add(loaded);
            }
#endif

            return assemblies;
        }

        /// <summary>
        ///     Gets the exported types of an assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="options">The scan options.</param>
        /// <returns>
        ///     The exported types, or an empty array if they could not be enumerated.
        /// </returns>
        private static Type[] GetExportedTypes(Assembly assembly, CodeSourceScanOptions options)
        {
            try
            {
#if NETSTANDARD1_0
                return assembly.ExportedTypes.ToArray();
#else
                return assembly.GetExportedTypes();
#endif
            }
            catch (Exception ex) when (ScanExceptionFilter.IsRecoverable(ex, CodeSourceScanStage.TypeEnumeration))
            {
                ReportError(options, CodeSourceScanStage.TypeEnumeration, assembly.FullName, null, null, ex);

                return new Type[0];
            }
        }

        /// <summary>
        ///     Gets the code source of a type: its own annotations, then those of its constructors and
        ///     methods.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly that declares the type.</param>
        /// <param name="type">The type.</param>
        /// <param name="options">The scan options.</param>
        /// <returns>
        ///     The type's code source, or null if neither the type nor any of its members is annotated.
        /// </returns>
        private static CodeSourceObjectsResult GetTypeCodeSource(string assemblyName, Type type,
            CodeSourceScanOptions options)
        {
#if NET40
            var typeMember = (MemberInfo)type;
#else
            var typeMember = (MemberInfo)type.GetTypeInfo();
#endif
            var parentHistory = new List<CodeSourceObjectHistory>();
            var typeAttributes = ReadAttributes(typeMember, assemblyName, type.FullName, null, options);
            if (typeAttributes.IsNotNull())
                parentHistory.AddRange(typeAttributes.Select(atr =>
                    SetHistoryItemData(atr, type.FullName, string.Empty, assemblyName, null, options)));

            var children = new List<CodeSourceObject>();
            foreach (var ctor in GetConstructors(type, assemblyName, options))
                AddChild(children, ctor, type.FullName, assemblyName, options);

            foreach (var method in GetMethods(type, assemblyName, options))
                AddChild(children, method, type.FullName, assemblyName, options);

            if (parentHistory.Count == 0 && children.Count == 0)
                return null;

            return new CodeSourceObjectsResult
            {
                Parent = new CodeSourceObject
                {
                    FullName = type.FullName,
                    Name = type.Name,
                    History = parentHistory
                },
                Children = children
            };
        }

        /// <summary>
        ///     Gets the constructors of a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="options">The scan options.</param>
        /// <returns>
        ///     The constructors, or an empty array if they could not be enumerated.
        /// </returns>
        private static ConstructorInfo[] GetConstructors(Type type, string assemblyName,
            CodeSourceScanOptions options)
        {
            try
            {
#if NET40
                return type.GetConstructors();
#else
                return type.GetTypeInfo().DeclaredConstructors.ToArray();
#endif
            }
            catch (Exception ex) when (ScanExceptionFilter.IsRecoverable(ex, CodeSourceScanStage.MemberEnumeration))
            {
                ReportError(options, CodeSourceScanStage.MemberEnumeration, assemblyName, type.FullName, null, ex);

                return new ConstructorInfo[0];
            }
        }

        /// <summary>
        ///     Gets the methods of a type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="options">The scan options.</param>
        /// <returns>
        ///     The methods, or an empty array if they could not be enumerated.
        /// </returns>
        private static MethodInfo[] GetMethods(Type type, string assemblyName, CodeSourceScanOptions options)
        {
            try
            {
#if NET40
                return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
#else
                return type.GetRuntimeMethods().ToArray();
#endif
            }
            catch (Exception ex) when (ScanExceptionFilter.IsRecoverable(ex, CodeSourceScanStage.MemberEnumeration))
            {
                ReportError(options, CodeSourceScanStage.MemberEnumeration, assemblyName, type.FullName, null, ex);

                return new MethodInfo[0];
            }
        }

        /// <summary>
        ///     Adds a child entry for an annotated constructor or method.
        /// </summary>
        /// <param name="children">The children collected so far.</param>
        /// <param name="member">The constructor or method.</param>
        /// <param name="fullName">The full name of the declaring type.</param>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="options">The scan options.</param>
        private static void AddChild(List<CodeSourceObject> children, MethodBase member, string fullName,
            string assemblyName, CodeSourceScanOptions options)
        {
            var attributes = ReadAttributes(member, assemblyName, fullName, member.Name, options);
            if (attributes.IsNullOrEmpty())
                return; // Check if the current item doesn't have the attribute, then ignore

            children.Add(new CodeSourceObject
            {
                Name = member.Name,
                FullName = StringExtensions.SetFullName(fullName, member.Name),
                History = attributes
                    .Select(atr => SetHistoryItemData(atr, fullName, member.Name, assemblyName, member.Name, options))
                    .ToList()
            });
        }

        /// <summary>
        ///     Reads the <see cref="CodeSourceAttribute" /> instances applied to a type or member.
        /// </summary>
        /// <param name="member">The type (as a member) or the member.</param>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="typeName">The full name of the type.</param>
        /// <param name="memberName">The member name, or null for the type itself.</param>
        /// <param name="options">The scan options.</param>
        /// <returns>
        ///     The attributes, or null if they could not be read.
        /// </returns>
        private static List<CodeSourceAttribute> ReadAttributes(MemberInfo member, string assemblyName,
            string typeName, string memberName, CodeSourceScanOptions options)
        {
            try
            {
#if NET45_OR_GREATER || NET || NETSTANDARD1_5_OR_GREATER
                var attributes = member.GetCustomAttributes(typeof(CodeSourceAttribute));
#else
                var attributes = member.GetCustomAttributes(typeof(CodeSourceAttribute), true);
#endif
                return attributes.Cast<CodeSourceAttribute>().ToList();
            }
            catch (Exception ex) when (ScanExceptionFilter.IsRecoverable(ex, CodeSourceScanStage.AttributeRead))
            {
                ReportError(options, CodeSourceScanStage.AttributeRead, assemblyName, typeName, memberName, ex);

                return null;
            }
        }

        /// <summary>
        ///     Sets history item data.
        /// </summary>
        /// <remarks>
        ///     An <c>AppliedOn</c> value that is present but not a valid date is reported as
        ///     <see cref="CodeSourceScanStage.AttributeValue" />, and the history item keeps a null date. 
        /// </remarks>
        /// <param name="historyAttribute">The history attribute.</param>
        /// <param name="fullName">Name of the full.</param>
        /// <param name="currentItemName">The current item name.</param>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="memberName">The member name, or null for the type itself.</param>
        /// <param name="options">The scan options.</param>
        /// <returns>
        ///     A CodeSourceObjectHistory.
        /// </returns>
        private static CodeSourceObjectHistory SetHistoryItemData(CodeSourceAttribute historyAttribute,
            string fullName, string currentItemName, string assemblyName, string memberName,
            CodeSourceScanOptions options)
        {
            var history = new CodeSourceObjectHistory();

            history.AuthorName = historyAttribute.AuthorName;
            history.Comment = historyAttribute.Comment;
            history.Copyright = historyAttribute.Copyright;
            history.SourceUrl = historyAttribute.SourceUrl;
            history.AppliedOn = historyAttribute.InternalAppliedOn;
            history.Version = historyAttribute.Version.IsMissing() ? "1.0" : historyAttribute.Version;
            history.CodePath = StringExtensions.SetCodePath(fullName, currentItemName);
            history.Tags = historyAttribute.Tags;
            history.RelatedTaskId = historyAttribute.RelatedTaskId;

            if (history.AppliedOn == null && historyAttribute.AppliedOn.IsPresent())
                ReportError(options, CodeSourceScanStage.AttributeValue, assemblyName, fullName, memberName, null,
                    "AppliedOn is not a valid 'yyyy-MM-dd' date, so it was ignored.");

            return history;
        }

        /// <summary>
        ///     Reports a recoverable failure to the caller's callback, if there is one.
        /// </summary>
        /// <remarks>
        ///     An exception thrown by the callback is deliberately not caught: it aborts the scan.
        /// </remarks>
        /// <param name="options">The scan options.</param>
        /// <param name="stage">The scan stage.</param>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="typeName">The full name of the type.</param>
        /// <param name="memberName">The member name.</param>
        /// <param name="exception">The exception, or null.</param>
        /// <param name="description">(Optional) The description used when there is no exception.</param>
        private static void ReportError(CodeSourceScanOptions options, CodeSourceScanStage stage,
            string assemblyName, string typeName, string memberName, Exception exception,
            string description = null)
        {
            var onError = options?.OnError;
            if (onError == null)
                return;

            onError(new CodeSourceScanError(stage, assemblyName, typeName, memberName, exception, description));
        }

#if !NETSTANDARD1_0

        /// <summary>
        ///     Gets the names of the assemblies referenced by the root assembly.
        /// </summary>
        /// <param name="root">The root assembly.</param>
        /// <param name="options">The scan options.</param>
        /// <returns>
        ///     The referenced assembly names, or an empty array if they could not be listed.
        /// </returns>
        private static AssemblyName[] GetReferencedAssemblies(Assembly root, CodeSourceScanOptions options)
        {
            try
            {
                return root.GetReferencedAssemblies();
            }
            catch (Exception ex) when (ScanExceptionFilter.IsRecoverable(ex, CodeSourceScanStage.AssemblyLoad))
            {
                ReportError(options, CodeSourceScanStage.AssemblyLoad, root.FullName, null, null, ex);

                return new AssemblyName[0];
            }
        }

        /// <summary>
        ///     Loads a referenced assembly by name.
        /// </summary>
        /// <param name="reference">The referenced assembly name.</param>
        /// <param name="options">The scan options.</param>
        /// <returns>
        ///     The loaded assembly, or null if it could not be loaded.
        /// </returns>
        private static Assembly LoadReference(AssemblyName reference, CodeSourceScanOptions options)
        {
            try
            {
                return Assembly.Load(reference);
            }
            catch (Exception ex) when (ScanExceptionFilter.IsRecoverable(ex, CodeSourceScanStage.AssemblyLoad))
            {
                ReportError(options, CodeSourceScanStage.AssemblyLoad, reference.FullName, null, null, ex);

                return null;
            }
        }
#endif
    }
}