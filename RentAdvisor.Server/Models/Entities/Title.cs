using System.Text.Json.Serialization;

namespace RentAdvisor.Server.Models.Entities
{
    public class Title
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public int RequiredPoints { get; set; }

        [JsonIgnore]
        public virtual ICollection<User> Users { get; set; }
    }
}
