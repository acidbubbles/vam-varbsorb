namespace Varbsorb.Models
{
    public class SceneReference
    {
        public FreeFile File { get; set; }
        public int Index { get; set; }
        public int Length { get; set; }

        public SceneReference(FreeFile file, int index, int length)
        {
            File = file;
            Index = index;
            Length = length;
        }
    }
}
