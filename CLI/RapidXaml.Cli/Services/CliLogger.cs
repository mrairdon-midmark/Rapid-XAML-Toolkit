// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using RapidXamlToolkit.Logging;

namespace RapidXaml.Cli.Services
{
    /// <summary>
    /// Logger implementation for CLI.
    /// </summary>
    public class CliLogger : ILogger
    {
        /// <inheritdoc/>
        public bool UseExtendedLogging { get; set; }

        /// <inheritdoc/>
        public void RecordError(string message, bool force = false)
        {
            Console.Error.WriteLine($"[ERROR] {message}");
        }

        /// <inheritdoc/>
        public void RecordException(Exception exception)
        {
            Console.Error.WriteLine($"[EXCEPTION] {exception}");
        }

        /// <inheritdoc/>
        public void RecordFeatureUsage(string feature, bool quiet = false)
        {
            // Silent for CLI
        }

        /// <inheritdoc/>
        public void RecordGeneralError(string message)
        {
            Console.Error.WriteLine($"[ERROR] {message}");
        }

        /// <inheritdoc/>
        public void RecordInfo(string message)
        {
            if (UseExtendedLogging)
            {
                Console.WriteLine($"[INFO] {message}");
            }
        }

        /// <inheritdoc/>
        public void RecordNotice(string message)
        {
            Console.WriteLine($"[NOTICE] {message}");
        }
    }
}
