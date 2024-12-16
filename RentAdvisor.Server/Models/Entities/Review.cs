using System.Text.Json.Serialization;

namespace RentAdvisor.Server.Models.Entities
{
    public class Review
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string UserId { get; set; }
        public virtual User User { get; set; }
        public Guid PropertyId { get; set; }
        [JsonIgnore]
        public virtual Property Property { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
