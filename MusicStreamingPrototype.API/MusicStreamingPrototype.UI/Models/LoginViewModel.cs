using System.ComponentModel.DataAnnotations;

namespace MusicStreamingPrototype.UI.Models
{
    public class LoginViewModel
    {
        [Required, Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = "";

        [Required, DataType(DataType.Password), Display(Name = "Parola")]
        public string Password { get; set; } = "";

        public string? ReturnUrl { get; set; }
    }
}
