

using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Flutter_Backed.Models
{
    [Table("Tbl_Test_UserLogin")]
    public class Login
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string UserName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public string CreatedOn { get; set; }
        public bool IsActive { get; set; }
        public string HashPassword { get; set; }
        [System.ComponentModel.DefaultValue(UserRoles.USER)]
        public string? Role { get; set; }

    }
    public class LoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? Role { get; set; }
    }

    [Table("Tbl_RefreshToken")]
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime RevokenOn { get; set; }
    }
    public class RefreshTokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
    public class LoginResponse
    {
        public string? Message { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
