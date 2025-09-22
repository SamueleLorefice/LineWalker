namespace LineWalker;

public struct Message(string text, bool updatePrevious = false, LogLevel level = LogLevel.Info) {
    public string Text { get; set; } = text;
    public bool UpdatePrevious { get; init; } = updatePrevious;
    public LogLevel Level { get; init; } = level;

    public void Deconstruct(out string Text, out bool UpdatePrevious, out LogLevel Level) {
        Text = this.Text;
        UpdatePrevious = this.UpdatePrevious;
        Level = this.Level;
    }
}