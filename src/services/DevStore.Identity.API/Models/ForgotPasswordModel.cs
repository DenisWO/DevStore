using System.ComponentModel.DataAnnotations;

namespace DevStore.Identity.API.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
