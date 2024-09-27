namespace Scheds.Models.Forum
{
    public class Attachment
    {
        public int AttachmentId { get; set; }
        public string FilePath { get; set; }
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
    }
}
