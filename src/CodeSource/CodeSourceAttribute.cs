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
using CodeSource.Abstractions;
using CodeSource.Extensions.Internal;

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
        private DateTime? _internalAppliedOn;
        private string _sourceUrl;

        internal DateTime? InternalAppliedOn
        {
            get => AppliedOn.IsPresent() 
                ? AppliedOn.SetAppliedDate() 
                : _internalAppliedOn;
            private set => _internalAppliedOn = value;
        }

        /// <inheritdoc/>
        public string SourceUrl
        {
            get => _sourceUrl;
            set
            {
                value.ValidateSourceUrl();

                _sourceUrl = value;
            }
        }

        /// <inheritdoc/>
        public string AuthorName { get; set; }

        /// <inheritdoc/>
        public string Copyright { get; set; }

        /// <inheritdoc/>
        public string AppliedOn { get; set; }

        /// <inheritdoc/>
        public string Comment { get; set; }

        /// <inheritdoc/>
        public double Version { get; set; }

        /// <inheritdoc/>
        public string Tags { get; set; }

        /// <inheritdoc/>
        public string RelatedTaskId { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSourceAttribute"/> class.
        /// </summary>
        /// =================================================================================================
        public CodeSourceAttribute() { }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about its origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="version">(Optional) Change version.</param>
        /// =================================================================================================
        public CodeSourceAttribute(
            string sourceUrl,
            double version = 1.0)
        {
            sourceUrl.ValidateSourceUrl();

            SourceUrl = sourceUrl;
            Version = version;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about its origin.
        /// </summary>
        /// <param name="sourceUrl">Required. URL to the original source of the code.</param>
        /// <param name="authorName">
        ///     (Optional) Optional. Name of the code author. The default value is null.
        /// </param>
        /// <param name="version">(Optional) Change version.</param>
        /// =================================================================================================
        public CodeSourceAttribute(
            string sourceUrl,
            string authorName = null,
            double version = 1.0)
        {
            sourceUrl.ValidateSourceUrl();

            SourceUrl = sourceUrl;
            AuthorName = authorName;
            Version = version;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about its origin.
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
        public CodeSourceAttribute(
            string sourceUrl,
            string authorName = null,
            string copyright = null,
            double version = 1.0)
        {
            sourceUrl.ValidateSourceUrl();

            SourceUrl = sourceUrl;
            AuthorName = authorName;
            Copyright = copyright.SetCopyRight();
            Version = version;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSource.CodeSourceAttribute" /> class.
        ///     Decorates the code with information about its origin.
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
        /// <param name="workItemId">(Optional) Identifier for the work item (Related work item/task id).</param>
        /// <param name="tags">(Optional) The tags.</param>
        /// =================================================================================================
        public CodeSourceAttribute(
            string sourceUrl,
            string authorName = null,
            string copyright = null,
            string appliedOn = null,
            string comment = null,
            double version = 1.0,
            string workItemId = null,
            string tags = null)
        {
            sourceUrl.ValidateSourceUrl();

            SourceUrl = sourceUrl;
            AuthorName = authorName;
            Copyright = copyright.SetCopyRight();
            Comment = comment;
            InternalAppliedOn = appliedOn.SetAppliedDate();
            Version = version;
            RelatedTaskId = workItemId;
            Tags = tags;
        }
    }
}