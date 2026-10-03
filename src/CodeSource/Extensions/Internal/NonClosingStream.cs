// ***********************************************************************
//  Assembly          : RzR.Shared.Attributes.CodeSource
//  Author            : RzR
//  Created On        : 2026-10-01 20:09
// 
//  Last Modified By : RzR
//  Last Modified On : 2026-10-01 21:21
//  ***********************************************************************
//  <copyright file="NonClosingStream.cs" company="RzR SOFT & TECH">
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

#endregion

namespace RzR.Core.CodeSource.Extensions.Internal
{
    /// <summary>
    ///     A stream wrapper that delegates to an inner stream and never closes or disposes it.
    /// </summary>
    /// <seealso cref="T:System.IO.Stream"/>
    /// <seealso cref="T:Stream" />
    internal sealed class NonClosingStream : Stream
    {
        private readonly Stream _inner;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NonClosingStream" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown when <paramref name="inner" /> is null.
        /// </exception>
        /// <param name="inner">The caller-owned stream to delegate to.</param>
        internal NonClosingStream(Stream inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <inheritdoc />
        public override bool CanRead => _inner.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _inner.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _inner.CanWrite;

        /// <inheritdoc />
        public override long Length => _inner.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _inner.Seek(offset, origin);
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            _inner.SetLength(value);
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _inner.Read(buffer, offset, count);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            _inner.Write(buffer, offset, count);
        }

        /// <inheritdoc />
        public override void Flush()
        {
            _inner.Flush();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing) 
                _inner.Flush();

            base.Dispose(disposing);
        }
    }
}