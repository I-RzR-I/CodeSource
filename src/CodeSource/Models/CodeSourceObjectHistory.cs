// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2024-12-23 15:31
// 
//  Last Modified By : RzR
//  Last Modified On : 2024-12-23 19:46
// ***********************************************************************
//  <copyright file="CodeSourceObjectHistory.cs" company="RzR SOFT & TECH">
//   Copyright © RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace CodeSource.Models
{
    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    ///     A code source object history.
    /// </summary>
    /// =================================================================================================
    public class CodeSourceObjectHistory
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Code execution path.
        /// </summary>
        /// <value>
        ///     The full pathname of the code file.
        /// </value>
        /// =================================================================================================
        public string CodePath { get; internal set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     URL to the original source of the code.
        /// </summary>
        /// <value>
        ///     The source URL.
        /// </value>
        /// =================================================================================================
        public string SourceUrl { get; internal set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Name of the code author.
        /// </summary>
        /// <value>
        ///     The name of the author.
        /// </value>
        /// =================================================================================================
        public string AuthorName { get; internal set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Copyright of the code.
        /// </summary>
        /// <value>
        ///     The copyright.
        /// </value>
        /// =================================================================================================
        public string Copyright { get; internal set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Date of applied on current project. FORMAT: 'yyyy-MM-dd'.
        /// </summary>
        /// <value>
        ///     The applied on.
        /// </value>
        /// =================================================================================================
        public DateTime? AppliedOn { get; internal set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Additional comments for applied code.
        /// </summary>
        /// <value>
        ///     The comment.
        /// </value>
        /// =================================================================================================
        public string Comment { get; internal set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Change version.
        /// </summary>
        /// <value>
        ///     The version.
        /// </value>
        /// =================================================================================================
        public double Version { get; internal set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the tags.
        /// </summary>
        /// <value>
        ///     The tags.
        /// </value>
        /// =================================================================================================
        public string Tags { get; internal set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        ///     Gets or sets the identifier of the related task.
        /// </summary>
        /// <value>
        ///     The identifier of the related task.
        /// </value>
        /// =================================================================================================
        public string RelatedTaskId { get; internal set; }
    }
}