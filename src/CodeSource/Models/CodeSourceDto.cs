// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2022-12-13 01:45
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:48
// ***********************************************************************
//  <copyright file="CodeSourceDto.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System;

// ReSharper disable ClassNeverInstantiated.Global

#endregion

namespace CodeSource.Models
{
    /// <summary>
    ///     Code source result
    /// </summary>
    public class CodeSourceDto
    {
        /// <summary>
        ///     Code execution path.
        /// </summary>
        public string CodePath { get; internal set; }

        /// <summary>
        ///     URL to the original source of the code.
        /// </summary>
        public string SourceUrl { get; internal set; }

        /// <summary>
        ///     Name of the code author.
        /// </summary>
        public string AuthorName { get; internal set; }

        /// <summary>
        ///     Copyright of the code.
        /// </summary>
        public string Copyright { get; internal set; }

        /// <summary>
        ///     Date of applied on current project.
        ///     FORMAT: 'yyyy-MM-dd'
        /// </summary>
        public DateTime? AppliedOn { get; internal set; }

        /// <summary>
        ///     Additional comments for applied code.
        /// </summary>
        public string Comment { get; internal set; }

        /// <summary>
        ///     Change version.
        /// </summary>
        public double Version { get; internal set; }
    }
}