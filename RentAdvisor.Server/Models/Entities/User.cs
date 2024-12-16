using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;

namespace RentAdvisor.Server.Models.Entities
{
    public class User : IdentityUser
    {
        [JsonIgnore]
        public virtual ICollection<Review> Reviews { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime lastLogin { get; set; } = DateTime.UtcNow;
    }
}
