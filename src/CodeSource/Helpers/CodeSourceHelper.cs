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
using CodeSource.Models;

#endregion

namespace CodeSource.Helpers
{
    /// <summary>
    ///     Code source helper methods
    /// </summary>
    public static class CodeSourceHelper
    {
        /// <summary>
        ///     Get assembly code source
        /// </summary>
        /// <param name="assembly">Optional. The default value is null.</param>
        /// <returns></returns>
        /// <remarks>Running this method can take some time in case when you have a big project.</remarks>
        public static IEnumerable<CodeSourceResultDto> GetCodeSourceAssembly(string assembly = null)
        {
            try
            {
                var codeSource = new List<CodeSourceResultDto>();
                foreach (var localAssembly in GetListOfEntryAssemblyWithReferences(assembly))
                    foreach (var t in localAssembly.ExportedTypes)
                    {
                        var ti = t.GetTypeInfo();
                        var obj = new CodeSourceResultDto { Children = new List<CodeSourceDto>() };

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
        
        /// <summary>
        ///     Get entry assembly with references
        /// </summary>
        /// <param name="assembly">Optional. The default value is null.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        private static IEnumerable<Assembly> GetListOfEntryAssemblyWithReferences(string assembly = null)
        {
            Assembly mainAsm;
            var listOfAssemblies = new List<Assembly>();
            if (string.IsNullOrEmpty(assembly))
                mainAsm = Assembly.GetEntryAssembly();
            mainAsm = Assembly.Load(new AssemblyName(assembly));

            listOfAssemblies.Add(mainAsm);
            listOfAssemblies.AddRange(mainAsm.GetReferencedAssemblies().Select(refAsmName => Assembly.Load(refAsmName)));

            return listOfAssemblies;
        }

        /// <summary>
        ///     Get Classes code source details
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        /// <param name="dataObject">Current result object data</param>
        /// <remarks></remarks>
        private static void GetClassCodeSource(ref TypeInfo typeInfo, ref CodeSourceResultDto dataObject)
        {
            try
            {
                var parent = new CodeSourceDto { CodePath = typeInfo.FullName };
                var classAttributes = typeInfo.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();

                foreach (var atr in classAttributes)
                {
                    var attribute = (CodeSourceAttribute)atr;
                    parent.AuthorName = attribute.AuthorName;
                    parent.Comment = attribute.Comment;
                    parent.Copyright = attribute.Copyright;
                    parent.SourceUrl = attribute.SourceUrl;
                    parent.AppliedOn = attribute.AppliedOn;

                    dataObject.Parent = parent;
                }
            }
            catch
            {
                /*ignored*/
            }
        }

        /// <summary>
        ///     Get CTOR code source details
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        /// <param name="dataObject">Current result object data</param>
        /// <remarks></remarks>
        private static void GetCtorCodeSource(ref TypeInfo typeInfo, ref CodeSourceResultDto dataObject)
        {
            try
            {
                foreach (var ctor in typeInfo.GetConstructors())
                {
                    var ctorAttributes = ctor.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
                    if (ctorAttributes.Any() && dataObject.Parent == null)
                        dataObject.Parent = new CodeSourceDto { CodePath = typeInfo.FullName };

                    foreach (var atr in ctorAttributes)
                    {
                        var attribute = (CodeSourceAttribute)atr;
                        var child = new CodeSourceDto
                        {
                            CodePath = $"{typeInfo.FullName}{ctor.Name}",
                            AuthorName = attribute.AuthorName,
                            Comment = attribute.Comment,
                            Copyright = attribute.Copyright,
                            SourceUrl = attribute.SourceUrl,
                            AppliedOn = attribute.AppliedOn
                        };

                        dataObject.Children.Add(child);
                    }
                }
            }
            catch
            {
                /*ignored*/
            }
        }

        /// <summary>
        ///     Get methods code source details
        /// </summary>
        /// <param name="typeInfo">Type info</param>
        /// <param name="dataObject">Current result object data</param>
        /// <param name="exportedTypes">Exported assembly type</param>
        /// <remarks></remarks>
        private static void GetMethodCodeSource(ref TypeInfo typeInfo, ref CodeSourceResultDto dataObject,
            Type exportedTypes)
        {
            try
            {
                var classMethods = exportedTypes.GetRuntimeMethods();
                foreach (var m in classMethods)
                {
                    var classMethodAttributes = m.GetCustomAttributes(typeof(CodeSourceAttribute)).ToList();
                    if (classMethodAttributes.Any() && dataObject.Parent == null)
                        dataObject.Parent = new CodeSourceDto { CodePath = typeInfo.FullName };

                    foreach (var atr in classMethodAttributes)
                    {
                        var attribute = (CodeSourceAttribute)atr;
                        var child = new CodeSourceDto
                        {
                            CodePath = $"{typeInfo.FullName}.{m.Name}",
                            AuthorName = attribute.AuthorName,
                            Comment = attribute.Comment,
                            Copyright = attribute.Copyright,
                            SourceUrl = attribute.SourceUrl,
                            AppliedOn = attribute.AppliedOn
                        };

                        dataObject.Children.Add(child);
                    }
                }
            }
            catch
            {
                /*ignored*/
            }
        }
    }
}