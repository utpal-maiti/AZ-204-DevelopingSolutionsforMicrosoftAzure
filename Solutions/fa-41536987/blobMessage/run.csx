public static void Run(Stream myBlob, string name, ILogger log, out string outputQueueItem)
{
    outputQueueItem = name;
    log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");
}