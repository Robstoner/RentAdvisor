using System.Text.Json.Serialization;

namespace RentAdvisor.Server.Models.Entities
{
    public class PropertyPhotos
    {
        public Guid Id { get; set; }
        public string PhotoPath { get; set; } = string.Empty;
        public Guid PropertyId { get; set; }
        [JsonIgnore]
        public virtual Property Property { get; set; }
        public string UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
