using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace UCode.Extensions.ConsoleApp
{
    /// <summary>
    /// A class that extends <see cref="TextWriter"/> to enable dual-output functionality.
    /// This class writes output to both the console and a specified file or stream,
    /// optionally doing so in parallel. Implements asynchronous and synchronous write
    /// operations and provides buffering for performance optimization.
    ///
    /// Usage example:
    /// <code>
    /// // Example usage in an async Main method or similar context:
    /// string filePath = "output.txt";
    /// await using (var dualOutput = new DualOutput(filePath, overwrite: true))
    /// {
    ///     // Redirect Console output to the DualOutput instance
    ///     Console.SetOut(dualOutput);
    ///
    ///     // Write to both console and file
    ///     Console.WriteLine("This message goes to console and the file.");
    ///
    ///     // Asynchronously write another line
    ///     await Console.Out.WriteLineAsync("Another asynchronous line.");
    ///
    ///     // Optionally flush with a timeout
    ///     dualOutput.Flush(2000);
    ///
    ///     // Print statistics by calling ToString()
    ///     Console.WriteLine(dualOutput);
    ///
    ///     // Restore original Console output before disposing DualOutput
    ///     Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
    /// }
    /// Console.WriteLine("Output was written to 'output.txt' and displayed on the console.");
    /// </code>
    /// </summary>
    public class DualOutput : TextWriter, IAsyncDisposable, IDisposable
    {
        private readonly TextWriter _consoleOutput;           // Stores the reference to the original console output writer.
        private readonly StreamWriter _fileOutput;            // Handles the output writing to a file or stream.

        private readonly Channel<string> _channel;            // Provides a bounded channel for buffering messages.
        private readonly CancellationTokenSource _cancellationTokenSource = new(); // Handles cancellation tokens for tasks.
        private readonly Task _processingTask;                // Background task for processing messages from the channel.

        private bool _disposed;                               // Indicates whether the object has been disposed.
        private volatile int _pendingMessages;                // Tracks the number of pending messages in the queue.



        // Accumulates time in ticks (for console and file) instead of using Stopwatch.
        private long _consoleWriteTicks;                      // Accumulates total ticks spent writing to the console.
        private long _fileWriteTicks;                         // Accumulates total ticks spent writing to the file.

        private int _totalWriteCalls;                         // Counts total calls to any write method.

        // Tracks total ticks spent in ProcessQueueAsync
        private long _processQueueTotalTicks;

        /// <summary>
        /// Initializes a new instance of the <see cref="DualOutput"/> class with a specified file path.
        /// </summary>
        /// <param name="consoleTextWriter">The original console write stream.</param>
        /// <param name="filePath">The path of the file to write to.</param>
        /// <param name="overwrite">If true, overwrites the file; if false, appends to the file. Default is true.</param>
        /// <exception cref="IOException">Thrown if the file cannot be created or accessed.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if the user does not have permission to access the file.</exception>
        public DualOutput(TextWriter consoleTextWriter, string filePath, bool overwrite = true)
            : this(consoleTextWriter, new FileStream(filePath,
                                  overwrite ? FileMode.Create : FileMode.Append,
                                  FileAccess.Write,
                                  FileShare.Read))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DualOutput"/> class with a specified output stream.
        /// </summary>
        /// <param name="consoleTextWriter">The original console write stream.</param>
        /// <param name="outputStream">The stream where data will be written.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="outputStream"/> is null.</exception>
        public DualOutput(TextWriter consoleTextWriter, Stream outputStream)
        {
            ArgumentNullException.ThrowIfNull(outputStream);

            this._consoleOutput = consoleTextWriter; // Save the original console output.
            this._fileOutput = new StreamWriter(outputStream) { AutoFlush = this.AutoFlush }; // Initialize file output writer.

            // Configure a bounded channel with a specified capacity for message buffering.
            this._channel = Channel.CreateBounded<string>(new BoundedChannelOptions(1000)
            {
                SingleReader = true,   // Only one reader will process messages.
                SingleWriter = false,  // Multiple writers can write messages.
                FullMode = BoundedChannelFullMode.Wait // Writers wait if the channel is full.
            });

            // Start the background processing task to handle messages.
            this._processingTask = Task.Run(this.ProcessQueueAsync, this._cancellationTokenSource.Token);
        }

        /// <summary>
        /// Auto flush stream writer
        /// </summary>
        public bool AutoFlush
        {
            get; init;
        } = true;

        /// <summary>
        /// Gets the encoding used by this writer.
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;

        /// <summary>
        /// Gets the total bytes written to the file.
        /// </summary>
        public long TotalFileBytesWritten
        {
            get; private set;
        }

        /// <summary>
        /// Gets the total bytes written to the console.
        /// </summary>
        public long TotalConsoleBytesWritten
        {
            get; private set;
        }

        /// <summary>
        /// Gets the time of the first write performed.
        /// </summary>
        public DateTime? FirstWriteTime
        {
            get; private set;
        }

        /// <summary>
        /// Gets the time of the last write performed.
        /// </summary>
        public DateTime? LastWriteTime
        {
            get; private set;
        }

        /// <summary>
        /// Gets the total time spent writing to the console (converted from ticks).
        /// </summary>
        public TimeSpan ConsoleWriteTime => TimeSpan.FromTicks(this._consoleWriteTicks);

        /// <summary>
        /// Gets the total time spent writing to the file (converted from ticks).
        /// </summary>
        public TimeSpan FileWriteTime => TimeSpan.FromTicks(this._fileWriteTicks);

        /// <summary>
        /// Gets the total number of write calls made to this <see cref="DualOutput"/> instance.
        /// </summary>
        public int TotalWriteCalls => this._totalWriteCalls;

        /// <summary>
        /// Gets the total time consumed by the <see cref="ProcessQueueAsync"/> method (converted from ticks).
        /// </summary>
        public TimeSpan ProcessQueueTotalTime => TimeSpan.FromTicks(this._processQueueTotalTicks);

        /// <summary>
        /// Continuously processes messages from the channel, batching them for performance,
        /// and writes to both console and file. Writes can be performed simultaneously
        /// to avoid waiting for one or the other.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled.</exception>
        private async Task ProcessQueueAsync()
        {
            // Track the start time in ticks for the entire queue processing.
            var processQueueStartTicks = DateTime.UtcNow.Ticks;

            try
            {
                while (await this._channel.Reader.WaitToReadAsync(this._cancellationTokenSource.Token).ConfigureAwait(false))
                {
                    var stringBuilder = new StringBuilder();
                    while (this._channel.Reader.TryRead(out var message))
                    {
                        stringBuilder.Append(message);
                    }

                    if (stringBuilder.Length > 0)
                    {
                        var combinedMessage = stringBuilder.ToString();

                        // Record the first and last write times
                        this.FirstWriteTime ??= DateTime.UtcNow;
                        this.LastWriteTime = DateTime.UtcNow;

                        // Perform console and file writes in parallel to achieve simultaneity
                        var consoleTask = Task.Run(() =>
                        {
                            var consoleStartTicks = DateTime.UtcNow.Ticks;
                            this._consoleOutput.Write(combinedMessage);
                            var consoleEndTicks = DateTime.UtcNow.Ticks;

                            this._consoleWriteTicks += consoleEndTicks - consoleStartTicks;
                            this.TotalConsoleBytesWritten += Encoding.UTF8.GetByteCount(combinedMessage);
                        });

                        var fileTask = Task.Run(() =>
                        {
                            var fileStartTicks = DateTime.UtcNow.Ticks;
                            this._fileOutput.Write(combinedMessage);
                            var fileEndTicks = DateTime.UtcNow.Ticks;

                            this._fileWriteTicks += fileEndTicks - fileStartTicks;
                            this.TotalFileBytesWritten += Encoding.UTF8.GetByteCount(combinedMessage);
                        });

                        await Task.WhenAll(consoleTask, fileTask);

                        // Update message count
                        Interlocked.Add(ref this._pendingMessages, -1 * CountOccurrences(stringBuilder));
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected exception when cancellation is requested.
            }
            finally
            {
                // Calculate the total ticks in the queue processing.
                var processQueueEndTicks = DateTime.UtcNow.Ticks;
                this._processQueueTotalTicks = processQueueEndTicks - processQueueStartTicks;

                this._fileOutput.Flush(); // Ensure all data is flushed to the file.
            }
        }

        /// <summary>
        /// Counts the number of messages in a batch based on newline characters.
        /// </summary>
        /// <param name="stringBuilder">The <see cref="StringBuilder"/> containing the batched messages.</param>
        /// <returns>The number of messages in the batch.</returns>
        private static int CountOccurrences(StringBuilder stringBuilder) => stringBuilder
                .ToString()
                .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
                .Length;

        /// <summary>
        /// Writes a string value to the output channel.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed.</exception>
        public override void Write([AllowNull] string? value)
        {
            if (this._disposed || value == null)
            {
                return;
            }

            Interlocked.Increment(ref this._pendingMessages);
            Interlocked.Increment(ref this._totalWriteCalls);
            this._channel.Writer.TryWrite(value);
        }

        /// <summary>
        /// Writes a string value followed by a newline character to the output channel.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed.</exception>
        public override void WriteLine([AllowNull] string? value)
        {
            if (this._disposed)
            {
                return;
            }

            var text = (value ?? string.Empty) + Environment.NewLine;
            Interlocked.Increment(ref this._pendingMessages);
            Interlocked.Increment(ref this._totalWriteCalls);
            this._channel.Writer.TryWrite(text);
        }

        /// <summary>
        /// Asynchronously writes a string value to the output channel.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed.</exception>
        public override async Task WriteAsync([AllowNull] string? value)
        {
            if (this._disposed || value == null)
            {
                return;
            }

            Interlocked.Increment(ref this._pendingMessages);
            Interlocked.Increment(ref this._totalWriteCalls);
            await this._channel.Writer.WriteAsync(value, this._cancellationTokenSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously writes a string value followed by a newline character to the output channel.
        /// </summary>
        /// <param name="value">The string value to write.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="ObjectDisposedException">Thrown if the object has been disposed.</exception>
        public override async Task WriteLineAsync([AllowNull] string? value)
        {
            if (this._disposed)
            {
                return;
            }

            var text = (value ?? string.Empty) + Environment.NewLine;
            Interlocked.Increment(ref this._pendingMessages);
            Interlocked.Increment(ref this._totalWriteCalls);
            await this._channel
                .Writer
                .WriteAsync(text, this._cancellationTokenSource.Token)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Flushes the file output and waits for the queue to drain within the specified timeout.
        /// </summary>
        /// <param name="timeoutMilliseconds">The timeout in milliseconds. Default is infinite.</param>
        /// <exception cref="TimeoutException">Thrown if the operation exceeds the specified timeout.</exception>
        public void Flush(int timeoutMilliseconds = Timeout.Infinite)
        {
            var startTicks = DateTime.UtcNow.Ticks;
            while (Interlocked.CompareExchange(ref this._pendingMessages, 0, 0) != 0)
            {
                // If there is a timeout, check the current elapsed time in ticks.
                if (timeoutMilliseconds != Timeout.Infinite)
                {
                    var nowTicks = DateTime.UtcNow.Ticks;
                    // Convert the elapsed ticks to milliseconds:
                    var elapsedMilliseconds = (nowTicks - startTicks) / TimeSpan.TicksPerMillisecond;
                    if (elapsedMilliseconds > timeoutMilliseconds)
                    {
                        throw new TimeoutException("Flush timed out waiting for the queue to be processed.");
                    }
                }
                Thread.Sleep(10);
            }
            this._fileOutput.Flush();
            this._consoleOutput.Flush();
        }

        /// <summary>
        /// Asynchronously flushes the file output and waits for the queue to drain.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled.</exception>
        public override async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            while (Interlocked.CompareExchange(ref this._pendingMessages, 0, 0) != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(10, cancellationToken).ConfigureAwait(false);
            }
            await this._fileOutput.FlushAsync(cancellationToken).ConfigureAwait(false);
            await this._consoleOutput.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Disposes the <see cref="DualOutput"/> object, ensuring that all queued messages are written.
        /// </summary>
        /// <param name="disposing">A value indicating whether the method was called from <see cref="Dispose()"/>.</param>
        protected new virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }

            if (disposing)
            {
                this._channel.Writer.TryComplete();
                this._cancellationTokenSource.Cancel();

                try
                {
                    this._processingTask.Wait();
                }
                catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
                {
                    // Expected when cancellation is requested.
                }

                this._fileOutput.Flush();
                this._fileOutput.Dispose();
                this._cancellationTokenSource.Dispose();
            }

            this._disposed = true;
        }

        /// <summary>
        /// Disposes the <see cref="DualOutput"/> object.
        /// </summary>
        public new void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Asynchronously disposes the <see cref="DualOutput"/> object.
        /// </summary>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public new ValueTask DisposeAsync()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
            return ValueTask.CompletedTask;
        }

        /// <summary>
        /// Returns statistics about this <see cref="DualOutput"/> instance as a string.
        /// </summary>
        /// <returns>A string containing statistics about the class usage.</returns>
        public override string ToString() =>
$@"First Write Time: {this.FirstWriteTime}
Last Write Time: {this.LastWriteTime}
Total Console Bytes Written: {this.TotalConsoleBytesWritten}
Total File Bytes Written: {this.TotalFileBytesWritten}
Total Console Write Time: {this.ConsoleWriteTime.TotalMilliseconds} ms
Total File Write Time: {this.FileWriteTime.TotalMilliseconds} ms
Total Write Calls: {this._totalWriteCalls}
Total ProcessQueueAsync Time: {this.ProcessQueueTotalTime.TotalMilliseconds} ms";

    }
}
