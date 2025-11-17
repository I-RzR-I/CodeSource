// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2022-12-13 02:23
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:48
// ***********************************************************************
//  <copyright file="CodeSourceHelper.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeSource.Extensions.Internal;
using CodeSource.Models;

// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable RedundantAssignment

#endregion

namespace CodeSource.Helpers
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Code source helper methods.
    /// </summary>
    /// =================================================================================================
    public static class CodeSourceHelper
    {
        private static Type _typeInfo;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Get assembly code source.
        /// </summary>
        /// <remarks>
        ///     Running this method can take some time in case when you have a big project.
        /// </remarks>
        /// <param name="assembly">(Optional) Optional. The default value is null.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the code source assemblies in
        ///     this collection.
        /// </returns>
        /// =================================================================================================
        [Obsolete("This method is deprecated. Use available from CodeSource.Services.CodeSourceScanner.FindAnnotations")]
        public static IEnumerable<CodeSourceObjectsResult> GetCodeSourceAssembly(string assembly = null)
        {
            var assemblies = GetListOfEntryAssemblyWithReferences(assembly);

            return GetCodeSourceAssembly(assemblies);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Get assembly code source.
        /// </summary>
        /// <remarks>
        ///     Running this method can take some time in case when you have a big project.
        /// </remarks>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the code source assemblies in
        ///     this collection.
        /// </returns>
        /// =================================================================================================
        internal static IEnumerable<CodeSourceObjectsResult> GetCodeSourceAssembly(IEnumerable<Assembly> assemblies)
        {
            try
            {
                var codeSource = new List<CodeSourceObjectsResult>();
                foreach (var localAssembly in assemblies)
                {
#if NET45_OR_GREATER || NET || NETSTANDARD1_5_OR_GREATER
                    foreach (var t in localAssembly.GetExportedTypes())
#elif NETSTANDARD1_0
                    foreach (var t in localAssembly.ExportedTypes)
#else
                    foreach (var t in localAssembly.GetExportedTypes())
#endif
                    {
                        var obj = Activator.CreateInstance<CodeSourceObjectsResult>();

                        _typeInfo = t;
                        GetClassCodeSource(ref _typeInfo, ref obj);
                        GetCtorCodeSource(ref _typeInfo, ref obj);
                        GetMethodCodeSource(ref _typeInfo, ref obj, t);

                        if (obj.Parent != null || obj.Children != null && obj.Children.Any())
                            codeSource.Add(obj);
                    }
                }

                return codeSource;
            }
            catch
            {
                return null;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Get entry assembly with references.
        /// </summary>
        /// <param name="assembly">The Assembly name.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the list of entry assembly with
        ///     references in this collection.
        /// </returns>
        /// =================================================================================================
        private static IEnumerable<Assembly> GetListOfEntryAssemblyWithReferences(string assembly)
        {
            var listOfAssemblies = new List<Assembly>();
            var mainAsm = Assembly.Load(new AssemblyName(assembly!));

            listOfAssemblies.Add(mainAsm);

#if NETSTANDARD1_0
            var assemblies = mainAsm.ExportedTypes.Select(x => x.GetTypeInfo().Assembly);
            listOfAssemblies.AddRange(assemblies);
#else
            listOfAssemblies.AddRange(mainAsm.GetReferencedAssemblies().Select(Assembly.Load));
#endif

            return listOfAssemblies;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Get Classes code source details.
        /// </summary>
        /// <param name="typeInfo">[in,out] Type info.</param>
        /// <param name="dataObject">[in,out] Current result object data.</param>
        /// =================================================================================================
        private static void GetClassCodeSource(ref Type typeInfo, ref CodeSourceObjectsResult dataObject)
        {
            try
            {
                var parent = new CodeSourceObject()
                {
                    FullName = typeInfo.FullName,
                    Name = typeInfo.Name,
                    History = Activator.CreateInstance<List<CodeSourceObjectHistory>>()
                };

                //var classAttributes = typeInfo.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
                var classAttributes =
#if NET45_OR_GREATER || NET
                        typeInfo.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
#elif NETSTANDARD1_0
                    typeInfo.GetTypeInfo().GetCustomAttributes(typeof(CodeSourceAttribute), true).ToList();
#elif NETSTANDARD1_5_OR_GREATER
                    typeInfo.GetTypeInfo().GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
#else
                    typeInfo.GetCustomAttributes(typeof(CodeSourceAttribute), true).ToList();
#endif

                if (classAttributes.IsNullOrEmpty()) return; // Check if the current item doesn't have the attribute, then ignore

                var changeHistory = Activator.CreateInstance<List<CodeSourceObjectHistory>>();
                foreach (var atr in classAttributes)
                {
                    changeHistory.Add(SetHistoryItemData((CodeSourceAttribute)atr, typeInfo.FullName, string.Empty));
                }

                parent.History = changeHistory;
                dataObject.Parent = parent;
            }
            catch
            {
                /*ignored*/
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Get CTOR code source details.
        /// </summary>
        /// <param name="typeInfo">[in,out] Type info.</param>
        /// <param name="dataObject">[in,out] Current result object data.</param>
        /// =================================================================================================
        private static void GetCtorCodeSource(ref Type typeInfo, ref CodeSourceObjectsResult dataObject)
        {
            try
            {
                var children = Activator.CreateInstance<List<CodeSourceObject>>();
                ConstructorInfo[] constructorInfos = new ConstructorInfo[] { };
#if NET45_OR_GREATER || NET || NETSTANDARD2_0_OR_GREATER
                constructorInfos = typeInfo.GetTypeInfo().DeclaredConstructors.ToArray();
#elif NETSTANDARD1_5 || NETSTANDARD1_0
                constructorInfos = typeInfo.GetTypeInfo().DeclaredConstructors.ToArray();
#else
                constructorInfos = typeInfo.GetConstructors();
#endif
                foreach (var ctor in constructorInfos)
                {
                    var ctorAttributes =
#if NET45_OR_GREATER || NET || NETSTANDARD1_5_OR_GREATER
                        ctor.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
#else
                        ((Attribute[])ctor.GetCustomAttributes(typeof(CodeSourceAttribute), true)).ToList();
#endif
                    if (ctorAttributes.Any() && dataObject.Parent == null)
                        dataObject.Parent = new CodeSourceObject
                        {
                            FullName = typeInfo.FullName,
                            Name = typeInfo.Name
                        };

                    SetChildHistoryData(ref children, ctorAttributes, typeInfo.FullName, ctor.Name);
                }

                dataObject.Children = dataObject.Children.IsNullOrEmpty()
                    ? children
                    : dataObject.Children.Concat(children);
            }
            catch
            {
                /*ignored*/
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Get methods code source details.
        /// </summary>
        /// <param name="typeInfo">[in,out] Type info.</param>
        /// <param name="dataObject">[in,out] Current result object data.</param>
        /// <param name="exportedTypes">Exported assembly type.</param>
        /// =================================================================================================
        private static void GetMethodCodeSource(ref Type typeInfo, ref CodeSourceObjectsResult dataObject,
            Type exportedTypes)
        {
            try
            {
                var children = Activator.CreateInstance<List<CodeSourceObject>>();
                var classMethods =
#if NET45_OR_GREATER || NET || NETSTANDARD1_5_OR_GREATER
                exportedTypes.GetRuntimeMethods();
#elif NETSTANDARD1_0
                exportedTypes.GetRuntimeMethods();
#else
                exportedTypes.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
#endif

                foreach (var m in classMethods)
                {
                    var classMethodAttributes =
#if NET45_OR_GREATER || NET || NETSTANDARD1_5_OR_GREATER
                        m.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
#else
                        ((Attribute[])m.GetCustomAttributes(typeof(CodeSourceAttribute), true)).ToList();
#endif
                    if (classMethodAttributes.Any() && dataObject.Parent == null)
                        dataObject.Parent = new CodeSourceObject
                        {
                            FullName = typeInfo.FullName,
                            Name = typeInfo.Name
                        };

                    SetChildHistoryData(ref children, classMethodAttributes, typeInfo.FullName, m.Name);
                }

                dataObject.Children = dataObject.Children.IsNullOrEmpty() 
                    ? children 
                    : dataObject.Children.Concat(children);
            }
            catch
            {
                /*ignored*/
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Sets child history data.
        /// </summary>
        /// <param name="children">[in,out] The children.</param>
        /// <param name="historyAttributes">The history attributes.</param>
        /// <param name="fullName">Name of the full.</param>
        /// <param name="currentItemName">The current item name.</param>
        /// =================================================================================================
        private static void SetChildHistoryData(ref List<CodeSourceObject> children, IEnumerable<Attribute> historyAttributes,
            string fullName, string currentItemName)
        {
            if (!historyAttributes.Any()) return; // Check if the current item doesn't have the attribute, then ignore

            var child = Activator.CreateInstance<CodeSourceObject>();
            child.Name = currentItemName;
            child.FullName = StringExtensions.SetFullName(fullName, currentItemName);

            var changeHistory = Activator.CreateInstance<List<CodeSourceObjectHistory>>();
            changeHistory.AddRange(historyAttributes.Select(
                atr => SetHistoryItemData((CodeSourceAttribute)atr, fullName, currentItemName)));

            child.History = changeHistory;

            children.Add(child);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Sets history item data.
        /// </summary>
        /// <param name="historyAttribute">The history attribute.</param>
        /// <param name="fullName">Name of the full.</param>
        /// <param name="currentItemName">The current item name.</param>
        /// <returns>
        ///     A CodeSourceObjectHistory.
        /// </returns>
        /// =================================================================================================
        private static CodeSourceObjectHistory SetHistoryItemData(CodeSourceAttribute historyAttribute,
            string fullName, string currentItemName)
        {
            var history = Activator.CreateInstance<CodeSourceObjectHistory>();

            history.AuthorName = historyAttribute.AuthorName;
            history.Comment = historyAttribute.Comment;
            history.Copyright = historyAttribute.Copyright;
            history.SourceUrl = historyAttribute.SourceUrl;
            history.AppliedOn = historyAttribute.InternalAppliedOn;
            history.Version = historyAttribute.Version == 0 ? 1.0 : historyAttribute.Version;
            history.CodePath = StringExtensions.SetCodePath(fullName, currentItemName);
            history.Tags = historyAttribute.Tags;
            history.RelatedTaskId = historyAttribute.RelatedTaskId;

            return history;
        }
    }
}