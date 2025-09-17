using Flutter_Backed.Data;              // Import DB context namespace
using Flutter_Backed.Models;            // Import models (Login, DTOs, etc.)
using Flutter_Backed.utils;             // Import helper classes (e.g., JWT generator)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;    // For EF Core database operations
using System.Security.Claims;           // For working with Claims (user identity data)

namespace Flutter_Backed.Controllers
{
    [ApiController]                    // Marks this class as an API controller
    [Route("[controller]")]             // Route will be "/Login" (controller name)
    public class LoginController : Controller
    {
        private readonly SQLDBContext _sqlDBContext; // Database context for DB operations
        private readonly IConfiguration _configuration; // To access config (like JWT settings)
        private readonly loginHelper _loginHelper;   // Helper class for JWT/Refresh token

        // Constructor - initializes dependencies (dependency injection)
        public LoginController(SQLDBContext sqlDBContext, IConfiguration configuration, loginHelper loginHelper)
        {
            _sqlDBContext = sqlDBContext;
            _configuration = configuration;
            _loginHelper = loginHelper;
        }

        // POST: /Login/Login
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                // Validate incoming request model
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                // Check if user exists in DB by username
                var user = await _sqlDBContext.LoginTbl.AsNoTracking().FirstOrDefaultAsync(x => x.UserName == loginDto.UserName);

                // If user does not exist OR password is incorrect → return Unauthorized
                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.HashPassword))
                {
                    return Unauthorized("Invalid email or password");
                }

                // Generate JWT Access Token
                var token = _loginHelper.GenerateJwtToken(user);

                // Generate a secure Refresh Token
                var refreshToken = _loginHelper.GenerateRefreshToken();

                // Create refresh token entry for DB
                var Ref = new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpiresOn = DateTime.Now.AddHours(Convert.ToInt32(_configuration["JWT:RefreshTokenExpiryHours"])), // Expiry from config
                    CreatedOn = DateTime.Now
                };

                // Save refresh token in DB
                _sqlDBContext.RefreshToken_Tbl.Add(Ref);
                await _sqlDBContext.SaveChangesAsync();

                // Return both tokens to client
                return Ok(new { AccessToken = token, RefreshToken = refreshToken });
            }
            catch (Exception ex)
            {
                // General error handling
                return BadRequest("Something went wrong");
            }
        }

        // POST: /Login/Register
        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Create new user entity
                var user = new Login
                {
                    Id = Guid.NewGuid(),                                         // Generate unique ID
                    UserName = loginDto.UserName,                               // Save username
                    Email = loginDto.UserName.Replace(" ", "") + "@gmail.com",  // Dummy email generation
                    HashPassword = BCrypt.Net.BCrypt.HashPassword(loginDto.Password), // Securely hash password
                    IsActive = true,                                            // By default active
                    Role = loginDto.Role != null ? loginDto.Role : UserRoles.USER.ToString(), // Assign role
                    CreatedOn = DateTime.Now.ToString()                         // Created date
                };

                // Add user to DB
                _sqlDBContext.LoginTbl.Add(user);
                await _sqlDBContext.SaveChangesAsync();

                return Ok("User created");
            }
            catch (DbUpdateException)
            {
                // Handles unique constraint (duplicate username/email)
                return BadRequest("UserName/Email already exists.");
            }
            catch (Exception)
            {
                return BadRequest("Something went wrong");
            }
        }

        // POST: /Login/Refresh
        // This endpoint is secured with [Authorize] (only valid tokens can access)
        [Authorize]
        [HttpPost("[action]")]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                // Check if Refresh Token is null/empty
                if (refreshTokenDto.RefreshToken == null || string.IsNullOrEmpty(refreshTokenDto.RefreshToken))
                {
                    return BadRequest("Invalid Refres Token");
                }

                // Extract principal (claims) from Access Token
                var principal = _loginHelper.GetPrincipalFromAccessToken(refreshTokenDto.AccessToken);

                if (principal == null)
                {
                    return BadRequest("Invalid Access Token");
                }

                // Get userId claim from token
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return BadRequest("Invalid Access Token");
                }

                // Fetch stored refresh token from DB for that user
                var userRefDtl = _sqlDBContext.RefreshToken_Tbl
                                  .Where(t => t.UserId == Guid.Parse(userId))
                                  .FirstOrDefault();

                // Validate refresh token: must match and not expired
                if (userRefDtl.Token != refreshTokenDto.RefreshToken || userRefDtl.ExpiresOn < DateTime.Now)
                {
                    return BadRequest("Invalid or Expired Refresh Token");
                }

                // Get user details
                var user = await _sqlDBContext.LoginTbl.FirstOrDefaultAsync(w => w.Id == userRefDtl.UserId);

                // Generate new Access Token
                var NewAT = _loginHelper.GenerateJwtToken(user);

                // Generate new Refresh Token
                var RefToken = _loginHelper.GenerateRefreshToken();

                // Update refresh token record in DB
                userRefDtl.Token = RefToken;
                userRefDtl.ExpiresOn = DateTime.Now.AddHours(Convert.ToInt32(_configuration["JWT:RefreshTokenExpiryHours"]));
                userRefDtl.CreatedOn = DateTime.Now;

                _sqlDBContext.RefreshToken_Tbl.Update(userRefDtl);
                await _sqlDBContext.SaveChangesAsync();

                // Return new Access & Refresh tokens
                return Ok(new
                {
                    AccessToken = NewAT,
                    refreshToken = RefToken
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex); // Return exception details (not recommended in prod)
            }
        }

        [HttpPost("[action]")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var token = _sqlDBContext.RefreshToken_Tbl.Where(a => a.UserId == Guid.Parse(userId));
            _sqlDBContext.RefreshToken_Tbl.RemoveRange(token);
            await _sqlDBContext.SaveChangesAsync();
            return Ok("Logout Successful");
        }
    }
}
