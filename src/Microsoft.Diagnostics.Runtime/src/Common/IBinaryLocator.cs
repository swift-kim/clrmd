﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Runtime
{
    /// <summary>
    /// ClrMD needs to be able to locate files that were used when the process the DataTarget
    /// represents was running.
    /// 
    /// Implementers of this interface MUST be thread safe.
    /// </summary>
    public interface IBinaryLocator
    {
        /// <summary>
        /// Attempts to locate a binary via the symbol server.  This function will then copy the file
        /// locally to the symbol cache and return the location of the local file on disk.  May be called
        /// from multiple threads at the same time.
        /// </summary>
        /// <param name="fileName">The filename of the binary to locate, this may be a full path as a hint of where to look.</param>
        /// <param name="buildTimeStamp">The build timestamp the binary is indexed under.</param>
        /// <param name="imageSize">The image size the binary is indexed under.</param>
        /// <param name="checkProperties">Whether or not to validate the properties of the binary after download.</param>
        /// <returns>A full path on disk (local) of where the binary was copied to, null if it was not found.</returns>
        string? FindBinary(string fileName, uint buildTimeStamp, uint imageSize, bool checkProperties = true);

        /// <summary>
        /// Attempts to locate a binary via the symbol server.  This function will then copy the file
        /// locally to the symbol cache and return the location of the local file on disk.  May be called
        /// from multiple threads at the same time.
        /// </summary>
        /// <param name="fileName">The filename of the binary to locate, this may be a full path as a hint of where to look.</param>
        /// <param name="buildTimeStamp">The build timestamp the binary is indexed under.</param>
        /// <param name="imageSize">The image size the binary is indexed under.</param>
        /// <param name="checkProperties">Whether or not to validate the properties of the binary after download.</param>
        /// <returns>A full path on disk (local) of where the binary was copied to, null if it was not found.</returns>
        Task<string?> FindBinaryAsync(string fileName, uint buildTimeStamp, uint imageSize, bool checkProperties = true);
    }
}
