// ***********************************************************************
//  Assembly         : RzR.Shared.Attributes.CodeSource
//  Author           : RzR
//  Created On       : 2022-12-13 13:32
// 
//  Last Modified By : RzR
//  Last Modified On : 2022-12-16 22:48
// ***********************************************************************
//  <copyright file="CodeSourceResultDto.cs" company="">
//   Copyright (c) RzR. All rights reserved.
//  </copyright>
// 
//  <summary>
//  </summary>
// ***********************************************************************

#region U S A G E S

using System.Collections.Generic;

#endregion

namespace CodeSource.Models
{
    /// <summary>
    ///     Code source result
    /// </summary>
    /// <remarks></remarks>
    public class CodeSourceResultDto
    {
        /// <summary>
        ///     Gets or sets code source parent data.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public CodeSourceDto Parent { get; set; }

        /// <summary>
        ///     Gets or sets code source child data.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public List<CodeSourceDto> Children { get; set; }
    }
}