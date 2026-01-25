namespace LineWalker;

/// <summary>
/// Abstraction for console writing operations to enable testing.
/// </summary>
public interface IConsoleWriter {
    /// <summary>
    /// Gets or sets the foreground color of the console.
    /// </summary>
    ConsoleColor ForegroundColor { get; set; }
    
    /// <summary>
    /// Gets or sets the background color of the console.
    /// </summary>
    ConsoleColor BackgroundColor { get; set; }
    
    /// <summary>
    /// Gets the current cursor position.
    /// </summary>
    (int Left, int Top) GetCursorPosition();
    
    /// <summary>
    /// Sets the cursor position.
    /// </summary>
    void SetCursorPosition(int left, int top);
    
    /// <summary>
    /// Writes a line to the console.
    /// </summary>
    void WriteLine(string text);
    
    /// <summary>
    /// Flushes any buffered output to the console.
    /// </summary>
    void Flush() { }
}
