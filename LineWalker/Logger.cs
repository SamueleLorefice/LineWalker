using System.Collections.Concurrent;
using System.Net.Quic;

namespace LineWalker;

public enum LogLevel {
    Debug,
    Info,
    Warning,
    Error,
    Critical
}

public record Message(string Text, bool UpdatePrevious = false, LogLevel Level = LogLevel.Info) {
    public string Text { get; set; } = Text;
}

public class Logger {
    private static Logger? _logger;
    private Thread _loggerThread;
    private bool _running = true;
    private readonly ConcurrentQueue<Message> _messageQueue;
    private int lastMessageLength = 0;
    private int lastMessageLines = 0;

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
    public static Logger GetInstance() {
        return _logger ??= new Logger();
    }
    
    /// <summary>
    /// Shuts down the logger and waits for the logger thread to finish.
    /// </summary>
    public static void Shutdown() {
        if (_logger == null) return;
        _logger._running = false;
        _logger._loggerThread.Join();
        _logger = null;
    }
    
    private Logger() {
        // Initialize the queue
        _messageQueue = new();
        _loggerThread = new(() => { 
            while (_running) {
                if(!_messageQueue.IsEmpty) {
                    LogNext();
                } else {
                    // Small delay to prevent CPU spinning when queue is empty
                    Thread.Sleep(1);
                }
            }
        });
        _loggerThread.Start();
    }

    private void LogNext() {
        try {
            if (!_messageQueue.TryDequeue(out var message)) return;
            string[] lines = message.Text.Split(Environment.NewLine);
            int maxLen = lines.Max(x => x.Length);
            int maxLines = lines.Length;

            if (message.UpdatePrevious && lastMessageLines > 0) {
                // Use ANSI escape sequences to move cursor up and clear lines
                // This is more reliable than Console.GetCursorPosition() in many environments
                for (int i = 0; i < lastMessageLines; i++) {
                    Console.Write("\u001b[1A"); // Move cursor up one line
                    Console.Write("\u001b[2K"); // Clear the entire line
                }
                Console.Write("\r"); // Move to beginning of line
            }

            var col = Console.ForegroundColor;
            var bgcol = Console.BackgroundColor;

            Console.ForegroundColor = message.Level switch {
                LogLevel.Debug => ConsoleColor.DarkGray,
                LogLevel.Warning => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Critical => ConsoleColor.DarkRed,
                _ => Console.ForegroundColor
            };
            
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
        } catch (Exception ex) {
            // If there's an issue with console operations, just continue
            Console.WriteLine($"[Logger Error]: {ex.Message}");
        }
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

    ~Logger() {
        _running = false;
        _loggerThread.Join();
    }
}