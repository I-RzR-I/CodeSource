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

#if NET45_OR_GREATER || NET || NETSTANDARD1_5_OR_GREATER

#region U S A G E S

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeSource.Abstractions;
using CodeSource.Models;

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
        public static IEnumerable<CodeSourceObjectsResult> GetCodeSourceAssembly(string assembly = null)
        {
            try
            {
                var codeSource = new List<CodeSourceObjectsResult>();
                foreach (var localAssembly in GetListOfEntryAssemblyWithReferences(assembly))
                    foreach (var t in localAssembly.ExportedTypes)
                    {
                        var ti = t.GetTypeInfo();
                        var obj = Activator.CreateInstance<CodeSourceObjectsResult>();

                        GetClassCodeSource(ref ti, ref obj);
                        GetCtorCodeSource(ref ti, ref obj);
                        GetMethodCodeSource(ref ti, ref obj, t);

                        if (obj.Parent != null || obj.Children != null && obj.Children.Any())
                            codeSource.Add(obj);
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
        /// <param name="assembly">(Optional) Optional. The default value is null.</param>
        /// <returns>
        ///     An enumerator that allows foreach to be used to process the list of entry assembly with
        ///     references in this collection.
        /// </returns>
        /// =================================================================================================
        private static IEnumerable<Assembly> GetListOfEntryAssemblyWithReferences(string assembly = null)
        {
            Assembly mainAsm;
            var listOfAssemblies = new List<Assembly>();

            if (string.IsNullOrEmpty(assembly))
                mainAsm = Assembly.GetEntryAssembly();
            mainAsm = Assembly.Load(new AssemblyName(assembly!));

            listOfAssemblies.Add(mainAsm);
            listOfAssemblies.AddRange(mainAsm.GetReferencedAssemblies().Select(Assembly.Load));

            return listOfAssemblies;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Get Classes code source details.
        /// </summary>
        /// <param name="typeInfo">[in,out] Type info.</param>
        /// <param name="dataObject">[in,out] Current result object data.</param>
        /// =================================================================================================
        private static void GetClassCodeSource(ref TypeInfo typeInfo, ref CodeSourceObjectsResult dataObject)
        {
            try
            {
                var parent = new CodeSourceObject()
                {
                    FullName = typeInfo.FullName,
                    Name = typeInfo.Name,
                    History = Activator.CreateInstance<List<CodeSourceObjectHistory>>()
                };

                var classAttributes = typeInfo.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
                if (!classAttributes.Any()) return; // Check if the current item doesn't have the attribute, then ignore

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
        private static void GetCtorCodeSource(ref TypeInfo typeInfo, ref CodeSourceObjectsResult dataObject)
        {
            try
            {
                var children = Activator.CreateInstance<List<CodeSourceObject>>();
                foreach (var ctor in typeInfo.GetConstructors())
                {
                    var ctorAttributes = ctor.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
                    if (ctorAttributes.Any() && dataObject.Parent == null)
                        dataObject.Parent = new CodeSourceObject
                        {
                            FullName = typeInfo.FullName,
                            Name = typeInfo.Name
                        };

                    SetChildHistoryData(ref children, ctorAttributes, typeInfo.FullName, ctor.Name);
                }

                dataObject.Children = children;
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
        private static void GetMethodCodeSource(ref TypeInfo typeInfo, ref CodeSourceObjectsResult dataObject,
            Type exportedTypes)
        {
            try
            {
                var children = Activator.CreateInstance<List<CodeSourceObject>>();
                var classMethods = exportedTypes.GetRuntimeMethods();
                foreach (var m in classMethods)
                {
                    var classMethodAttributes = m.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
                    if (classMethodAttributes.Any() && dataObject.Parent == null)
                        dataObject.Parent = new CodeSourceObject
                        {
                            FullName = typeInfo.FullName,
                            Name = typeInfo.Name
                        };

                    SetChildHistoryData(ref children, classMethodAttributes, typeInfo.FullName, m.Name);
                }

                dataObject.Children = children;
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
        private static void SetChildHistoryData(ref List<CodeSourceObject> children, IReadOnlyCollection<Attribute> historyAttributes,
            string fullName, string currentItemName)
        {
            if (!historyAttributes.Any()) return; // Check if the current item doesn't have the attribute, then ignore

            var child = Activator.CreateInstance<CodeSourceObject>();
            child.Name = currentItemName;
            child.FullName = $"{fullName}.{currentItemName}";

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
        private static CodeSourceObjectHistory SetHistoryItemData(ICodeSourceAttribute historyAttribute,
            string fullName, string currentItemName)
        {
            var history = Activator.CreateInstance<CodeSourceObjectHistory>();

            history.AuthorName = historyAttribute.AuthorName;
            history.Comment = historyAttribute.Comment;
            history.Copyright = historyAttribute.Copyright;
            history.SourceUrl = historyAttribute.SourceUrl;
            history.AppliedOn = historyAttribute.AppliedOn;
            history.Version = historyAttribute.Version;
            history.CodePath = $"{fullName}{currentItemName}";

            return history;
        }
    }
}
#endif