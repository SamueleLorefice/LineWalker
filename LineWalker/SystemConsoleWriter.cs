namespace LineWalker;

/// <summary>
/// Default console writer that wraps the Console's standard output StreamWriter.
/// </summary>
public class SystemConsoleWriter : IConsoleWriter {
    private readonly TextWriter _stdout;
    
    public SystemConsoleWriter() {
        // Get a reference to Console's StreamWriter for stdout
        _stdout = Console.Out;
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
        _stdout.Flush();
        Console.SetCursorPosition(left, top);
    }

    public void WriteLine(string text) => _stdout.WriteLine(text);
    
    /// <summary>
    /// Flushes the console output to ensure all buffered output is written.
    /// </summary>
    public void Flush() => _stdout.Flush();
}
