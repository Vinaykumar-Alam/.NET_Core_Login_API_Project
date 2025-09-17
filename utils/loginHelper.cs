using Flutter_Backed.Models;              // Import models (e.g., Login model)
using Microsoft.IdentityModel.Tokens;     // Provides classes for token validation and signing
using System.IdentityModel.Tokens.Jwt;    // Provides JWT creation and validation functionality
using System.Security.Claims;             // Used to define identity claims (like userId, role, email)
using System.Security.Cryptography;       // Used to generate secure refresh tokens
using System.Text;                        // For encoding/decoding strings

namespace Flutter_Backed.utils
{
    // This helper class manages JWT and Refresh Token generation/validation
    public class loginHelper
    {
        private readonly IConfiguration _configuration;  // To read JWT settings from appsettings.json

        // Constructor to inject configuration
        public loginHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Method to generate a secure Refresh Token
        public string GenerateRefreshToken()
        {
            var randomNum = new byte[32];   // Create a 32-byte array for random data (256-bit token)

            // Use cryptographic RNG to fill the byte array with secure random bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNum);    // Fill randomNum with random bytes
                return Convert.ToBase64String(randomNum); // Convert to Base64 string for storage/transmission
            }
        }

        // Method to extract ClaimsPrincipal (user info) from an existing Access Token
        public ClaimsPrincipal GetPrincipalFromAccessToken(string AccessToken)
        {
            // Setup token validation parameters
            var tokenvalidationParams = new TokenValidationParameters
            {
                ValidateLifetime = false,   // Do NOT validate token expiry (we just need claims)
                ValidateIssuer = false,     // Do NOT validate issuer
                ValidateIssuerSigningKey = true, // YES: validate the signature key (token authenticity)
                ValidateAudience = false,   // Do NOT validate audience
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"])) // Signing key from appsettings.json
            };

            var tokenHandler = new JwtSecurityTokenHandler(); // Handles reading & validating JWTs

            try
            {
                // Validate token using the parameters defined above
                var principal = tokenHandler.ValidateToken(AccessToken, tokenvalidationParams, out var newToken);

                // Ensure the token is a valid JWT and signed with HMAC-SHA256
                if (!(newToken is JwtSecurityToken jwt) ||
                    !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase))
                {
                    return null; // Invalid token
                }

                return principal; // Return the extracted user claims (userId, role, etc.)
            }
            catch (Exception)
            {
                // If token is invalid (tampered/expired/wrong key), return null
                return null;
            }
        }

        // Method to generate a new JWT (Access Token) for a user
        public string GenerateJwtToken(Login User)
        {
            // Define user claims (these go inside the JWT payload)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, User.Id.ToString()), // User ID (GUID)
                new Claim(ClaimTypes.Name, User.UserName),                 // Username
                new Claim(ClaimTypes.Email, User.Email),                   // Email
                new Claim(ClaimTypes.Role, User.Role)                 // User role (Admin/User/etc.
            };

            // Create a secret key using the value from appsettings.json (must be long enough!)
            var secretkey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));

            // Signing credentials: which algorithm/key to use
            var creds = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],   // Who issued the token (from config)
                audience: _configuration["JWT:Audience"], // Who can use the token (from config)
                claims: claims,                         // Add claims (user details)
                signingCredentials: creds,              // Sign with HMAC-SHA256
                expires: DateTime.Now.AddMinutes(
                    Convert.ToInt32(_configuration["JWT:AccessTokenExpiryMinutes"])) // Token expiry time
            );

            // Convert token object into a string and return
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
