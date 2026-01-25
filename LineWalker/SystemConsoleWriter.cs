namespace LineWalker;

/// <summary>
/// Default console writer that wraps System.Console with a stream writer.
/// </summary>
public class SystemConsoleWriter : IConsoleWriter, IDisposable {
    private readonly StreamWriter _streamWriter;
    
    public SystemConsoleWriter() {
        // Open standard output and create a StreamWriter for better performance
        var stdout = Console.OpenStandardOutput();
        _streamWriter = new StreamWriter(stdout, Console.OutputEncoding, 1024, leaveOpen: true) {
            AutoFlush = false // We'll control flushing manually for batching
        };
    }
    
    public ConsoleColor ForegroundColor {
        get => Console.ForegroundColor;
        set => Console.ForegroundColor = value;
    }

    public ConsoleColor BackgroundColor {
        get => Console.BackgroundColor;
        set => Console.BackgroundColor = value;
    }

    public (int Left, int Top) GetCursorPosition() => Console.GetCursorPosition();

    public void SetCursorPosition(int left, int top) {
        // Flush before cursor manipulation to ensure all output is written
        _streamWriter.Flush();
        Console.SetCursorPosition(left, top);
    }

    public void WriteLine(string text) => _streamWriter.WriteLine(text);
    
    /// <summary>
    /// Flushes the stream writer to ensure all buffered output is written.
    /// </summary>
    public void Flush() => _streamWriter.Flush();
    
    public void Dispose() {
        _streamWriter.Flush();
        _streamWriter.Dispose();
        GC.SuppressFinalize(this);
    }
}
