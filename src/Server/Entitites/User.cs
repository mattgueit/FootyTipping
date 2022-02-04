using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace FootyTipping.Server.Entitites
{
    public class User
    {
        public int Id { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string LastName { get; set; }

        [Column(TypeName = "VARCHAR")]
        [StringLength(50)]
        public string Username  { get; set; }

        [JsonIgnore]
        public string PasswordHash { get; set; }
    }
}
