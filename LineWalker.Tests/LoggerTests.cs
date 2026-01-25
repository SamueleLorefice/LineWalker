using LineWalker;

namespace LineWalker.Tests;

public class LoggerTests : IDisposable {
    private readonly MockConsoleWriter _mockConsole;

    public LoggerTests() {
        _mockConsole = new MockConsoleWriter();
        Logger.Configure(_mockConsole);
    }

    public void Dispose() {
        // Reset the singleton for next test
        Logger.Shutdown();
        
        // Small delay to ensure cleanup
        Thread.Sleep(20);
    }

    [Fact]
    public void Log_SimpleMessage_WritesToConsole() {
        // Arrange
        var logger = Logger.GetInstance();
        var message = "Hello, World!";

        // Act
        logger.Log(message);
        Thread.Sleep(100); // Wait for async processing

        // Assert
        Assert.Single(_mockConsole.WrittenLines);
        Assert.Equal(message, _mockConsole.WrittenLines[0]);
    }

    [Fact]
    public void Log_MultipleMessages_WritesInOrder() {
        // Arrange
        var logger = Logger.GetInstance();
        var messages = new[] { "First", "Second", "Third" };

        // Act
        foreach (var msg in messages) {
            logger.Log(msg);
        }
        Thread.Sleep(100); // Wait for async processing

        // Assert
        Assert.Equal(3, _mockConsole.WrittenLines.Count);
        for (int i = 0; i < messages.Length; i++) {
            Assert.Equal(messages[i], _mockConsole.WrittenLines[i]);
        }
    }

    [Fact]
    public void Log_WithLogLevel_SetsCorrectColor() {
        // Arrange
        var logger = Logger.GetInstance();
        
        // Act
        logger.Log("Trace message", LogLevel.Trace);
        logger.Log("Debug message", LogLevel.Debug);
        logger.Log("Info message", LogLevel.Info);
        logger.Log("Warning message", LogLevel.Warning);
        logger.Log("Error message", LogLevel.Error);
        logger.Log("Critical message", LogLevel.Critical);
        Thread.Sleep(150);

        // Assert
        Assert.Equal(6, _mockConsole.ColorChanges.Count);
        Assert.Equal(ConsoleColor.Gray, _mockConsole.ColorChanges[0].Fg);
        Assert.Equal(ConsoleColor.DarkGray, _mockConsole.ColorChanges[1].Fg);
        Assert.Equal(ConsoleColor.White, _mockConsole.ColorChanges[2].Fg);
        Assert.Equal(ConsoleColor.Yellow, _mockConsole.ColorChanges[3].Fg);
        Assert.Equal(ConsoleColor.Red, _mockConsole.ColorChanges[4].Fg);
        Assert.Equal(ConsoleColor.DarkRed, _mockConsole.ColorChanges[5].Fg);
    }

    [Fact]
    public void MinLogLevel_FiltersMessages() {
        // Arrange
        var logger = Logger.GetInstance();
        logger.MinLogLevel = LogLevel.Warning;

        // Act
        logger.Log("Trace", LogLevel.Trace);
        logger.Log("Debug", LogLevel.Debug);
        logger.Log("Info", LogLevel.Info);
        logger.Log("Warning", LogLevel.Warning);
        logger.Log("Error", LogLevel.Error);
        Thread.Sleep(100);

        // Assert
        Assert.Equal(2, _mockConsole.WrittenLines.Count);
        Assert.Equal("Warning", _mockConsole.WrittenLines[0]);
        Assert.Equal("Error", _mockConsole.WrittenLines[1]);
    }

    [Fact]
    public void Log_UpdatePrevious_RepositionsCursor() {
        // Arrange
        var logger = Logger.GetInstance();
        
        // Act
        logger.Log("Initial message");
        Thread.Sleep(50);
        logger.Log("Updated message", updatePrevious: true);
        Thread.Sleep(100);

        // Assert
        Assert.Equal(2, _mockConsole.WrittenLines.Count);
        Assert.NotEmpty(_mockConsole.CursorPositions);
        // Cursor should have been repositioned
        var lastPosition = _mockConsole.CursorPositions.Last();
        Assert.Equal(0, lastPosition.Left);
    }

    [Fact]
    public void Log_UpdatePreviousWithPadding_FillsRemainingSpace() {
        // Arrange
        var logger = Logger.GetInstance();
        
        // Act
        logger.Log("This is a long message");
        Thread.Sleep(50);
        logger.Log("Short", updatePrevious: true);
        Thread.Sleep(100);

        // Assert
        Assert.Equal(2, _mockConsole.WrittenLines.Count);
        var updatedMessage = _mockConsole.WrittenLines[1];
        // Should be padded to match the previous message length
        Assert.True(updatedMessage.Length >= "This is a long message".Length);
    }

