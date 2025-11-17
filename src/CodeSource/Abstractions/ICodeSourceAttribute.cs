// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2023-10-08 22:04
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-12-23 19:53
// ***********************************************************************
//  <copyright file="ICodeSourceAttribute.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

namespace CodeSource.Abstractions
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     Code source attribute.
    /// </summary>
    /// =================================================================================================
    public interface ICodeSourceAttribute
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     URL to the original source of the code.
        /// </summary>
        /// <value>
        ///     The source URL.
        /// </value>
        /// =================================================================================================
        public string SourceUrl { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Name of the code author.
        /// </summary>
        /// <value>
        ///     The name of the author.
        /// </value>
        /// =================================================================================================
        public string AuthorName { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Copyright of the code.
        /// </summary>
        /// <value>
        ///     The copyright.
        /// </value>
        /// =================================================================================================
        public string Copyright { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Date of applied on current project. FORMAT: 'yyyy-MM-dd'.
        /// </summary>
        /// <value>
        ///     The applied on.
        /// </value>
        /// =================================================================================================
        public string AppliedOn { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Additional comment for applied code.
        /// </summary>
        /// <value>
        ///     The comment.
        /// </value>
        /// =================================================================================================
        public string Comment { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Change version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        /// =================================================================================================
        public double Version { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the tags.
        /// </summary>
        /// <value>
        ///     The tags.
        /// </value>
        /// <remarks>
        ///     All tags split by ';'
        ///     e.g., security, design-doc, todo.
        /// </remarks>
        /// =================================================================================================
        public string Tags { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets the identifier of the related task.
        /// </summary>
        /// <value>
        ///     The identifier of the related task.
        /// </value>
        /// <remarks>
        ///     Working item id: e.g., bug tracker, feature or task;
        ///     As in many tracking systems, the ids are notated with '#',
        ///      from the start. A good idea to set it as the '#123' format.
        /// </remarks>
        /// =================================================================================================
        public string RelatedTaskId { get; }
    }
}