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

Environment.Exit(0);