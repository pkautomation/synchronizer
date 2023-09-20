using log4net;
using log4net.Config;
using Synchronizer;
using System.Timers;

FileHelpers fileHelpers = new();
uint synchronizationInterval;

if (args.Length != 4) PrintUsageAndExit("Invalid amount of arguments provided");
if (!Directory.Exists(args[0])) PrintUsageAndExit("Source directory does not exist");
if (!Directory.Exists(args[1])) PrintUsageAndExit("Replica directory does not exist");
if (!uint.TryParse(args[2], out synchronizationInterval)) PrintUsageAndExit("Invalid synchronization interval value");
if (uint.Parse(args[2]) == 0) PrintUsageAndExit("Synchronization interval should be greater than 0");
if (!Directory.Exists(Path.GetDirectoryName(args[3]))) PrintUsageAndExit("Log file can be created only in the directory that exists");

var sourceDirectory = args[0];
var replicaDirectory = args[1];

ILog log = LogManager.GetLogger(typeof(Program));
System.Text.Encoding.RegisterProvider(
    System.Text.CodePagesEncodingProvider.Instance);
GlobalContext.Properties["LogName"] = args[3];
XmlConfigurator.Configure(new FileInfo("log4net.config"));

System.Timers.Timer timer;
timer = new System.Timers.Timer(synchronizationInterval * 1000);
timer.Elapsed += OnTimedEvent!;
timer.AutoReset = true;
timer.Enabled = true;

log.Debug($"Source directory set to:{sourceDirectory}");
log.Debug($"Replica directory set to:{replicaDirectory}");
log.Debug($"Synchronization interval set to:{synchronizationInterval} seconds");
log.Debug($"Logger file path set to:{args[3]}");

Console.ReadLine();

void OnTimedEvent(Object source, ElapsedEventArgs e)
{
    log.Debug("Syncing...");
    ProcessFolders(sourceDirectory, replicaDirectory);
}

void ProcessFolders(string sourceDir, string replicaDir)
{
    if(!Directory.Exists(sourceDir))
    {
        log.Error("Source directory no longer exist. Exiting the program");
        Environment.Exit(-1);
    }

    if (!Directory.Exists(replicaDir))
    {
        log.Error("Replica directory no longer exist. Exiting the program");
        Environment.Exit(-1);
    }

    var sourceDirSubdirectories = Directory.GetDirectories(sourceDir);
    var replicaDirSubdirectories = Directory.GetDirectories(replicaDir);
    var sourceDirFileNames = Directory.GetFiles(sourceDir);
    var replicaDirFileNames = Directory.GetFiles(replicaDir);

    foreach (var sourceSubdirectory in sourceDirSubdirectories)
    {
        var folderThatExistsInSourceAndReplica = replicaDirSubdirectories.FirstOrDefault(replica => Path.GetFileName(replica) == Path.GetFileName(sourceSubdirectory));
        if (folderThatExistsInSourceAndReplica != null) {
            log.Debug($"Nesting process to {sourceSubdirectory} and {folderThatExistsInSourceAndReplica}");
            ProcessFolders(sourceSubdirectory, folderThatExistsInSourceAndReplica);
        } else
        {
            log.Info($"Copying source subdirectory {sourceSubdirectory} to {replicaDir}");
            fileHelpers.CopyDirectory(sourceSubdirectory, Path.GetFullPath($"{replicaDir}/{Path.GetFileName(sourceSubdirectory)}"));
        }
    }

    foreach (var replicaSubdirectory in replicaDirSubdirectories)
    {
        if (!sourceDirSubdirectories.Any(source => Path.GetFileName(source) == Path.GetFileName(replicaSubdirectory)))
        {
            log.Info($"Deleting directory {replicaSubdirectory}");
            fileHelpers.DeleteDirectory(replicaSubdirectory);
        }
    }

    ProcessFiles(sourceDir, replicaDir);
}

void ProcessFiles(string sourceDir, string replicaDir)
{
    var sourceDirFileNames = Directory.GetFiles(sourceDir);
    var replicaDirFileNames = Directory.GetFiles(replicaDir);

    foreach (var sourceFile in sourceDirFileNames)
    {
        var replicaFileThatExistInBothDirectories = replicaDirFileNames.FirstOrDefault(replicaFile => Path.GetFileName(replicaFile) == Path.GetFileName(sourceFile));
        if (replicaFileThatExistInBothDirectories is not null)
        {
            if (fileHelpers.GetMD5Checksum(replicaFileThatExistInBothDirectories) != fileHelpers.GetMD5Checksum(sourceFile))
            {
                log.Info($"Replacing {replicaFileThatExistInBothDirectories} with {sourceFile}");
                File.Copy(sourceFile, replicaFileThatExistInBothDirectories, true);
            }
        }
        else
        {
            log.Info($"Copying source file {sourceFile} to {replicaDir}");
            File.Copy(sourceFile, Path.GetFullPath($"{replicaDir}/{Path.GetFileName(sourceFile)}"));
        }
    }

    foreach (var replicaFile in replicaDirFileNames)
    {
        if (!sourceDirFileNames.Any(sourceFile => Path.GetFileName(sourceFile) == Path.GetFileName(replicaFile)))
        {
            log.Info($"Deleting file {replicaFile}");
            File.Delete(replicaFile);
        }
    }
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