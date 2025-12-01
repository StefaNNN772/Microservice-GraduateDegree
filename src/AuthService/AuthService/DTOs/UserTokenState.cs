namespace AuthService.DTOs
{
    public class UserTokenState
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
