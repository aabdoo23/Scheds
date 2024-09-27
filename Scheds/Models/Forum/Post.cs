using System.Text.Json.Serialization;

namespace Scheds.Models.Forum
{
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [JsonIgnore] 
        public virtual List<Comment> Comments { get; set; }

        public virtual List<Attachment> Attachments { get; set; }
    }
}