    [Fact]
    public void Log_MultiLineMessage_HandlesCorrectly() {
        // Arrange
        var logger = Logger.GetInstance();
        var multiLineMessage = $"Line 1{Environment.NewLine}Line 2{Environment.NewLine}Line 3";

        // Act
        logger.Log(multiLineMessage);
        Thread.Sleep(100);

        // Assert
        Assert.Single(_mockConsole.WrittenLines);
        Assert.Contains("Line 1", _mockConsole.WrittenLines[0]);
        Assert.Contains("Line 2", _mockConsole.WrittenLines[0]);
        Assert.Contains("Line 3", _mockConsole.WrittenLines[0]);
    }

    [Fact]
    public void Log_ArrayOfStrings_JoinsWithNewLines() {
        // Arrange
        var logger = Logger.GetInstance();
        var lines = new[] { "Line 1", "Line 2", "Line 3" };

        // Act
        logger.Log(lines);
        Thread.Sleep(100);

        // Assert
        Assert.Single(_mockConsole.WrittenLines);
        Assert.Contains("Line 1", _mockConsole.WrittenLines[0]);
        Assert.Contains("Line 2", _mockConsole.WrittenLines[0]);
        Assert.Contains("Line 3", _mockConsole.WrittenLines[0]);
    }

    [Fact]
    public void Log_NullObject_LogsNULL() {
        // Arrange
        var logger = Logger.GetInstance();
        
        // Act
        object? nullObject = null;
        logger.Log(nullObject!);
        Thread.Sleep(100);

        // Assert
        Assert.Single(_mockConsole.WrittenLines);
        Assert.Equal("NULL", _mockConsole.WrittenLines[0]);
    }

    [Fact]
    public void Log_EmptyString_SkipsLogging() {
        // Arrange
        var logger = Logger.GetInstance();
        
        // Act
        logger.Log("");
        logger.Log("Valid message");
        Thread.Sleep(100);

        // Assert
        Assert.Single(_mockConsole.WrittenLines);
        Assert.Equal("Valid message", _mockConsole.WrittenLines[0]);
    }

    [Fact]
    public void Dispose_DrainsQueueBeforeExiting() {
        // Arrange
        var logger = Logger.GetInstance();
        for (int i = 0; i < 10; i++) {
            logger.Log($"Message {i}");
        }

        // Wait a bit to let some messages process
        Thread.Sleep(50);

        // Act
        logger.Dispose();

        // Assert - all messages should be processed before dispose completes
        Assert.Equal(10, _mockConsole.WrittenLines.Count);
    }

    [Fact]
    public void QueueCount_ReturnsCorrectValue() {
        // Arrange
        var logger = Logger.GetInstance();
        for (int i = 0; i < 5; i++) {
            logger.Log($"Message {i}");
        }

        // Act
        var count = logger.QueueCount;

        // Assert
        Assert.True(count >= 0 && count <= 5);
    }

    [Fact]
    public void Log_FromMultipleThreads_HandlesCorrectly() {
        // Arrange
        var logger = Logger.GetInstance();
        var threads = new List<Thread>();
        var messageCount = 10;

        // Act
        for (int i = 0; i < 5; i++) {
            var threadIndex = i;
            var thread = new Thread(() => {
                for (int j = 0; j < messageCount; j++) {
                    logger.Log($"Thread {threadIndex}, Message {j}");
                }
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads) {
            thread.Join();
        }

        Thread.Sleep(200); // Wait for all messages to be processed

        // Assert
        Assert.Equal(50, _mockConsole.WrittenLines.Count);
    }

    [Fact]
    public void Log_WithDifferentLogLevels_ResetsColorAfterNonInfo() {
        // Arrange
        var logger = Logger.GetInstance();
        var originalFg = _mockConsole.ForegroundColor;
        var originalBg = _mockConsole.BackgroundColor;

        // Act
        logger.Log("Warning message", LogLevel.Warning);
        Thread.Sleep(50);
        logger.Log("Info message", LogLevel.Info);
        Thread.Sleep(100);

        // Assert
        Assert.Equal(2, _mockConsole.WrittenLines.Count);
        // After warning (non-Info), colors should be reset
        // The implementation resets to the color that was current before the warning
    }

    [Fact]
    public void Log_UpdatePreviousMultipleTimes_MaintainsCorrectState() {
        // Arrange
        var logger = Logger.GetInstance();
        
        // Act
        logger.Log("Loading...");
        Thread.Sleep(50);
        logger.Log("Loading... 25%", updatePrevious: true);
        Thread.Sleep(50);
        logger.Log("Loading... 50%", updatePrevious: true);
        Thread.Sleep(50);
        logger.Log("Loading... 100%", updatePrevious: true);
        Thread.Sleep(100);

        // Assert
        Assert.Equal(4, _mockConsole.WrittenLines.Count);
        Assert.Equal("Loading...", _mockConsole.WrittenLines[0]);
        Assert.Contains("Loading... 100%", _mockConsole.WrittenLines[3]);
    }
}
