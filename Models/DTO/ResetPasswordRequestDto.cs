using System.ComponentModel.DataAnnotations;

namespace EcommApp.Models.DTO
{
    public class ResetPasswordRequestDto
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

    }
}
