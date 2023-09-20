using log4net;
using log4net.Config;

namespace Synchronizer;

internal class Synchronizator
{
    private readonly ILog log;
    private readonly string sourceDir;
    private readonly string replicaDir;
    private readonly FileHelpers fileHelpers = new();

    public Synchronizator(string sourceDir, string replicaDir)
    {
        this.sourceDir = sourceDir;
        this.replicaDir = replicaDir;

        log = LogManager.GetLogger(typeof(Synchronizator));
        System.Text.Encoding.RegisterProvider(
            System.Text.CodePagesEncodingProvider.Instance);
        XmlConfigurator.Configure(new FileInfo("log4net.config"));
    }

    public void ProcessFolders()
    {
        ProcessFolders(sourceDir, replicaDir);
    }

    private void ProcessFolders(string sourceDir, string replicaDir)
    {
        if (!Directory.Exists(sourceDir))
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
            if (folderThatExistsInSourceAndReplica != null)
            {
                log.Debug($"Nesting process to {sourceSubdirectory} and {folderThatExistsInSourceAndReplica}");
                ProcessFolders(sourceSubdirectory, folderThatExistsInSourceAndReplica);
            }
            else
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
                FileHelpers.DeleteDirectory(replicaSubdirectory);
            }
        }

        ProcessFiles(sourceDir, replicaDir);
    }

    public void ProcessFiles(string sourceDir, string replicaDir)
    {
        var sourceDirFileNames = Directory.GetFiles(sourceDir);
        var replicaDirFileNames = Directory.GetFiles(replicaDir);

        foreach (var sourceFile in sourceDirFileNames)
        {
            var replicaFileThatExistInBothDirectories = replicaDirFileNames.FirstOrDefault(replicaFile => Path.GetFileName(replicaFile) == Path.GetFileName(sourceFile));
            if (replicaFileThatExistInBothDirectories is not null)
            {
                if (FileHelpers.GetMD5Checksum(replicaFileThatExistInBothDirectories) != FileHelpers.GetMD5Checksum(sourceFile))
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

}
