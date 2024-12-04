namespace EcommApp.Models.DTO
{
    public class LoginResponseDto
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string JwtToken { get; set; }
    }
}
