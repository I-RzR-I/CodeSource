// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="ICodeSourceAttribute.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

namespace RzR.Core.CodeSource.Abstractions
{
    /// <summary>
    ///     Code source attribute.
    /// </summary>
    public interface ICodeSourceAttribute
    {
        /// <summary>
        ///     URL to the original source of the code.
        /// </summary>
        /// <value>
        ///     The source URL.
        /// </value>
        public string SourceUrl { get; }

        /// <summary>
        ///     Name of the code author.
        /// </summary>
        /// <value>
        ///     The name of the author.
        /// </value>
        public string AuthorName { get; }

        /// <summary>
        ///     Copyright of the code.
        /// </summary>
        /// <remarks>
        ///     The constructor parameter <c>copyright</c> adds a "©" prefix (unless the value already
        ///     starts with "©") and gives null for a missing value. The named property keeps the value
        ///     as set, with no prefix.
        /// </remarks>
        /// <value>
        ///     The copyright.
        /// </value>
        public string Copyright { get; }

        /// <summary>
        ///     Date of applied on current project. FORMAT: 'yyyy-MM-dd'.
        /// </summary>
        /// <remarks>
        ///     Holds the value as given, through the constructor parameter <c>appliedOn</c> or the named
        ///     property. A value that is not a valid 'yyyy-MM-dd' date is not exported.
        /// </remarks>
        /// <value>
        ///     The applied on.
        /// </value>
        public string AppliedOn { get; }

        /// <summary>
        ///     Additional comment for applied code.
        /// </summary>
        /// <value>
        ///     The comment.
        /// </value>
        public string Comment { get; }

        /// <summary>
        ///     Change version.
        /// </summary>
        /// <remarks>
        ///     A missing value is exported as "1.0".
        /// </remarks>
        /// <value>
        ///     The version.
        /// </value>
        public string Version { get; }

        /// <summary>
        ///     Gets the tags.
        /// </summary>
        /// <remarks>
        ///     All tags split by ';'
        ///     e.g., security, design-doc, todo.
        /// </remarks>
        /// <value>
        ///     The tags.
        /// </value>
        public string Tags { get; }

        /// <summary>
        ///     Gets the identifier of the related task.
        /// </summary>
        /// <remarks>
        ///     Working item id: e.g., bug tracker, feature or task;
        ///     As in many tracking systems, the ids are notated with '#', from the start. A good idea to
        ///     set it as the '#123' format. The constructor parameter <c>workItemId</c> maps to this
        ///     property.
        /// </remarks>
        /// <value>
        ///     The identifier of the related task.
        /// </value>
        public string RelatedTaskId { get; }
    }
}