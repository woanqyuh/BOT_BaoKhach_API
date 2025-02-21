
using BotBaoKhach.Models;
using BotBaoKhach.Repositories;
using MongoDB.Bson;
using BotBaoKhach.Dtos;
using AutoMapper;
using Telegram.Bot.Types;


namespace BotBaoKhach.Services
{
    public interface IUserService
    {

        Task<ApiResponse<List<UserModel>>> GetUsers();
        Task<ApiResponse<UserModel>> GetUserByIdAsync(ObjectId id);
        Task<ApiResponse<UserModel>> CreateUserAsync(RegisterModel model, ObjectId userId);   
        Task<ApiResponse<UserModel>> UpdateUserAsync(ObjectId id, UpdateUserModel model);  
        Task<ApiResponse<string>> DeleteUserAsync(ObjectId id);
        Task<ApiResponse<string>> ChangePasswordAsync(ObjectId userId, ChangePasswordModel model);
    }

    public class UserService : IUserService
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IAuthService _authService;
        public UserService(IUserRepository userRepository, IPermissionRepository permissionRepository, IAuthService authService, IMapper mapper)
        {
            _userRepository = userRepository;
            _authService = authService;
            _mapper = mapper;
            _permissionRepository = permissionRepository;
        }
        public async Task<ApiResponse<List<UserModel>>> GetUsers()
        {
            try
            {
                var userListDto = await _userRepository.GetAllAsync();
                var allPermissions = await _permissionRepository.GetAllAsync();
                var userModels = userListDto.Select(user => new UserModel
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Fullname = user.Fullname,
                    CreatedAt = user.CreatedAt,
                    Role = user.Role,
                    Permission = _mapper.Map<List<PermissionModel>>(allPermissions
                            .Where(p => user.Permission != null && user.Permission.Contains(p.Id.ToString()))
                            .ToList())
                }).ToList();


                return ApiResponse<List<UserModel>>.Success(userModels);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }
        public async Task<ApiResponse<UserModel>> GetUserByIdAsync(ObjectId id) 
        {
            try
            {
                var userDto = await _userRepository.GetByIdAsync(id);
                if (userDto == null)
                {
                    return ApiResponse<UserModel>.Fail("User not found", StatusCodeEnum.NotFound);
                }

                var allPermissions = await _permissionRepository.GetAllAsync();
                var permissionUser = allPermissions
                    .Where(p => userDto.Permission != null && userDto.Permission.Contains(p.Id.ToString()))
                    .ToList();
                var userModel = new UserModel
                {
                    Id = userDto.Id.ToString(),
                    Username = userDto.Username,
                    Fullname = userDto.Fullname,
                    Role = userDto.Role,
                    CreatedAt = userDto.CreatedAt,
                    Permission = _mapper.Map<List<PermissionModel>>(permissionUser)
                };

                return ApiResponse<UserModel>.Success(userModel);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }


        }
        public async Task<ApiResponse<UserModel>> CreateUserAsync(RegisterModel model, ObjectId userId)
        {
            try
            {
                model.Username = model.Username.Trim().ToLower();
                var existingUser = await _userRepository.GetByUsernameAsync(model.Username);
                if (existingUser != null)
                {
                    return ApiResponse<UserModel>.Fail($"User with username '{model.Username}' already exists.", StatusCodeEnum.Invalid);
                }
                var hashedPassword = _authService.HashPassword(model.Password);
                var user = new UserDto
                {
                    Id = ObjectId.GenerateNewId(),
                    Username = model.Username,
                    Password = hashedPassword,
                    Fullname = model.Fullname,
                    Role = model.Role,
                    Permission = model.Permission
                };
               
                var allPermissions = await _permissionRepository.GetAllAsync();
                var permissionUser = allPermissions
                    .Where(p => user.Permission != null && user.Permission.Contains(p.Id.ToString()))
                    .ToList();
                var userModel = new UserModel
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Fullname = user.Fullname,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    Permission = _mapper.Map<List<PermissionModel>>(permissionUser)
                };

                await _userRepository.AddAsync(user);
                return ApiResponse<UserModel>.Success(userModel, "User created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }



        public async Task<ApiResponse<UserModel>> UpdateUserAsync(ObjectId id, UpdateUserModel model)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return ApiResponse<UserModel>.Fail("User not found", StatusCodeEnum.NotFound);
                }

                user.Fullname = model.Fullname;
                user.Role = model.Role;
                user.Permission = model.Permission;

                var allPermissions = await _permissionRepository.GetAllAsync();
                var permissionUser = allPermissions
                    .Where(p => user.Permission != null && user.Permission.Contains(p.Id.ToString()))
                    .ToList();

                var userModel = new UserModel
                {
                    Id = user.Id.ToString(),
                    Username = user.Username,
                    Fullname = user.Fullname,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt,
                    Permission = _mapper.Map<List<PermissionModel>>(permissionUser)
                };

                await _userRepository.UpdateAsync(id, user);

                return ApiResponse<UserModel>.Success(userModel, "User updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }


        public async Task<ApiResponse<string>> DeleteUserAsync(ObjectId id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);

                if (user == null)
                {
                    return ApiResponse<string>.Fail("User not found", StatusCodeEnum.NotFound);
                }

                await _userRepository.DeleteAsync(id);
                return ApiResponse<string>.Success(null, "User deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
        public async Task<ApiResponse<string>> ChangePasswordAsync(ObjectId userId, ChangePasswordModel model)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);

                if (user == null)
                {
                    return ApiResponse<string>.Fail("User not found", StatusCodeEnum.NotFound);
                }

                // Kiểm tra mật khẩu cũ
                if (!_authService.VerifyPassword(model.OldPassword, user.Password))
                {
                    return ApiResponse<string>.Fail("Sai mật khẩu.", StatusCodeEnum.Invalid);
                }
                var hashedPassword = _authService.HashPassword(model.NewPassword);

                // Cập nhật mật khẩu mới
                user.Password = hashedPassword;
                await _userRepository.UpdateAsync(userId, user);

                return ApiResponse<string>.Success(null, "Password changed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
    }

}
