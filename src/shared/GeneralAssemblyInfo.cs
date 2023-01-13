// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2022-12-12 19:36
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:48
// ***********************************************************************
//  <copyright file="GeneralAssemblyInfo.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Reflection;
using System.Resources;

#endregion

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("RzR ®")]
[assembly: AssemblyProduct("Code source attribute")]
[assembly: AssemblyCopyright("Copyright © 2022-2023 RzR All rights reserved.")]
[assembly: AssemblyTrademark("® RzR™")]
[assembly: AssemblyDescription("Provide an easy, accurate, and organized solution for storing data in your source code about some ideas, comments, or code references, which was an inspiration for realizing your current functionality.")]

#if NET45_OR_GREATER || NET || NETSTANDARD
[assembly: AssemblyMetadata("TermsOfService", "")]

[assembly: AssemblyMetadata("ContactUrl", "")]
[assembly: AssemblyMetadata("ContactName", "RzR")]
[assembly: AssemblyMetadata("ContactEmail", "ddpRzR@hotmail.com")]
#endif
#if NETSTANDARD1_6_OR_GREATER || NET35_OR_GREATER
[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]
#endif

[assembly: AssemblyVersion("1.0.3.0959")]
[assembly: AssemblyFileVersion("1.0.3.0959")]
[assembly: AssemblyInformationalVersion("1.0.3.x")]