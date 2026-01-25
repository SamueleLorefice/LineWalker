namespace LineWalker;

/// <summary>
/// Default console writer that wraps System.Console.
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

    public void SetCursorPosition(int left, int top) => Console.SetCursorPosition(left, top);

    public void WriteLine(string text) => Console.WriteLine(text);
}
