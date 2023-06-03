using DevStore.API.Core.Users.Extensions;
using DevStore.API.Core.Users.Interfaces;
using DevStore.Identity.API.Data;
using DevStore.Identity.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DevStore.Identity.API.Services
{
    public class AuthenticationService
    {
        public readonly UserManager<User> UserManager;
        public readonly SignInManager<User> SignInManager;
        private readonly IAspNetUser _aspNetUser;
        private readonly IConfiguration _configuration;
        public AuthenticationService(UserManager<User> userManager, 
            SignInManager<User> signInManager,
            IAspNetUser aspNetUser,
            IConfiguration configuration)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            _aspNetUser = aspNetUser;
            _configuration = configuration;
        }

        public async Task<UserLoginResponse> GetJwt(string email)
        {
            var user = await UserManager.FindByEmailAsync(email);
            var claims = await UserManager.GetClaimsAsync(user);

            var identityClaims = await GetUserClaims(claims, user);

            var token = CreateToken(identityClaims);
            var refreshToken = GenerateRefreshToken();

            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"],
                out int refreshTokenValidityInMinutes);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);

            await UserManager.UpdateAsync(user);

            return GetResponseToken(token, refreshToken);
        }
        public async Task<UserLoginResponse> RefreshToken(string refreshToken)
        {
            var accessToken = _aspNetUser.GetUserToken();
            
            var principal = GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                return null;

            string username = principal.GetUserEmail();
            var user = await UserManager.FindByNameAsync(username);

            if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
                return null;

            var claims = await UserManager.GetClaimsAsync(user);

            var identityClaims = await GetUserClaims(claims, user);
            var token = CreateToken(identityClaims);
            var newRefreshToken = GenerateRefreshToken();

            _ = int.TryParse(_configuration["JWT:RefreshTokenValidityInMinutes"],
                out int refreshTokenValidityInMinutes);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);
            await UserManager.UpdateAsync(user);

            return GetResponseToken(token, refreshToken);
        }
        private async Task<ClaimsIdentity> GetUserClaims(ICollection<Claim> claims, User user)
        {
            var userRoles = await UserManager.GetRolesAsync(user);

            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, user.Id));
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Nbf, ToUnixEpochDate(DateTime.UtcNow).ToString()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64));

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim("role", userRole));
            }

            var identityClaims = new ClaimsIdentity();
            identityClaims.AddClaims(claims);

            return identityClaims;
        }
        private static long ToUnixEpochDate(DateTime date) => (long)Math.Round((date.ToUniversalTime() - new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero)).TotalSeconds);
        private UserLoginResponse GetResponseToken(JwtSecurityToken token, string refreshToken)
        {
            return new UserLoginResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                ExpiresIn = token.ValidTo
            };
        }
        private JwtSecurityToken CreateToken(ClaimsIdentity identityClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"]));
            _ = int.TryParse(_configuration["JWT:TokenValidityInMinutes"], out int tokenValidityInMinutes);
            var currentIssuer = $"{_aspNetUser.GetHttpContext().Request.Scheme}://{_aspNetUser.GetHttpContext().Request.Host}";

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(new SecurityTokenDescriptor
            {
                Issuer = currentIssuer,
                Audience = _configuration["JWT:ValidAudience"],
                Expires = DateTime.UtcNow.AddMinutes(tokenValidityInMinutes),
                Subject = identityClaims,
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256Signature)
            });

            return token;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public async Task RevokeAccess()
        {
            var user = await UserManager.FindByNameAsync(_aspNetUser.GetUserEmail());

            await SignInManager.SignOutAsync();

            user.RefreshToken = null;
            await UserManager.UpdateAsync(user);
        }
        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: securityToken.Issuer,
                expires: securityToken.ValidTo,
                claims: principal.Claims,
                signingCredentials: new SigningCredentials(securityToken.SigningKey, SecurityAlgorithms.HmacSha256Signature)
                );

            if (!jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256Signature, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }
}
