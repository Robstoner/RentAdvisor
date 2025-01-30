using System.Text.Json.Serialization;

namespace RentAdvisor.Server.Models.Entities
{
    public class Property
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public string[] Features { get; set; }
        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<PropertyPhotos> PropertyPhotos { get; set; }
        public string UserId { get; set; }
        [JsonIgnore]
        public virtual User User { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
