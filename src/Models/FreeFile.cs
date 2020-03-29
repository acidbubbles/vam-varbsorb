namespace Varbsorb
{
    public class FreeFile
    {
        public string Path { get; set; }
        public string LocalPath { get; set; }

        public FreeFile(string vam, string path)
        {
            Path = path;
            LocalPath = path.Substring(vam.Length + 1);
        }
    }
}