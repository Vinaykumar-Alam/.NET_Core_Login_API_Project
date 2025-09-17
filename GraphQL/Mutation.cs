using Flutter_Backed.Data;
using Flutter_Backed.Models;
using Flutter_Backed.utils;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Flutter_Backed.GraphQL
{
    public class Mutation
    {

        public async Task<Login> Register(string userName, string password, string role, [Service] SQLDBContext _sqlDBContext)
        {
            var user = new Login { UserName = userName, HashPassword = password, Role = role };
            _sqlDBContext.LoginTbl.Add(user);
            await _sqlDBContext.SaveChangesAsync();
            return user;
        }

        public async Task<LoginResponse> Login(string userName, string password, [Service] SQLDBContext sQLDBContext, [Service] IConfiguration configuration, [Service] loginHelper loginHelper)
        {

            // Check if user exists in DB by username
            var user = await sQLDBContext.LoginTbl.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == userName);

            // If user does not exist OR password is incorrect → return Unauthorized
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.HashPassword))
            {
                return new LoginResponse
                {
                    Message = "Invalid Username or Password"
                };
            }

            // Generate JWT Access Token
            var token = loginHelper.GenerateJwtToken(user);

            // Generate a secure Refresh Token
            var refreshToken = loginHelper.GenerateRefreshToken();

            // Create refresh token entry for DB
            var Ref = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = refreshToken,
                ExpiresOn = DateTime.Now.AddHours(Convert.ToInt32(configuration["JWT:RefreshTokenExpiryHours"])), // Expiry from config
                CreatedOn = DateTime.Now
            };

            // Save refresh token in DB
            sQLDBContext.RefreshToken_Tbl.Add(Ref);
            await sQLDBContext.SaveChangesAsync();

            // Return both tokens to client
            return new LoginResponse { Message = "Successfully logged In", AccessToken = token, RefreshToken = refreshToken };
        }

    }
}