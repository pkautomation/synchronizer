namespace Synchronizer;
internal class FileHelpers
{
   public string GetMD5Checksum(string filename)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        using var stream = File.OpenRead(filename);
        var hash = md5.ComputeHash(stream);

        return BitConverter.ToString(hash);
    }

    public void DeleteDirectory(string path)
    {
        Directory.Delete(path, true);
    }

    public void CopyDirectory(string sourceDirectory, string destDirectory)
    {
        Directory.CreateDirectory(destDirectory);
        string[] files = Directory.GetFiles(sourceDirectory);
        foreach (string file in files)
        {
            string name = Path.GetFileName(file);
            string dest = Path.Combine(destDirectory, name);
            File.Copy(file, dest);
        }
        string[] folders = Directory.GetDirectories(sourceDirectory);
        foreach (string folder in folders)
        {
            string name = Path.GetFileName(folder);
            string dest = Path.Combine(destDirectory, name);
            CopyDirectory(folder, dest);
        }
    }
}
