// Copyright (c) Matt Lacey Ltd. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using RapidXamlToolkit.XamlAnalysis;

namespace RapidXaml.Cli.Services
{
    /// <summary>
    /// CLI implementation of text snapshot for XAML analysis.
    /// </summary>
    public class CliTextSnapshot : ITextSnapshot
    {
        private readonly string[] lines;

        /// <summary>
        /// Initializes a new instance of the <see cref="CliTextSnapshot"/> class.
        /// </summary>
        /// <param name="filePath">Path to the XAML file.</param>
        public CliTextSnapshot(string filePath)
        {
            var content = File.ReadAllText(filePath);
            this.lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            this.Length = content.Length;
        }

        /// <inheritdoc/>
        public int Length { get; }

        /// <inheritdoc/>
        public int LineCount => this.lines.Length;

        /// <inheritdoc/>
        public string GetText()
        {
            return string.Join(Environment.NewLine, this.lines);
        }

        /// <inheritdoc/>
        public ITextSnapshotLine GetLineFromLineNumber(int lineNumber)
        {
            if (lineNumber < 0 || lineNumber >= this.lines.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(lineNumber));
            }

            return new CliTextSnapshotLine(this.lines[lineNumber], lineNumber);
        }

        /// <inheritdoc/>
        public ITextSnapshotLine GetLineFromPosition(int position)
        {
            int currentPos = 0;
            for (int i = 0; i < this.lines.Length; i++)
            {
                int lineLength = this.lines[i].Length + Environment.NewLine.Length;
                if (currentPos + lineLength > position)
                {
                    return new CliTextSnapshotLine(this.lines[i], i);
                }
                currentPos += lineLength;
            }

            return new CliTextSnapshotLine(this.lines[this.lines.Length - 1], this.lines.Length - 1);
        }
    }

    /// <summary>
    /// CLI implementation of text snapshot line.
    /// </summary>
    public class CliTextSnapshotLine : ITextSnapshotLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CliTextSnapshotLine"/> class.
        /// </summary>
        /// <param name="text">Line text.</param>
        /// <param name="lineNumber">Line number.</param>
        public CliTextSnapshotLine(string text, int lineNumber)
        {
            this.LineNumber = lineNumber;
            this.Length = text.Length;
        }

        /// <inheritdoc/>
        public int LineNumber { get; }

        /// <inheritdoc/>
        public int Length { get; }

        /// <inheritdoc/>
        public ITextSnapshot Snapshot => throw new NotImplementedException();
    }
}
