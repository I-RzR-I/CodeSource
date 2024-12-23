// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2022-12-12 19:54
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:48
// ***********************************************************************
//  <copyright file="CodeSourceAttribute.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;
using System.Globalization;
using CodeSource.Abstractions;

// ReSharper disable RedundantCast

#endregion

namespace CodeSource
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Attribute for code source. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="T:Attribute"/>
    /// <seealso cref="T:CodeSource.Abstractions.ICodeSourceAttribute"/>
    /// =================================================================================================
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class CodeSourceAttribute : Attribute, ICodeSourceAttribute
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about it's origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="version">(Optional) Change version.</param>
        /// =================================================================================================
        public CodeSourceAttribute(string sourceUrl, double version = 1.0)
        {
            SourceUrl = sourceUrl;
            Version = version;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about it's origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">
        ///     (Optional) Optional. Name of the code author. The default value is null.
        /// </param>
        /// <param name="version">(Optional) Change version.</param>
        /// =================================================================================================
        public CodeSourceAttribute(string sourceUrl, string authorName = null, double version = 1.0)
        {
            SourceUrl = sourceUrl;
            AuthorName = authorName;
            Version = version;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about it's origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">
        ///     (Optional) Optional. Name of the code author. The default value is null.
        /// </param>
        /// <param name="copyright">
        ///     (Optional) Optional. Copyright of the code. The default value is null.
        /// </param>
        /// <param name="version">(Optional) Change version.</param>
        /// =================================================================================================
        public CodeSourceAttribute(string sourceUrl, string authorName = null, string copyright = null, double version = 1.0)
        {
            SourceUrl = sourceUrl;
            AuthorName = authorName;
            Copyright = $"{(!string.IsNullOrEmpty(copyright?.Trim()) ? $"© {copyright}" : copyright)}";
            Version = version;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about it's origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">
        ///     (Optional) Optional. Name of the code author. The default value is null.
        /// </param>
        /// <param name="copyright">
        ///     (Optional) Optional. Copyright of the code. The default value is null.
        /// </param>
        /// <param name="appliedOn">
        ///     (Optional)
        ///     Optional. Date when applied code in local project. FORMAT: 'yyyy-MM-dd'. The default
        ///     value is null.
        /// </param>
        /// <param name="comment">
        ///     (Optional) Optional. Addition own comment. The default value is null.
        /// </param>
        /// <param name="version">(Optional) Change version.</param>
        /// =================================================================================================
        public CodeSourceAttribute(string sourceUrl, string authorName = null, string copyright = null,
            string appliedOn = null, string comment = null, double version = 1.0)
        {
            SourceUrl = sourceUrl;
            AuthorName = authorName;
            Copyright = $"{(!string.IsNullOrEmpty(copyright?.Trim()) ? $"© {copyright}" : copyright)}";
            Comment = comment;
            AppliedOn = string.IsNullOrEmpty((appliedOn ?? "").Trim())
                ? (DateTime?)null
                : DateTime.ParseExact(appliedOn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            Version = version;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSourceAttribute"/> class.
        /// </summary>
        /// =================================================================================================
        public CodeSourceAttribute() { }

        /// <inheritdoc/>
        public string SourceUrl { get; set; }

        /// <inheritdoc/>
        public string AuthorName { get; set; }

        /// <inheritdoc/>
        public string Copyright { get; set; }

        /// <inheritdoc/>
        public DateTime? AppliedOn { get; private set; }

        /// <inheritdoc/>
        public string Comment { get; set; }

        /// <inheritdoc/>
        public double Version { get; set; }
    }
}