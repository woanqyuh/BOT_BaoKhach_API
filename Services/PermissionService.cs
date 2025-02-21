
using BotBaoKhach.Models;
using BotBaoKhach.Repositories;
using MongoDB.Bson;
using BotBaoKhach.Dtos;
using AutoMapper;


namespace BotBaoKhach.Services
{
    public interface IPermissionService
    {

        Task<ApiResponse<List<PermissionModel>>> GetAll();
        Task<ApiResponse<PermissionModel>> GetByIdAsync(ObjectId id);
        Task<ApiResponse<PermissionModel>> CreateAsync(PermissionRequest model);
        Task<ApiResponse<PermissionModel>> UpdateAsync(ObjectId id, PermissionRequest model);
        Task<ApiResponse<string>> DeleteAsync(ObjectId id);
    }

    public class PermissionService : IPermissionService
    {
        private readonly IMapper _mapper;
        private readonly IPermissionRepository _repository;
        private readonly IAuthService _authService;


        public PermissionService(IPermissionRepository repository, IAuthService authService, IMapper mapper)
        {
            _repository = repository;
            _authService = authService;
            _mapper = mapper;
        }
        public async Task<ApiResponse<List<PermissionModel>>> GetAll()
        {
            try
            {
                var permissionListDto = await _repository.GetAllAsync();

                return ApiResponse<List<PermissionModel>>.Success(_mapper.Map<List<PermissionModel>>(permissionListDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PermissionModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }

        }
        public async Task<ApiResponse<PermissionModel>> GetByIdAsync(ObjectId id)
        {
            try
            {
                var permissionDto = await _repository.GetByIdAsync(id);
                if (permissionDto == null)
                {
                    return ApiResponse<PermissionModel>.Fail("Permission not found", StatusCodeEnum.NotFound);
                }

                return ApiResponse<PermissionModel>.Success(_mapper.Map<PermissionModel>(permissionDto));
            }
            catch (Exception ex)
            {
                return ApiResponse<PermissionModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }


        }
        public async Task<ApiResponse<PermissionModel>> CreateAsync(PermissionRequest model)
        {
            try
            {
                var permission = new PermissionDto
                {
                    Id = ObjectId.GenerateNewId(),
                    Name = model.Name,

                };
                await _repository.AddAsync(permission);

                var permissionModel = _mapper.Map<PermissionModel>(permission);
                return ApiResponse<PermissionModel>.Success(permissionModel, "permission created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PermissionModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }



        public async Task<ApiResponse<PermissionModel>> UpdateAsync(ObjectId id, PermissionRequest model)
        {
            try
            {
                var permission = await _repository.GetByIdAsync(id);

                if (permission == null)
                {
                    return ApiResponse<PermissionModel>.Fail("permission not found", StatusCodeEnum.NotFound);
                }

                permission.Name = model.Name;

                await _repository.UpdateAsync(id, permission);

                var permissionModel = _mapper.Map<PermissionModel>(permission);
                return ApiResponse<PermissionModel>.Success(permissionModel, "permission updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<PermissionModel>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }


        public async Task<ApiResponse<string>> DeleteAsync(ObjectId id)
        {
            try
            {
                var permission = await _repository.GetByIdAsync(id);

                if (permission == null)
                {
                    return ApiResponse<string>.Fail("permission not found", StatusCodeEnum.NotFound);
                }

                await _repository.DeleteAsync(id);
                return ApiResponse<string>.Success(null, "permission deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }

    }

}
