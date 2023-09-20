using log4net;
using log4net.Config;
using Synchronizer;

uint synchronizationInterval;

if (args.Length != 4) PrintUsageAndExit("Invalid amount of arguments provided");
if (!Directory.Exists(args[0])) PrintUsageAndExit("Source directory does not exist");
if (!Directory.Exists(args[1])) PrintUsageAndExit("Replica directory does not exist");
if (!uint.TryParse(args[2], out synchronizationInterval)) PrintUsageAndExit("Invalid synchronization interval value");
if (!Directory.Exists(System.IO.Path.GetDirectoryName(args[3]))) PrintUsageAndExit("Log file can be created only in the directory that exists");

var sourceDirectory = args[0];
var replicaDirectory = args[1];

ILog log = LogManager.GetLogger(typeof(Program));
System.Text.Encoding.RegisterProvider(
    System.Text.CodePagesEncodingProvider.Instance);
GlobalContext.Properties["LogName"] = args[3];
XmlConfigurator.Configure(new FileInfo("log4net.config"));

// create function that will trigger the synchronization with specified interval on the input between two folders 
// https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-7.0&red

ProcessFolders(sourceDirectory, replicaDirectory);

void ProcessFolders(string sourceDir, string replicaDir)
{
    var sourceDirSubdirectories = Directory.GetDirectories(sourceDir);
    var replicaDirSubdirectories = Directory.GetDirectories(replicaDir);
    var sourceDirFileNames = Directory.GetFiles(sourceDir);
    var replicaDirFileNames = Directory.GetFiles(replicaDir);

    foreach (var sourceSubdirectory in sourceDirSubdirectories)
    {
        var folderThatExistsInSourceAndReplica = replicaDirSubdirectories.FirstOrDefault(replica => Path.GetFileName(Path.GetFileName(replica)) == Path.GetFileName(Path.GetFileName(sourceSubdirectory)));
        if (folderThatExistsInSourceAndReplica != null) {
            log.Info($"Going nested :-) to: {sourceSubdirectory} and {folderThatExistsInSourceAndReplica}");
            ProcessFolders(sourceSubdirectory, folderThatExistsInSourceAndReplica);
        } else
        {
            log.Info($"Copying source subdirectory {sourceSubdirectory} to {replicaDir}");
            var kupa =  Path.GetFileName(sourceSubdirectory) ;
            FileHelpers.CopyDirectory(sourceSubdirectory, Path.GetFullPath($"{replicaDir}/{Path.GetFileName(Path.GetFileName(sourceSubdirectory))}"));
        }
    }

    foreach (var replicaSubdirectory in replicaDirSubdirectories)
    {
        var x = Path.GetFileName(Path.GetFileName(replicaSubdirectory));
        var y = Path.GetFileName(replicaSubdirectory);

        if (!sourceDirSubdirectories.Any(source => Path.GetFileName(Path.GetFileName(source)) == Path.GetFileName(Path.GetFileName(replicaSubdirectory))))
        {
            log.Info($"Deleting {replicaSubdirectory}");
            FileHelpers.DeleteDirectory(replicaSubdirectory);
        }
    }

    //jeszcze zająć się porownywaniem plikow (porownanie nazw i checksumy)
    ProcessFiles(sourceDir, replicaDir);
}

void ProcessFiles(string sourceDir, string replicaDir)
{

}

void PrintUsageAndExit(string reasonMsg = "")
{
    if (reasonMsg?.Length > 0) Console.WriteLine($"Invalid input provided. Reason:{reasonMsg}");

    if (OperatingSystem.IsWindows())
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("Synchronizer.exe {sourceDirectoryPath} {replicaDirectoryPath} {intervalInSeconds} {logfilePath}");

    } else
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("./Synchronizer SOURCEDIRECTORYPATH REPLICADIRECTORYPATH SYNCINTERVALINSECONDS LOGFILEPATH");

    }

    Environment.Exit(-1);
}