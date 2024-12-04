using System.ComponentModel.DataAnnotations;

namespace EcommApp.Models.DTO
{
    public class ForgotPasswordRequestDto
    {

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
