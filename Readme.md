# Line Walker

A Singleton, quick to set up, minimal and lightweight console logging library with support to updating the previous message.

## How to use

```cs
public static void Main(string[] args)
{
    // Grab a reference to the logger instance.
    static var logger = Linewalker.Logger.GetInstance();

    // Log a message to the console
    lineWalker.Log("Hello, World!");


    // Log another message as a warning this time.
    lineWalker.Log("Goodbye, World!", Level:LogLevel.Warning);
    
    // Log a status update for the first time.
    lineWalker.Log("Loading...");
    
    // Log a status update for the second time. this will update the previous message.
    lineWalker.Log("Loading... 50% done", updatePrevious: true);
    
    // Log a status update for the third time. this will update the previous message.
    lineWalker.Log("Loading... 100% done", updatePrevious: true);
    
}

/* Output:
Hello, World!
Goodbye, World!
Loading... 100% done
*/
```