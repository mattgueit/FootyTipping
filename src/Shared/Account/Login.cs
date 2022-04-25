using System.ComponentModel.DataAnnotations;

namespace FootyTipping.Shared.Account
{
    public class Login
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
