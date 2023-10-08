// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2023-10-08 22:04
// 
//  Last Modified By : RzR
//  Last Modified On : 2023-10-08 22:04
// ***********************************************************************
//  <copyright file="ICodeSourceAttribute.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

using System;

namespace CodeSource.Abstractions
{
    /// <summary>
    ///     Code source attribute DTO
    /// </summary>
    public interface ICodeSourceAttribute
    {
        /// <summary>
        ///     URL to the original source of the code.
        /// </summary>
        public string SourceUrl { get; }

        /// <summary>
        ///     Name of the code author.
        /// </summary>
        public string AuthorName { get; }

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
        ///     Additional comment for applied code.
        /// </summary>
        public string Comment { get; }

        /// <summary>
        ///     Change version
        /// </summary>
        public double Version { get; }
    }
}