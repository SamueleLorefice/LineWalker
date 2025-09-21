# LineWalker

A **lightweight**, **thread-safe**, **singleton** console logging library for .NET with support for real-time message updates and colored output.

## ✨ Features

- 🔄 **Real-time message updates** - Update previous console output without cluttering
- 🎨 **Colored output** - Automatic color coding for different log levels  
- 🧵 **Thread-safe** - Asynchronous logging with internal message queue
- 📦 **Lightweight** - Minimal dependencies and small footprint
- ⚡ **Fast setup** - Singleton pattern, ready to use instantly

## 🚀 Quick Start

### Installation

```bash
dotnet add package LineWalker
```

### Basic Usage

```csharp
using LineWalker;

public static void Main(string[] args)
{
    // Get the logger instance
    var logger = Logger.GetInstance();

    // Log a message to the console
    logger.Log("Hello, World!");

    // Log another message as a warning
    logger.Log("Goodbye, World!", LogLevel.Warning);
    
    // Log a status update
    logger.Log("Loading...");
    
    // Update the previous message (great for progress indicators)
    logger.Log("Loading... 50% done", updatePrevious: true);
    logger.Log("Loading... 100% done", updatePrevious: true);
    
    // Clean shutdown
    Logger.Shutdown();
}

/* Output:
Hello, World!
Goodbye, World!
Loading... 100% done
*/
```

## 📚 Advanced Examples

### Progress Indicators

```csharp
var logger = Logger.GetInstance();

logger.Log("Processing...");
for (int i = 0; i <= 100; i += 10)
{
    var progress = $"Progress: {i}%";
    logger.Log(progress, updatePrevious: i > 0);
    await Task.Delay(100);
}
```

### Multi-line Messages

```csharp
string[] status = {
    "System Status:",
    "✅ Database: Connected",
    "⚠️  Cache: Warning"
};
logger.Log(status, LogLevel.Info);
```

### Object Logging

```csharp
var data = new { Name = "John", Status = "Active" };
logger.Log(data, LogLevel.Info);
```

## 🎯 Log Levels

| Level | Color | Usage |
|-------|-------|-------|
| `Debug` | Dark Gray | Debug information |
| `Info` | Default | General information |
| `Warning` | Yellow | Warning messages |
| `Error` | Red | Error messages |  
| `Critical` | Dark Red | Critical failures |

## 📖 Full Documentation

For complete documentation, examples, and API reference, visit the [GitHub repository](https://github.com/SamueleLorefice/LineWalker).

---

**Author:** Samuele Lorefice  
**License:** MIT