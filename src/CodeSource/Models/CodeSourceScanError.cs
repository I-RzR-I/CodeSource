// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="CodeSourceScanError.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RzR.Core.CodeSource.Extensions.Internal;

#endregion

namespace RzR.Core.CodeSource.Models
{
    /// <summary>
    ///     Describes a recoverable failure met during an annotation scan.
    /// </summary>
    public sealed class CodeSourceScanError
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CodeSourceScanError" /> class.
        /// </summary>
        /// <param name="stage">The scan stage at which the failure occurred.</param>
        /// <param name="assemblyName">The display name of the assembly.</param>
        /// <param name="typeName">The full name of the type, or null.</param>
        /// <param name="memberName">The member name, or null.</param>
        /// <param name="exception">
        ///     The exception, or null when <paramref name="description" /> is used.
        /// </param>
        /// <param name="description">
        ///     (Optional) The failure description used when there is no exception.
        /// </param>
        internal CodeSourceScanError(CodeSourceScanStage stage, string assemblyName, string typeName,
            string memberName, Exception exception, string description = null)
        {
            Stage = stage;
            AssemblyName = assemblyName;
            TypeName = typeName;
            MemberName = memberName;
            Exception = exception;
            Message = BuildMessage(stage, assemblyName, typeName, memberName, exception, description);
        }

        /// <summary>
        ///     Gets the scan stage at which the failure occurred.
        /// </summary>
        /// <value>
        ///     The stage.
        /// </value>
        public CodeSourceScanStage Stage { get; }

        /// <summary>
        ///     Gets the display name of the assembly being scanned or loaded.
        /// </summary>
        /// <value>
        ///     The name of the assembly.
        /// </value>
        public string AssemblyName { get; }

        /// <summary>
        ///     Gets the full name of the type being scanned.
        /// </summary>
        /// <value>
        ///     The name of the type, or null when the failure is at assembly level.
        /// </value>
        public string TypeName { get; }

        /// <summary>
        ///     Gets the name of the member being scanned.
        /// </summary>
        /// <value>
        ///     The name of the member, or null when the failure is at assembly or type level.
        /// </value>
        public string MemberName { get; }

        /// <summary>
        ///     Gets a human-readable description of the failure.
        /// </summary>
        /// <value>
        ///     The message.
        /// </value>
        public string Message { get; }

        /// <summary>
        ///     Gets the exception that caused the failure.
        /// </summary>
        /// <value>
        ///     The exception, or null for <see cref="CodeSourceScanStage.AttributeValue" />.
        /// </value>
        public Exception Exception { get; }

        /// <summary>
        ///     Returns the failure message.
        /// </summary>
        /// <returns>
        ///     The value of <see cref="Message" />.
        /// </returns>
        public override string ToString()
        {
            return Message;
        }

        /// <summary>
        ///     Builds the failure message from names and exception messages only.
        /// </summary>
        /// <param name="stage">The scan stage.</param>
        /// <param name="assemblyName">The name of the assembly.</param>
        /// <param name="typeName">The name of the type.</param>
        /// <param name="memberName">The name of the member.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="description">The failure description used when there is no exception.</param>
        /// <returns>
        ///     The message.
        /// </returns>
        private static string BuildMessage(CodeSourceScanStage stage, string assemblyName, string typeName,
            string memberName, Exception exception, string description)
        {
            var builder = new StringBuilder();
            builder.Append(stage).Append(": ");

            if (exception.IsNotNull())
            {
                builder.Append(exception.GetType().FullName).Append(": ").Append(exception.Message);

                var loaderMessages = GetLoaderMessages(exception);
                if (loaderMessages.Count > 0)
                    builder.Append(" Loader exceptions: ").Append(string.Join(" | ", loaderMessages.ToArray()));
            }
            else
            {
                builder.Append(description);
            }

            AppendName(builder, "assembly", assemblyName);
            AppendName(builder, "type", typeName);
            AppendName(builder, "member", memberName);

            return builder.ToString();
        }

        /// <summary>
        ///     Gets the distinct loader exception messages of a <see cref="ReflectionTypeLoadException" />
        ///     .
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///     The loader messages, or an empty list for any other exception type.
        /// </returns>
        private static List<string> GetLoaderMessages(Exception exception)
        {
            var typeLoadException = exception as ReflectionTypeLoadException;
            if (typeLoadException?.LoaderExceptions == null)
                return new List<string>();

            return typeLoadException.LoaderExceptions
                .Where(x => x.IsNotNull())
                .Select(x => x.Message)
                .Distinct()
                .ToList();
        }

        /// <summary>
        ///     Appends a named value when it is present.
        /// </summary>
        /// <param name="builder">The message builder.</param>
        /// <param name="label">The label.</param>
        /// <param name="value">The value.</param>
        private static void AppendName(StringBuilder builder, string label, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            builder.Append(" [").Append(label).Append(": ").Append(value).Append(']');
        }
    }
}