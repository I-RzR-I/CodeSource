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
using System.Collections.Generic;
using System.Globalization;

#endregion

namespace CodeSource
{
    /// <summary>
    ///     Decorates the code with information about it's origin.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class CodeSourceAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about it's origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">Optional. Name of the code author. The default value is null.</param>
        /// <param name="copyright">Optional. Copyright of the code. The default value is null.</param>
        /// <remarks></remarks>
        public CodeSourceAttribute(string sourceUrl, string authorName = null, string copyright = null)
        {
            SourceUrl = sourceUrl;
            AuthorsName = new List<string>(1) {authorName};
            Copyright = $"{(!string.IsNullOrEmpty(copyright?.Trim()) ? $"© {copyright}" : copyright)}";
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about it's origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorsName">Optional. Name of the code authors. The default value is null.</param>
        /// <param name="copyright">Optional. Copyright of the code. The default value is null.</param>
        /// <remarks></remarks>
        public CodeSourceAttribute(string sourceUrl, List<string> authorsName = null, string copyright = null)
        {
            SourceUrl = sourceUrl;
            AuthorsName = authorsName;
            Copyright = $"{(!string.IsNullOrEmpty(copyright?.Trim()) ? $"© {copyright}" : copyright)}";
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about it's origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">Optional. Name of the code author. The default value is null.</param>
        /// <param name="copyright">Optional. Copyright of the code. The default value is null.</param>
        /// <param name="appliedOn">
        ///     Optional. Date when applied code in local project. FORMAT: 'yyyy-MM-dd'. The default value is
        ///     null.
        /// </param>
        /// <param name="comment">Optional. Addition own comment. The default value is null.</param>
        /// <remarks></remarks>
        public CodeSourceAttribute(string sourceUrl, string authorName = null, string copyright = null,
            string appliedOn = null, string comment = null)
        {
            SourceUrl = sourceUrl;
            AuthorsName = new List<string>(1) {authorName};
            Copyright = $"{(!string.IsNullOrEmpty(copyright?.Trim()) ? $"© {copyright}" : copyright)}";
            Comments = new List<string>(1) {comment};
            AppliedOn = string.IsNullOrEmpty((appliedOn ?? "").Trim())
                ? (DateTime?) null
                : DateTime.ParseExact(appliedOn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about it's origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">Optional. Name of the code author. The default value is null.</param>
        /// <param name="copyright">Optional. Copyright of the code. The default value is null.</param>
        /// <param name="appliedOn">
        ///     Optional. Date when applied code in local project. FORMAT: 'yyyy-MM-dd'. The default value is
        ///     null.
        /// </param>
        /// <param name="comments">Optional. Addition own comments. The default value is null.</param>
        /// <remarks></remarks>
        public CodeSourceAttribute(string sourceUrl, string authorName = null, string copyright = null,
            string appliedOn = null, List<string> comments = null)
        {
            SourceUrl = sourceUrl;
            AuthorsName = new List<string>(1) {authorName};
            Copyright = $"{(!string.IsNullOrEmpty(copyright?.Trim()) ? $"© {copyright}" : copyright)}";
            Comments = comments;
            AppliedOn = string.IsNullOrEmpty((appliedOn ?? "").Trim())
                ? (DateTime?) null
                : DateTime.ParseExact(appliedOn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about it's origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorsName">Optional. Name of the code authors. The default value is null.</param>
        /// <param name="copyright">Optional. Copyright of the code. The default value is null.</param>
        /// <param name="appliedOn">
        ///     Optional. Date when applied code in local project. FORMAT: 'yyyy-MM-dd'. The default value is
        ///     null.
        /// </param>
        /// <param name="comments">Optional. Addition own comments. The default value is null.</param>
        /// <remarks></remarks>
        public CodeSourceAttribute(string sourceUrl, List<string> authorsName = null, string copyright = null,
            string appliedOn = null, List<string> comments = null)
        {
            SourceUrl = sourceUrl;
            AuthorsName = authorsName;
            Copyright = $"{(!string.IsNullOrEmpty(copyright?.Trim()) ? $"© {copyright}" : copyright)}";
            Comments = comments;
            AppliedOn = string.IsNullOrEmpty((appliedOn ?? "").Trim())
                ? (DateTime?) null
                : DateTime.ParseExact(appliedOn, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     URL to the original source of the code.
        /// </summary>
        public string SourceUrl { get; }

        /// <summary>
        ///     Name of the code author.
        /// </summary>
        public List<string> AuthorsName { get; }

        /// <summary>
        ///     Copyright of the code.
        /// </summary>
        public string Copyright { get; }

        /// <summary>
        ///     Date of applied on current project.
        ///     FORMAT: 'yyyy-MM-dd'
        /// </summary>
        public DateTime? AppliedOn { get; }

        /// <summary>
        ///     Additional comments for applied code.
        /// </summary>
        public List<string> Comments { get; }
    }
}