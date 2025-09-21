using LineWalker;

var logger = Logger.GetInstance();

logger.Log("Hello, World!");

logger.Log("Update me!", LogLevel.Warning, updatePrevious:false);
Thread.Sleep(250);
logger.Log("Update me again!", LogLevel.Warning, updatePrevious:true);
Thread.Sleep(250);
logger.Log("Finished!", updatePrevious:true);
Thread.Sleep(250);
logger.Log("DONE!", updatePrevious:false);

// Wait for all messages to be processed before exiting
while (logger.QueueCount > 0)
{
    Thread.Sleep(50);
}

// Give a little extra time for the last message to be processed
Thread.Sleep(100);

// Properly shutdown the logger
Logger.Shutdown();