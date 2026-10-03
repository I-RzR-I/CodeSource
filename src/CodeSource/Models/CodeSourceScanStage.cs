// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="CodeSourceScanStage.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

namespace RzR.Core.CodeSource.Models
{
    /// <summary>
    ///     The stage of an annotation scan at which a recoverable failure occurred.
    /// </summary>
    public enum CodeSourceScanStage
    {
        /// <summary>
        ///     Loading a referenced assembly by name failed. The reference was skipped.
        /// </summary>
        AssemblyLoad = 0,

        /// <summary>
        ///     Enumerating the exported types of an assembly failed. The assembly was skipped.
        /// </summary>
        TypeEnumeration = 1,

        /// <summary>
        ///     Enumerating the constructors or the methods of a type failed. That member kind was skipped.
        /// </summary>
        MemberEnumeration = 2,

        /// <summary>
        ///     Reading the <see cref="CodeSourceAttribute" /> instances of a type or member failed. That
        ///     type or member was skipped.
        /// </summary>
        AttributeRead = 3,

        /// <summary>
        ///     An attribute value is present but invalid, for example an <c>AppliedOn</c> value that is
        ///     not a 'yyyy-MM-dd' date. The annotation is still returned, with that value unset.
        /// </summary>
        AttributeValue = 4
    }
}