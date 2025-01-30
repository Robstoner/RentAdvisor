using System.Text.Json.Serialization;

namespace RentAdvisor.Server.Models.Entities
{
    public class Badge
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        [JsonIgnore]
        public virtual User[] Users { get; set; }
    }
}
