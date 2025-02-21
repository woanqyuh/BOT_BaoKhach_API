using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using BotBaoKhach.Models;
using MongoDB.Bson;
using BotBaoKhach.Repositories;
using Telegram.Bot;
using Microsoft.Extensions.Caching.Memory;
using BotBaoKhach.Dtos;
using Google.Apis.Sheets.v4.Data;
using Telegram.Bot.Types;
using AutoMapper;

namespace BotBaoKhach.Services
{
    public interface IAuthService
    {

        Task<ApiResponse<AuthResponse>> RefreshTokenAsync(TokenRequest model);
        Task<ApiResponse<AuthResponse>> LoginAsync(LoginModel model);
        string HashPassword(string password);
        bool VerifyPassword(string enteredPassword, string storedHash);
    }
    public class AuthService : IAuthService

    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IMapper _mapper;
        public AuthService(
            IMemoryCache cache,
            IConfiguration configuration,
            IUserRepository userRepository,
            IPermissionRepository permissionRepository,
            IMapper mapper
            )
        {
            _userRepository = userRepository;
            _permissionRepository = permissionRepository;
            _configuration = configuration;
            _mapper = mapper;
        }



        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginModel model)
        {
            try
            {
                var response = new ApiResponse<AuthResponse>();

                model.Username = model.Username.Trim().ToLower();
                var existingUser = await _userRepository.GetByUsernameAsync(model.Username);
                if (existingUser == null)
                {
                    return ApiResponse<AuthResponse>.Fail($"Người dùng với tên đăng nhập '{model.Username}' không tồn tại.", StatusCodeEnum.NotFound);
                }
                var verifyPassword = VerifyPassword(model.Password, existingUser.Password);
                if (!verifyPassword)
                {
                    return ApiResponse<AuthResponse>.Fail("Sai mật khẩu.", StatusCodeEnum.Invalid);
                }
                var allPermissions = await _permissionRepository.GetAllAsync();
                response.Data = new AuthResponse
                {
                    AccessToken = GenerateAccessToken(existingUser),
                    RefreshToken = GenerateRefreshToken(existingUser),
                    User = new UserModel
                    {
                        Id = existingUser.Id.ToString(),
                        Username = existingUser.Username,
                        Fullname = existingUser.Fullname,
                        CreatedAt = existingUser.CreatedAt,
                        Role = existingUser.Role,
                        Permission =_mapper.Map<List<PermissionModel>>(allPermissions
                            .Where(p => existingUser.Permission != null && existingUser.Permission.Contains(p.Id.ToString()))
                            .ToList())
                    }
                };

                return ApiResponse<AuthResponse>.Success(response.Data, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
        public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(TokenRequest model)
        {
            try
            {
                ClaimsPrincipal principal = GetPrincipalFromExpiredToken(model.RefreshToken);

                if (principal == null)
                {
                    return ApiResponse<AuthResponse>.Fail("Token không hợp lệ.", StatusCodeEnum.Unauthorized);
                }
                var userIdString = principal.Identity.Name;
                if (!ObjectId.TryParse(userIdString, out ObjectId userId))
                {
                    return ApiResponse<AuthResponse>.Fail("Token không hợp lệ.", StatusCodeEnum.Unauthorized);
                }

                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return ApiResponse<AuthResponse>.Fail("Người dùng không tồn tại.", StatusCodeEnum.NotFound);
                }

                var accessToken = GenerateAccessToken(user);
                var refreshToken = GenerateRefreshToken(user);

                var authResponse = new AuthResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    User = new UserModel
                    {
                        Username = user.Username,
                        Fullname = user.Fullname
                    }
                };

                return ApiResponse<AuthResponse>.Success(authResponse, "Thành công");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.Fail($"Đã xảy ra lỗi: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }

        public string GenerateAccessToken(UserDto user)
        {
            var accessTokenSecret = _configuration.GetValue<string>("JWT:AccessTokenSecret");
            var accessTokenExpirationMinutes = _configuration.GetValue<int>("JWT:AccessTokenExpirationMinutes");
            return GenerateToken(user, Convert.ToInt32(accessTokenExpirationMinutes), accessTokenSecret);
        }
        public string GenerateRefreshToken(UserDto user)
        {

            var refreshTokenSecret = _configuration.GetValue<string>("JWT:RefreshTokenSecret");
            var refreshTokenExpirationDays = _configuration.GetValue<int>("JWT:RefreshTokenExpirationDays");
            return GenerateToken(user, Convert.ToInt32(refreshTokenExpirationDays * 24 * 60), refreshTokenSecret);
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var refreshTokenSecret = _configuration.GetValue<string>("JWT:RefreshTokenSecret");
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(refreshTokenSecret)),
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {

                throw new Exception("Token has expired.");
            }
            catch (SecurityTokenInvalidSignatureException)
            {

                throw new Exception("Token has an invalid signature.");
            }
            catch (Exception ex)
            {

                throw new Exception($"Token validation failed: {ex.Message}");
            }
        }
        public string GenerateToken(UserDto user, int expiresIn, string secret)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, ObjectId.GenerateNewId().ToString()),
            new Claim(ClaimTypes.Name, user.Id.ToString()), 
            new Claim(ClaimTypes.Role, ((int)user.Role).ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);



            var token = new JwtSecurityToken(
                issuer: "",
                audience: "your-app",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresIn),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public string HashPassword(string password)
        {

            byte[] salt = new byte[128 / 8];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt); // Lấy một salt ngẫu nhiên
            }


            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));


            return Convert.ToBase64String(salt) + "." + hashedPassword;
        }
        public bool VerifyPassword(string enteredPassword, string storedHash)
        {

            var parts = storedHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            byte[] salt = Convert.FromBase64String(parts[0]);
            string storedPasswordHash = parts[1];


            string enteredPasswordHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: enteredPassword,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA512,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            return storedPasswordHash == enteredPasswordHash;
        }


    }
}


