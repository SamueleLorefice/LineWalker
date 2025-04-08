using System.Collections.Concurrent;

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
    private readonly Thread _loggerThread;
    private bool _running = true;
    private readonly ConcurrentQueue<Message> _messageQueue;
    private int lastMessageLength = 0;
    private int lastMessageLines = 0;

    public static Logger GetInstance() {
        return _logger ??= new Logger();
    }

    private Logger() {
        // Initialize the queue
        _messageQueue = new();
        _loggerThread = new(() => { while (_running || !_messageQueue.IsEmpty) LogNext(); });
    }

    private void LogNext() {
        if (!_messageQueue.TryDequeue(out var message)) return;
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
            message.Text = lines.Aggregate("", (_, x) => x + Environment.NewLine);
            //Get current cursor position
            var pos = Console.GetCursorPosition();
            //move cursor to the last message initial position
            Console.SetCursorPosition(0, pos.Top - lastMessageLines);
            //check if the message is shorter than the last message in terms of lines
            if (lastMessageLines >= maxLines) {
                //pad the message with new lines to make it as long as the last message
                string pad = Environment.NewLine + new string(' ', lastMessageLength);
                for (int i = 0; i <= maxLines - lastMessageLines; i++) message.Text += pad;
            }
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
    }

    public void Log(string message, LogLevel Level = LogLevel.Info, bool updatePrevious = false) {
        //add the message to the queue
        _messageQueue.Enqueue(new (message, updatePrevious));
    }
    
    ~Logger() {
        _running = false;
        _loggerThread.Join();
    }
}