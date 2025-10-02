using System.Collections.Concurrent;

namespace LineWalker;

/// <summary>
/// Logger class that handles logging messages in a separate thread.
/// It uses a concurrent queue to store messages and processes them on it's own thread.
/// </summary>
public class Logger : IDisposable {
    private static Logger? _logger;
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
    /// Shuts down the logger and waits for the logger thread to finish.
    /// </summary>
    public static void Shutdown() {
        if (_logger == null) return;
        _logger._loggerThread.Join();
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
        });
        
        _loggerThread.Start();
    }
    
    private void LogNext() {
        if (!_messageQueue.TryDequeue(out var message)) return;
        if (message.Level < MinLogLevel) return; //skip messages below the minimum log level
        string[] lines = message.Text.Split(Environment.NewLine);
        int maxLen = lines.Max(x => x.Length);
        int maxLines = lines.Length;

        if (message.UpdatePrevious) {
            //push all lines to be as long as the longest line on the previous message
            for (var index = 0; index < lines.Length; index++) {
                ref var line = ref lines[index]; //grab the reference to the line
                //add enough whitespace to the line to make it as long as the longest line on previous message
                if (line.Length < lastMessageLength) line += new string(' ', lastMessageLength - line.Length);
            }
            //merge the array of lines into a single string
            message.Text = string.Join(Environment.NewLine, lines);
            //Get current cursor position
            var pos = Console.GetCursorPosition();
            //move cursor to the last message initial position
            Console.SetCursorPosition(0, pos.Top - lastMessageLines);
            //check if the message is shorter than the last message in terms of lines
            if (lastMessageLines > maxLines) {
                //pad the message with new lines to make it as long as the last message
                string pad = Environment.NewLine + new string(' ', lastMessageLength);
                for (int i = 0; i <= maxLines - lastMessageLines; i++) message.Text += pad;
            }
        }

        var col = Console.ForegroundColor;
        var bgcol = Console.BackgroundColor;

        Console.ForegroundColor = GetFgColor(message.Level);
        Console.BackgroundColor = GetBgColor(message.Level);
        
        //write the message
        Console.WriteLine(message.Text);
        
        //reset the colors
        if(message.Level != LogLevel.Info) {
            Console.ForegroundColor = col;
            Console.BackgroundColor = bgcol;
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
            _ => Console.ForegroundColor
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
            _ => Console.BackgroundColor
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
    public void Log(object message, LogLevel Level = LogLevel.Info, bool updatePrevious = false) => _messageQueue.Enqueue(new (message.ToString() ?? "NULL", updatePrevious, Level));
    
    private void ReleaseThread() {
        _cts.Cancel();
        _loggerThread.Join();
    }

    /// <inheritdoc />
    public void Dispose() {
        ReleaseThread();
        GC.SuppressFinalize(this);
    }

    ~Logger() => ReleaseThread();
}