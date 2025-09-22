using LineWalker;

using (var logger = Logger.GetInstance()) {
    logger.Log("Hello, World!");

    logger.Log("Update me!", LogLevel.Warning, updatePrevious:false);
    Thread.Sleep(250);
    logger.Log("Update me again!", LogLevel.Warning, updatePrevious:true);
    Thread.Sleep(250);
    logger.Log("Finished!", updatePrevious:true);
    Thread.Sleep(250);
    logger.Log("DONE!", updatePrevious:false);

}
Console.WriteLine("Logger is now out of scope!");

Environment.Exit(0);
