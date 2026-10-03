// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:22
//  ***********************************************************************
//  <copyright file="CodeSourceAttribute.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using RzR.Core.CodeSource.Abstractions;
using RzR.Core.CodeSource.Extensions.Internal;

// ReSharper disable RedundantCast

#endregion

namespace RzR.Core.CodeSource
{
    /// <summary>
    ///     Attribute for code source. This class cannot be inherited.
    /// </summary>
    /// <seealso cref="T:Attribute"/>
    /// <seealso cref="T:RzR.Core.CodeSource.Abstractions.ICodeSourceAttribute"/>
    /// <seealso cref="T:CodeSource.Abstractions.ICodeSourceAttribute" />
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class CodeSourceAttribute : Attribute, ICodeSourceAttribute
    {
        /// <summary>
        ///     (Immutable)
        ///     The <see cref="ObsoleteAttribute" /> message shared by the positional constructors that
        ///     take two or three arguments.
        /// </summary>
        private const string PositionalArgumentsObsoleteMessage =
            "Use CodeSource(sourceUrl) with named properties, e.g. [CodeSource(\"url\", AuthorName = \"...\", Version = \"...\")]. Positional arguments bind by position.";

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSourceAttribute" /> class.
        /// </summary>
        /// <remarks>
        ///     Set the values through named properties. <see cref="Version" /> stays null and is
        ///     exported as "1.0".
        /// </remarks>
        public CodeSourceAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about its origin.
        /// </summary>
        /// <remarks>
        ///     Preferred constructor. Set the remaining values through named properties, e.g.
        ///     <c>[CodeSource("url", AuthorName = "...", Version = "...")]</c>. <see cref="Version" /> is
        ///     set to "1.0" unless the <see cref="Version" /> property is also set.
        /// </remarks>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        public CodeSourceAttribute(string sourceUrl)
        {
            SourceUrl = sourceUrl;
            Version = "1.0";
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about its origin.
        /// </summary>
        /// <remarks>
        ///     Obsolete. Positional arguments bind by position, so <c>[CodeSource("url", "John")]</c>
        ///     sets <see cref="Version" /> to "John". Use <see cref="CodeSourceAttribute(string)" />
        ///     with named properties instead, e.g. <c>[CodeSource("url", Version = "...")]</c>.
        /// </remarks>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="version">Change version.</param>
        [Obsolete(PositionalArgumentsObsoleteMessage)]
        public CodeSourceAttribute(
            string sourceUrl,
            string version)
        {
            SourceUrl = sourceUrl;
            Version = version;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about its origin.
        /// </summary>
        /// <remarks>
        ///     Obsolete. Positional arguments bind by position, so the second argument is always the
        ///     author name and the third is always the version. Use
        ///     <see cref="CodeSourceAttribute(string)" /> with named properties instead, e.g.
        ///     <c>[CodeSource("url", AuthorName = "...", Version = "...")]</c>.
        /// </remarks>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">Name of the code author.</param>
        /// <param name="version">Change version.</param>
        [Obsolete(PositionalArgumentsObsoleteMessage)]
        public CodeSourceAttribute(
            string sourceUrl,
            string authorName,
            string version)
        {
            SourceUrl = sourceUrl;
            AuthorName = authorName;
            Version = version;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about its origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">Name of the code author.</param>
        /// <param name="copyright">
        ///     Copyright of the code. A "©" prefix is added unless the value already starts with "©";
        ///     a missing value gives null.
        /// </param>
        /// <param name="version">Change version.</param>
        public CodeSourceAttribute(
            string sourceUrl,
            string authorName,
            string copyright,
            string version)
        {
            SourceUrl = sourceUrl;
            AuthorName = authorName;
            Copyright = copyright.SetCopyRight();
            Version = version;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about its origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">
        ///     (Optional) Optional. Name of the code author. The default value is null.
        /// </param>
        /// <param name="copyright">
        ///     (Optional) Optional. Copyright of the code. A "©" prefix is added unless the value
        ///     already starts with "©"; a missing value gives null. The default value is null.
        /// </param>
        /// <param name="appliedOn">
        ///     (Optional)
        ///     Optional. Date when applied code in local project. FORMAT: 'yyyy-MM-dd'. Stored as is in
        ///     <see cref="AppliedOn" />; an invalid date is not exported. The default value is null.
        /// </param>
        /// <param name="comment">
        ///     (Optional) Optional. Addition own comment. The default value is null.
        /// </param>
        /// <param name="version">(Optional) Change version. The default value is "1.0".</param>
        /// <param name="workItemId">
        ///     (Optional) Identifier for the work item (Related work item/task id). Maps to
        ///     <see cref="RelatedTaskId" />. The default value is null.
        /// </param>
        /// <param name="tags">(Optional) The tags.</param>
        public CodeSourceAttribute(
            string sourceUrl,
            string authorName = null,
            string copyright = null,
            string appliedOn = null,
            string comment = null,
            string version = "1.0",
            string workItemId = null,
            string tags = null)
        {
            SourceUrl = sourceUrl;
            AuthorName = authorName;
            Copyright = copyright.SetCopyRight();
            Comment = comment;
            AppliedOn = appliedOn;
            Version = version;
            RelatedTaskId = workItemId;
            Tags = tags;
        }

        /// <summary>
        ///     Gets the parsed <see cref="AppliedOn" /> date.
        /// </summary>
        /// <remarks>
        ///     Computed from the public <see cref="AppliedOn" /> value, so the constructor path and the
        ///     named-property path give the same result.
        /// </remarks>
        /// <value>
        ///     The applied date, or <see langword="null" /> when <see cref="AppliedOn" /> is missing or
        ///     is not a valid 'yyyy-MM-dd' date.
        /// </value>
        internal DateTime? InternalAppliedOn => AppliedOn.SetAppliedDate();

        /// <inheritdoc />
        public string SourceUrl { get; set; }

        /// <inheritdoc />
        public string AuthorName { get; set; }

        /// <inheritdoc />
        public string Copyright { get; set; }

        /// <inheritdoc />
        public string AppliedOn { get; set; }

        /// <inheritdoc />
        public string Comment { get; set; }

        /// <inheritdoc />
        public string Version { get; set; }

        /// <inheritdoc />
        public string Tags { get; set; }

        /// <inheritdoc />
        public string RelatedTaskId { get; set; }
    }
}