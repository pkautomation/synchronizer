using log4net;
using log4net.Config;
using Synchronizer;
using System.Timers;
using static System.Net.Mime.MediaTypeNames;

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
var workingDir = AppDomain.CurrentDomain.BaseDirectory;
XmlConfigurator.Configure(new FileInfo(Path.Combine(workingDir, "./log4net.config")));

Synchronizator synchronizator = new(sourceDirectory, replicaDirectory);

log.Debug($"Source directory set to:{sourceDirectory}");
log.Debug($"Replica directory set to:{replicaDirectory}");
log.Debug($"Synchronization interval set to:{synchronizationInterval} seconds");
log.Debug($"Logger file path set to:{args[3]}");

System.Timers.Timer timer;
timer = new System.Timers.Timer(synchronizationInterval * 1000);
timer.Elapsed += OnTimedEvent!;
timer.AutoReset = true;
timer.Enabled = true;

Console.ReadLine();

void OnTimedEvent(Object source, ElapsedEventArgs e)
{
    try
    {
        log.Debug("Syncing...");
        synchronizator.ProcessFolders();
    } catch (Exception ex)
    {
        log.Error("Exception has been thrown:" + ex.Message);
        Environment.Exit(-1);
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