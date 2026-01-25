using System.Collections.Concurrent;

namespace LineWalker;

/// <summary>
/// Logger class that handles logging messages in a separate thread.
/// It uses a concurrent queue to store messages and processes them on it's own thread.
/// </summary>
public class Logger : IDisposable {
    private static Logger? _logger;
    private static IConsoleWriter _consoleWriter = new SystemConsoleWriter();
    private readonly Thread _loggerThread;
    private readonly ConcurrentQueue<Message> _messageQueue;
    private int lastMessageLength;
    private int lastMessageLines;
    private readonly CancellationTokenSource _cts = new();

    /// <summary>
    /// How many messages are currently in the queue waiting to be output.
    /// </summary>
    public int QueueCount => _messageQueue.Count;

    /// <summary>
    /// Gets the singleton instance of the logger.
    /// </summary>
    /// <remarks>
    /// If an instance does not exist yet, it creates a new instance of the logger.
    /// </remarks>
    /// <returns>reference to the <see cref="Logger"/> instance.</returns>
    public static Logger GetInstance() => _logger ??= new();

    /// <summary>
    /// Configures the console writer for testing purposes.
    /// </summary>
    /// <param name="consoleWriter">The console writer implementation to use.</param>
    public static void Configure(IConsoleWriter consoleWriter) {
        _consoleWriter = consoleWriter ?? throw new ArgumentNullException(nameof(consoleWriter));
    }

    /// <summary>
    /// Shuts down the logger and waits for the logger thread to finish.
    /// </summary>
    public static void Shutdown() {
        if (_logger == null) return;
        _logger.ReleaseThread();
        _logger = null;
    }
    
    public LogLevel MinLogLevel { get; set; } = LogLevel.Trace;
    
    internal Logger() {
        var token = _cts.Token;
        
        // Initialize the queue
        _messageQueue = new();
        _loggerThread = new(() => { 
            while (!token.IsCancellationRequested)
                if(!_messageQueue.IsEmpty) LogNext();
        }) {
            IsBackground = true
        };
        
        _loggerThread.Start();
    }
    
    private void LogNext() {
        if (!_messageQueue.TryDequeue(out var message)) return;
        if (message.Level < MinLogLevel) return; //skip messages below the minimum log level
        
        // Guard against null or empty messages
        if (string.IsNullOrEmpty(message.Text)) return;
        
        // Check if we can batch messages
        // We can only batch if the current message doesn't update the previous line
        if (!message.UpdatePrevious) {
            var messagesToLog = new List<Message> { message };
            
            // Peek ahead and collect messages that can be batched
            while (_messageQueue.TryPeek(out var nextMessage)) {
                // Stop batching if next message needs to update previous line
                if (nextMessage.UpdatePrevious) break;
                
                // Stop if we've collected enough messages (limit to prevent unbounded growth)
                if (messagesToLog.Count >= 100) break;
                
                // Dequeue and add to batch
                if (_messageQueue.TryDequeue(out nextMessage)) {
                    if (nextMessage.Level >= MinLogLevel && !string.IsNullOrEmpty(nextMessage.Text)) {
                        messagesToLog.Add(nextMessage);
                    }
                } else {
                    break;
                }
            }
            
            // Write all batched messages
            foreach (var msg in messagesToLog) {
                WriteMessage(msg, false);
            }
            
            // Flush the stream writer after writing the batch
            _consoleWriter.Flush();
        } else {
            // Message needs to update previous, flush any pending output first
            _consoleWriter.Flush();
            
            // Write the message with cursor manipulation
            WriteMessage(message, true);
            
            // Flush immediately since we manipulated the cursor
            _consoleWriter.Flush();
        }
    }
    
    private void WriteMessage(Message message, bool handleUpdatePrevious) {
        string[] lines = message.Text.Split(Environment.NewLine);
        int maxLen = lines.Max(x => x.Length);
        int maxLines = lines.Length;

        if (handleUpdatePrevious && message.UpdatePrevious) {
            //push all lines to be as long as the longest line on the previous message
            for (var index = 0; index < lines.Length; index++) {
                ref var line = ref lines[index]; //grab the reference to the line
                //add enough whitespace to the line to make it as long as the longest line on previous message
                if (line.Length < lastMessageLength) line += new string(' ', lastMessageLength - line.Length);
            }
            //merge the array of lines into a single string
            message.Text = string.Join(Environment.NewLine, lines);
            
            //Get current cursor position and move cursor to the last message initial position
            try {
                var pos = _consoleWriter.GetCursorPosition();
                _consoleWriter.SetCursorPosition(0, pos.Top - lastMessageLines);
            }
            catch (ArgumentOutOfRangeException) { 
                /*ignored, happens when the console is resized smaller than the current cursor position*/ 
            }
            catch (IOException) { 
                /*ignored, happens when console output is redirected or unavailable*/ 
            }
            
            //check if the message is shorter than the last message in terms of lines
            if (lastMessageLines > maxLines) {
                //pad the message with new lines to make it as long as the last message
                string pad = Environment.NewLine + new string(' ', lastMessageLength);
                for (int i = 0; i <= maxLines - lastMessageLines; i++) message.Text += pad;
            }
        }

        try {
            var col = _consoleWriter.ForegroundColor;
            var bgcol = _consoleWriter.BackgroundColor;

            _consoleWriter.ForegroundColor = GetFgColor(message.Level);
            _consoleWriter.BackgroundColor = GetBgColor(message.Level);
            
            //write the message
            _consoleWriter.WriteLine(message.Text);
            
            //reset the colors
            if(message.Level != LogLevel.Info) {
                _consoleWriter.ForegroundColor = col;
                _consoleWriter.BackgroundColor = bgcol;
            }
        }
        catch (IOException) {
            /*ignored, happens when console output is redirected or unavailable*/
        }
        
        //update the last message length and lines
        lastMessageLength = maxLen;
        lastMessageLines = maxLines;
    }

