using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace RentAdvisor.Server.Models.Entities
{
    public class User : IdentityUser
    {
        public int Score { get; set; } = 0;

        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; }
        [JsonIgnore]
        public virtual ICollection<Property> Properties { get; set; }
        [JsonIgnore]
        public virtual ICollection<PropertyPhotos> PropertyPhotos { get; set; }
        public virtual ICollection<Badge> Badges { get; set; }
        public Guid TitleId { get; set; } = new Guid("caabb5a5-9ddf-4b67-aa52-dafe7eef748a");
        public virtual Title Title { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime lastLogin { get; set; } = DateTime.UtcNow;
    }
}
