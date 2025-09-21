# LineWalker

[![NuGet Version](https://img.shields.io/nuget/v/LineWalker)](https://www.nuget.org/packages/LineWalker/)
[![MIT License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0+-blue.svg)](https://dotnet.microsoft.com/)

A **lightweight**, **thread-safe**, **singleton** console logging library for .NET that supports **real-time message updates** and **colored output**. Perfect for creating dynamic console applications with progress indicators, status updates, and structured logging.

## ✨ Key Features

- **🔄 Real-time Message Updates**: Update previous console output without cluttering the display
- **🎨 Colored Output**: Built-in support for different log levels with automatic color coding
- **🧵 Thread-Safe**: Asynchronous logging with internal message queue
- **📦 Lightweight**: Minimal dependencies and small footprint
- **⚡ Fast Setup**: Singleton pattern - ready to use in seconds
- **🔧 Flexible API**: Multiple overloads for different message types
- **📝 Multi-line Support**: Handle complex messages with multiple lines

## 🚀 Quick Start

### Installation

Install via NuGet Package Manager:

```bash
dotnet add package LineWalker
```

Or via Package Manager Console:

```powershell
Install-Package LineWalker
```

### Basic Usage

```csharp
using LineWalker;

// Get the logger instance
var logger = Logger.GetInstance();

// Simple logging
logger.Log("Hello, World!");

// Colored logging with different levels
logger.Log("This is a warning", LogLevel.Warning);
logger.Log("This is an error", LogLevel.Error);

// Update previous message (great for progress indicators)
logger.Log("Loading...");
await Task.Delay(1000);
logger.Log("Loading... 50%", updatePrevious: true);
await Task.Delay(1000);
logger.Log("Loading... 100% Complete!", updatePrevious: true);
```

### Output Example

```
Hello, World!
⚠️  This is a warning
❌ This is an error
Loading... 100% Complete!
```

## 📚 Advanced Usage

### Progress Indicators

```csharp
var logger = Logger.GetInstance();

logger.Log("Starting process...");

for (int i = 0; i <= 100; i += 10)
{
    var progress = $"Progress: [{new string('█', i / 10)}{new string('░', 10 - i / 10)}] {i}%";
    logger.Log(progress, updatePrevious: i > 0);
    await Task.Delay(200);
}

logger.Log("✅ Process completed!", updatePrevious: true);
```

### Multi-line Messages

```csharp
var logger = Logger.GetInstance();

string[] multiLineMessage = {
    "╔══════════════════════════╗",
    "║       System Status      ║",
    "╠══════════════════════════╣",
    "║ Database: ✅ Connected    ║",
    "║ API: ✅ Online           ║",
    "║ Cache: ⚠️ Warning        ║",
    "╚══════════════════════════╝"
};

logger.Log(multiLineMessage);
```

### Different Log Levels

```csharp
var logger = Logger.GetInstance();

logger.Log("Debug information", LogLevel.Debug);        // Dark gray
logger.Log("General information", LogLevel.Info);       // Default color
logger.Log("Warning message", LogLevel.Warning);        // Yellow
logger.Log("Error occurred", LogLevel.Error);           // Red
logger.Log("Critical failure", LogLevel.Critical);      // Dark red
```

### Working with Objects

```csharp
var logger = Logger.GetInstance();

var data = new { Name = "John", Age = 30, Status = "Active" };
logger.Log(data, LogLevel.Info);

// Output: { Name = John, Age = 30, Status = Active }
```

## 🏗️ API Reference

### Logger Class

#### Static Methods

| Method | Description |
|--------|-------------|
| `GetInstance()` | Returns the singleton logger instance |
| `Shutdown()` | Gracefully shuts down the logger and waits for background thread |

#### Instance Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| `Log(string message, bool updatePrevious = false)` | `message`: Text to log<br>`updatePrevious`: Whether to replace the previous message | Basic string logging |
| `Log(string message, LogLevel level, bool updatePrevious = false)` | `message`: Text to log<br>`level`: Log level for coloring<br>`updatePrevious`: Whether to replace previous message | String logging with level |
| `Log(string[] message, bool updatePrevious = false)` | `message`: Array of strings to log<br>`updatePrevious`: Whether to replace previous message | Multi-line string logging |
| `Log(string[] message, LogLevel level, bool updatePrevious = false)` | `message`: Array of strings<br>`level`: Log level<br>`updatePrevious`: Whether to replace previous message | Multi-line logging with level |
| `Log(object message, LogLevel level = LogLevel.Info, bool updatePrevious = false)` | `message`: Any object to log<br>`level`: Log level<br>`updatePrevious`: Whether to replace previous message | Object logging |

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `QueueCount` | `int` | Number of messages currently in the queue waiting to be output |

### LogLevel Enum

| Value | Description | Color |
|-------|-------------|-------|
| `Debug` | Debug information | Dark Gray |
| `Info` | General information | Default |
| `Warning` | Warning messages | Yellow |
| `Error` | Error messages | Red |
| `Critical` | Critical failures | Dark Red |

## 💪 Strengths

- **⚡ Performance**: Asynchronous background processing doesn't block your main thread
- **🎯 Simplicity**: Minimal API surface - easy to learn and use
- **🔄 Dynamic Updates**: Unique ability to update previous console output in real-time
- **🧵 Thread Safety**: Built-in concurrency handling with ConcurrentQueue
- **📦 Zero Dependencies**: No external dependencies beyond .NET runtime
- **🎨 Visual Appeal**: Automatic color coding improves readability
- **💾 Memory Efficient**: Lightweight design with minimal memory footprint

## ⚠️ Limitations

- **📺 Console Only**: Designed specifically for console applications
- **🔒 Singleton Pattern**: Global state may not suit all architectural patterns
- **📝 No File Output**: Currently only supports console output (no file logging)
- **🎨 Basic Formatting**: Limited formatting options compared to full-featured loggers
- **🔍 No Filtering**: No built-in log level filtering or configuration
- **📊 No Structured Logging**: No support for structured/semantic logging
- **🔌 No Extensibility**: Cannot add custom appenders or formatters

## 🔧 Best Practices

### Graceful Shutdown

```csharp
// In your application's cleanup code
Logger.Shutdown();
```

### Performance Considerations

```csharp
var logger = Logger.GetInstance();

// Check queue depth if needed
if (logger.QueueCount > 100)
{
    // Maybe slow down message generation
    await Task.Delay(10);
}
```

### Thread Safety

```csharp
// Safe to call from multiple threads
Parallel.For(0, 100, i =>
{
    Logger.GetInstance().Log($"Processing item {i}");
});
```

## 🎯 Use Cases

LineWalker is perfect for:

- **CLI Applications** with progress indicators
- **Build Tools** showing compilation progress
- **Data Processing** scripts with status updates
- **Simple Console Apps** needing basic logging
- **Development/Debugging** scenarios
- **Batch Processing** with real-time status

LineWalker might not be suitable for:

- **Web Applications** (use proper logging frameworks)
- **Services** requiring file logging
- **Applications** needing complex log filtering
- **Enterprise Applications** requiring structured logging

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**Samuele Lorefice**

- GitHub: [@SamueleLorefice](https://github.com/SamueleLorefice)

## 🔗 Links

- [NuGet Package](https://www.nuget.org/packages/LineWalker/)
- [Source Code](https://github.com/SamueleLorefice/LineWalker)
- [Issues](https://github.com/SamueleLorefice/LineWalker/issues)

---

*Made with ❤️ for the .NET community*