    private static ConsoleColor GetFgColor(LogLevel level) {
        return level switch {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.DarkGray,
            LogLevel.Info => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.DarkRed,
            _ => _consoleWriter.ForegroundColor
        };
    }

    private static ConsoleColor GetBgColor(LogLevel level) {
        return level switch {
            LogLevel.Trace => ConsoleColor.Black,
            LogLevel.Debug => ConsoleColor.Black,
            LogLevel.Info => ConsoleColor.Black,
            LogLevel.Warning => ConsoleColor.Black,
            LogLevel.Error => ConsoleColor.Black,
            LogLevel.Critical => ConsoleColor.Black,
            _ => _consoleWriter.BackgroundColor
        };
    }

    /// <summary>
    /// Enqueues a new message to be logged.
    /// </summary>
    /// <param name="message"><see cref="string"/> containing the message to be logged.</param>
    /// <param name="updatePrevious">Whether the previous logged line should be replaced by this message.</param>
    public void Log(string message, bool updatePrevious = false) => _messageQueue.Enqueue(new (message, updatePrevious));
    
    
    /// <summary>
    /// Enqueues a new message to be logged.
    /// </summary>
    /// <param name="message"><see cref="T:string[]"/> containing the message to be logged.</param>
    /// <param name="updatePrevious">Whether the previous logged line should be replaced by this message.</param>
    public void Log(string[] message, bool updatePrevious = false) => _messageQueue.Enqueue(new (string.Join(Environment.NewLine, message), updatePrevious));
    
    /// <summary>
    /// Enqueues a new message to be logged.
    /// </summary>
    /// <param name="message"><see cref="string"/> containing the message to be logged.</param>
    /// <param name="Level"><see cref="LogLevel"/> for this message. Sets colored console output.</param>
    /// <param name="updatePrevious">Whether the previous logged line should be replaced by this message.</param>
    public void Log(string message, LogLevel Level, bool updatePrevious = false) => _messageQueue.Enqueue(new (message, updatePrevious, Level));

    /// <summary>
    /// Enqueues a new message to be logged.
    /// </summary>
    /// <param name="message"><see cref="T:string[]"/> containing the message to be logged.</param>
    /// <param name="Level"><see cref="LogLevel"/> for this message. Sets colored console output.</param>
    /// <param name="updatePrevious">Whether the previous logged line should be replaced by this message.</param>
    public void Log(string[] message, LogLevel Level, bool updatePrevious = false) => _messageQueue.Enqueue(new (string.Join(Environment.NewLine, message), updatePrevious, Level));
    
    /// <summary>
    /// Enqueues a new message to be logged.
    /// </summary>
    /// <param name="message"><see cref="object"/> whose strign representation is going to be logged.</param>
    /// <param name="Level"><see cref="LogLevel"/> for this message. Sets colored console output.</param>
    /// <param name="updatePrevious">Whether the previous logged line should be replaced by this message.</param>
    public void Log(object? message, LogLevel Level = LogLevel.Info, bool updatePrevious = false) => _messageQueue.Enqueue(new (message?.ToString() ?? "NULL", updatePrevious, Level));
    
    private void ReleaseThread() {
        // Check if already cancelled
        if (_cts.IsCancellationRequested) return;
        
        // Signal cancellation
        _cts.Cancel();
        
        // Wait for the queue to drain completely
        while (!_messageQueue.IsEmpty) {
            Thread.Sleep(10); // Small delay to allow processing
        }
        
        // Wait for thread to finish
        if (_loggerThread.IsAlive) {
            _loggerThread.Join();
        }
        
        // Flush any remaining output
        _consoleWriter.Flush();
    }

    /// <inheritdoc />
    public void Dispose() {
        ReleaseThread();
        GC.SuppressFinalize(this);
    }

    ~Logger() => ReleaseThread();
}