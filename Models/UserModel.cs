namespace Mobile_shop_Frontend.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
    
    public class UserAuthModel
    {
        public string UserEmail { get; set; }
        public string Password { get; set; }
    }
    
    public class SignInResponse
    {
        public UserModel UserDetails { get; set; }
        public string Message { get; set; }
        public string AuthToken { get; set; }
    }

}