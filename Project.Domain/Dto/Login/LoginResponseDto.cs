namespace Project.Domain.Dto.Login
{
    public class LoginResponseDto
    {
        public LoginUserDetails UserDetails { get; set; }
        public string? AccessToken { get; set; }
    }
}
