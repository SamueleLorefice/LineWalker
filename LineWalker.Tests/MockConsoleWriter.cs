using LineWalker;

namespace LineWalker.Tests;

/// <summary>
/// Mock console writer for testing that captures all console operations.
/// </summary>
internal class MockConsoleWriter : IConsoleWriter {
    private readonly List<string> _writtenLines = new();
    private readonly List<(ConsoleColor Fg, ConsoleColor Bg)> _colorChanges = new();
    private readonly List<(int Left, int Top)> _cursorPositions = new();
    private (int Left, int Top) _currentPosition = (0, 0);

    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.White;
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;

    public List<string> WrittenLines => _writtenLines;
    public List<(ConsoleColor Fg, ConsoleColor Bg)> ColorChanges => _colorChanges;
    public List<(int Left, int Top)> CursorPositions => _cursorPositions;

    public (int Left, int Top) GetCursorPosition() => _currentPosition;

    public void SetCursorPosition(int left, int top) {
        _currentPosition = (left, top);
        _cursorPositions.Add(_currentPosition);
    }

    public void WriteLine(string text) {
        _writtenLines.Add(text);
        _colorChanges.Add((ForegroundColor, BackgroundColor));
        // Simulate advancing cursor
        var lines = text.Split(Environment.NewLine).Length;
        _currentPosition = (_currentPosition.Left, _currentPosition.Top + lines);
    }
    
    public void Flush() {
        // No-op for mock
    }

    public void Clear() {
        _writtenLines.Clear();
        _colorChanges.Clear();
        _cursorPositions.Clear();
    }
}
