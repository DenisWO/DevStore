using DS.Core.DomainObjects;
using System.Text.Json.Serialization;

namespace DS.Identity.API.Entities
{
    public class User : Entity
    {
        public string Email { get; set; }
        public Role Role { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }
        [JsonIgnore]
        public string PasswordSaltHash { get; set; }
    }
}
