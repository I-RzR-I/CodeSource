// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="ScanExceptionFilter.cs" company="RzR SOFT & TECH">
//      Copyright (c) RzR. All rights reserved.
//  </copyright>
//  <contact>
//      https://iamrzr.dev/contact
//  </contact>
//  <summary></summary>
//  ***********************************************************************

#region U S I N G

using System;
using System.IO;
using System.Reflection;
using System.Threading;
using RzR.Core.CodeSource.Extensions.Internal;
using RzR.Core.CodeSource.Models;

#endregion

namespace RzR.Core.CodeSource.Helpers
{
    /// <summary>
    ///     Decides which exceptions raised while scanning for annotations are recoverable, per scan
    ///     stage.
    /// </summary>
    internal static class ScanExceptionFilter
    {
        /// <summary>
        ///     Query if <paramref name="exception" /> is a recoverable failure for the given scan stage.
        /// </summary>
        /// <param name="exception">The exception raised by the scan step.</param>
        /// <param name="stage">The scan stage that raised the exception.</param>
        /// <returns>
        ///     True if the scan may report the failure and continue, false if the exception must
        ///     propagate.
        /// </returns>
        internal static bool IsRecoverable(Exception exception, CodeSourceScanStage stage)
        {
            if (exception.IsNull() || IsFatal(exception))
                return false;

            switch (stage)
            {
                case CodeSourceScanStage.AssemblyLoad:
                    return exception is NotSupportedException
                           || IsAssemblyFileFailure(exception);

                case CodeSourceScanStage.TypeEnumeration:
                    return exception is ReflectionTypeLoadException
                           || exception is TypeLoadException
                           || exception is NotSupportedException
                           || IsAssemblyFileFailure(exception);

                case CodeSourceScanStage.MemberEnumeration:
                    return exception is TypeLoadException
                           || IsAssemblyFileFailure(exception);

                case CodeSourceScanStage.AttributeRead:
                    return exception is TypeLoadException
                           || exception is NotSupportedException
                           || IsCustomAttributeFormatFailure(exception)
                           || exception is MemberAccessException
                           || exception is TargetInvocationException
                           || exception is InvalidOperationException
                           || IsAssemblyFileFailure(exception);

                default:
                    return false;
            }
        }

        /// <summary>
        ///     Query if <paramref name="exception" /> is a failure to locate, load or read an assembly
        ///     file.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///     True if the exception is a loader file failure, false if not.
        /// </returns>
        private static bool IsAssemblyFileFailure(Exception exception)
        {
            return exception is FileNotFoundException
#if NETSTANDARD1_0
                   || IsExactType(exception, "System.IO.FileLoadException")
#else
                   || exception is FileLoadException
#endif
                   || exception is BadImageFormatException;
        }

        /// <summary>
        ///     Query if <paramref name="exception" /> reports a malformed custom attribute blob.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///     True if the exception is a custom attribute format failure, false if not.
        /// </returns>
        private static bool IsCustomAttributeFormatFailure(Exception exception)
        {
#if NETSTANDARD1_0 || NETSTANDARD1_5
            return IsExactType(exception, "System.Reflection.CustomAttributeFormatException");
#else
            return exception is CustomAttributeFormatException;
#endif
        }

#if NETSTANDARD1_0 || NETSTANDARD1_5

        /// <summary>
        ///     Query if the runtime type of <paramref name="exception" /> has the given full name.
        /// </summary>
        /// <remarks>
        ///     Used for exception types that exist at run time but are absent from the netstandard1.x
        ///     reference surface, so they cannot be named in an <c>is</c> check.
        /// </remarks>
        /// <param name="exception">The exception.</param>
        /// <param name="fullName">The full name of the exception type.</param>
        /// <returns>
        ///     True if the names match, false if not.
        /// </returns>
        private static bool IsExactType(Exception exception, string fullName)
        {
            return string.Equals(exception.GetType().FullName, fullName, StringComparison.Ordinal);
        }
#endif

        /// <summary>
        ///     Query if <paramref name="exception" /> is a process-level failure that must never be
        ///     handled.
        /// </summary>
        /// <remarks>
        ///     A <see cref="TargetInvocationException" /> that wraps a fatal exception, for example an
        ///     attribute constructor that ran out of memory, is fatal as well.
        /// </remarks>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///     True if fatal, false if not.
        /// </returns>
        private static bool IsFatal(Exception exception)
        {
            if (IsFatalType(exception))
                return true;

            return exception is TargetInvocationException
                   && exception.InnerException != null
                   && IsFatalType(exception.InnerException);
        }

        /// <summary>
        ///     Query if <paramref name="exception" /> is one of the fatal runtime exception types.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>
        ///     True if fatal, false if not.
        /// </returns>
        private static bool IsFatalType(Exception exception)
        {
#if NETSTANDARD1_0 || NETSTANDARD1_5
            return exception is OutOfMemoryException;
#else
            return exception is OutOfMemoryException
                   || exception is StackOverflowException
                   || exception is ThreadAbortException;
#endif
        }
    }
}