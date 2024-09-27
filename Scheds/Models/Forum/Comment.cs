using System.Text.Json.Serialization;

namespace Scheds.Models.Forum
{
    public class Comment
    {
        public int CommentId { get; set; }
        public string Content { get; set; }
        public int Likes { get; set; }
        public DateTime CreatedAt { get; set; }
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [JsonIgnore]
        public virtual Post Post { get; set; }

        public int PostId { get; set; }
    }
}
