// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="CodeSourceObjectHistory.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Globalization;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace RzR.Core.CodeSource.Models
{
    /// <summary>
    ///     A code source object history.
    /// </summary>
    public class CodeSourceObjectHistory
    {
        /// <summary>
        ///     Code execution path.
        /// </summary>
        /// <value>
        ///     The full pathname of the code file.
        /// </value>
        public string CodePath { get; internal set; }

        /// <summary>
        ///     URL to the original source of the code.
        /// </summary>
        /// <value>
        ///     The source URL.
        /// </value>
        public string SourceUrl { get; internal set; }

        /// <summary>
        ///     Name of the code author.
        /// </summary>
        /// <value>
        ///     The name of the author.
        /// </value>
        public string AuthorName { get; internal set; }

        /// <summary>
        ///     Copyright of the code.
        /// </summary>
        /// <value>
        ///     The copyright.
        /// </value>
        public string Copyright { get; internal set; }

        /// <summary>
        ///     Date of applied on current project. FORMAT: 'yyyy-MM-dd'.
        /// </summary>
        /// <value>
        ///     The applied on.
        /// </value>
        public DateTime? AppliedOn { get; internal set; }

        /// <summary>
        ///     Additional comments for applied code.
        /// </summary>
        /// <value>
        ///     The comment.
        /// </value>
        public string Comment { get; internal set; }

        /// <summary>
        ///     Change version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        public string Version { get; internal set; }

        /// <summary>
        ///     Gets the tags.
        /// </summary>
        /// <value>
        ///     The tags.
        /// </value>
        public string Tags { get; internal set; }

        /// <summary>
        ///     Gets the identifier of the related task.
        /// </summary>
        /// <value>
        ///     The identifier of the related task.
        /// </value>
        public string RelatedTaskId { get; internal set; }

        /// <summary>
        ///     Returns a string that represents the current history entry.
        /// </summary>
        /// <returns>
        ///     A string that represents the current history entry.
        /// </returns>
        public override string ToString()
        {
            return string.Format("v{0} by {1} on {2}", Version, AuthorName,
                AppliedOn?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }
    }
}