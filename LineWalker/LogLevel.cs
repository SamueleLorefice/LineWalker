namespace LineWalker;

/// <summary>
/// Defines the severity level of a log message.
/// </summary>
public enum LogLevel {
    /// <summary>
    /// Extremely detailed logs, typically used for diagnosing issues.
    /// </summary>
    Trace,
    /// <summary>
    /// Additional information useful for debugging.
    /// </summary>
    Debug,
    /// <summary>
    /// General operational entries about what's going on inside the application.
    /// </summary>
    Info,
    /// <summary>
    /// Non-critical issues that may require attention.
    /// </summary>
    Warning,
    /// <summary>
    /// An error occurred, indicating a failure in a specific operation.
    /// </summary>
    Error,
    /// <summary>
    /// A severe error indicating an unrecoverable state in the application.
    /// </summary>
    Critical
}