namespace LineWalker;

/// <summary>
/// Default console writer that wraps System.Console.Out.
/// </summary>
public class SystemConsoleWriter : IConsoleWriter {
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
        Console.Out.Flush();
        Console.SetCursorPosition(left, top);
    }

    public void WriteLine(string text) => Console.Out.WriteLine(text);
    
    /// <summary>
    /// Flushes the console output to ensure all buffered output is written.
    /// </summary>
    public void Flush() => Console.Out.Flush();
}
