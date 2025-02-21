using BotBaoKhach.Dtos;
using BotBaoKhach.Models;
using BotBaoKhach.Repositories;
using MongoDB.Bson;
using Telegram.Bot;
using Newtonsoft.Json;
using MongoDB.Driver.Linq;
using AutoMapper;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net;
using System.Text.RegularExpressions;
using Quartz;
using Telegram.Bot.Types;
using System.Net.WebSockets;
using BotBaoKhach.Jobs;
using Microsoft.Extensions.Caching.Memory;


namespace BotBaoKhach.Services
{
    public interface ISettingBaoKhachService
    {

        Task<ApiResponse<string>> Start(ObjectId id);

        Task<ApiResponse<string>> Stop(ObjectId id);

        Task<ApiResponse<List<SettingBaoKhachModel>>> GetAll(ObjectId userId);
        Task<ApiResponse<string>> CreateAsync(SettingBaoKhachRequest model);
        Task<ApiResponse<string>> UpdateAsync(ObjectId id, SettingBaoKhachRequest model);
        Task<ApiResponse<string>> DeleteAsync(ObjectId id);

    }
    public class SettingBaoKhachService : ISettingBaoKhachService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IReadListRepository _readListRepository;
        private readonly IWriteListRepository _writeListRepository;
        private readonly ISettingBaoKhachRepository _settingBaoKhachRepository;
        private readonly IMapper _mapper;
        private readonly JobScheduler _scheduler;

        public SettingBaoKhachService(
            ISettingBaoKhachRepository settingBaoKhachRepository,
            IMapper mapper,
            IConfiguration configuration,
            IUserRepository userRepository,
            IPermissionRepository permissionRepository,
            JobScheduler scheduler,
            IReadListRepository readListRepository,
            IWriteListRepository writeListRepository
        )

        {
            _settingBaoKhachRepository = settingBaoKhachRepository;
            _mapper = mapper;
            _configuration = configuration;
            _permissionRepository = permissionRepository;
            _userRepository = userRepository;
            _scheduler = scheduler;
            _readListRepository = readListRepository;
            _writeListRepository = writeListRepository;
        }

        public async Task<ApiResponse<List<SettingBaoKhachModel>>> GetAll(ObjectId userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if(user.Permission == null)
                {
                    return ApiResponse<List<SettingBaoKhachModel>>.Fail($"Vui lòng cập nhật quyền cho tài khoản này", StatusCodeEnum.Invalid);
                }
                var tlListDto = await _settingBaoKhachRepository.GetAllAsync();
                var allPermissions = await _permissionRepository.GetAllAsync();

                var filtered = tlListDto
                    .Where(x => x.Sites != null && x.Sites.Any(site => user.Permission.Contains(site)))
                    .ToList();

                var settingModel = new List<SettingBaoKhachModel>();
                foreach (var s in filtered)
                {
  
                    var readListDtos = await _readListRepository.GetBySettingIdAsync(s.Id);
                    var readListModels = _mapper.Map<List<ReadListModel>>(readListDtos);

                    var writeListDtos = await _writeListRepository.GetBySettingIdAsync(s.Id);
                    var writeListModels = _mapper.Map<List<WriteListModel>>(writeListDtos);

                    var model = new SettingBaoKhachModel
                    {
                        Id = s.Id.ToString(),
                        Name = s.Name,
                        BotName = s.BotName,
                        BotToken = s.BotToken,
                        IsDividedByZone = s.IsDividedByZone,
                        IsCheckTeam = s.IsCheckTeam,
                        IsAmountVisible = s.IsAmountVisible,
                        ChatId = s.ChatId,
                        ReadSheetCredentialUrl = s.ReadSheetCredentialUrl,
                        ReadSheetId = s.ReadSheetId,
                        ReadSheetRange = s.ReadSheetRange,
                        ReadList = readListModels,
                        WriteList = writeListModels,
                        WriteSheetCredentialUrl = s.WriteSheetCredentialUrl,
                        WriteSheetId = s.WriteSheetId,
                        CreatedAt = s.CreatedAt,
                        Status = s.Status,
                        Sites = _mapper.Map<List<PermissionModel>>(allPermissions
                                .Where(p => s.Sites != null && s.Sites.Contains(p.Id.ToString()))
                                .ToList())
                    };

                    settingModel.Add(model);
                }
                return ApiResponse<List<SettingBaoKhachModel>>.Success(settingModel);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SettingBaoKhachModel>>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
        public async Task<ApiResponse<string>> CreateAsync(SettingBaoKhachRequest model)
        {
            try
            {
                var existingBotToken = await _settingBaoKhachRepository.GetByBotToken(model.BotToken);

                if (existingBotToken != null)
                {
                    return ApiResponse<string>.Fail("Bot token already exists", StatusCodeEnum.Conflict);
                }

                try
                {
                    var testBotClient = new TelegramBotClient(model.BotToken);
                    var me = await testBotClient.GetMe();
                }
                catch (Exception ex)
                {
                    return ApiResponse<string>.Fail("Failed to Create bot token: " + ex.Message, StatusCodeEnum.Invalid);
                }

                var newConfig = new SettingBaoKhachDto
                {
                    Id = ObjectId.GenerateNewId(),
                    Name = model.Name,
                    BotToken = model.BotToken,
                    BotName = model.BotName,
                    ChatId = model.ChatId,
                    Sites = model.Sites,
                    Status = (int)SettingBaoKhachEnum.Inactive,
                    ReadSheetCredentialUrl = model.ReadSheetCredentialUrl,
                    ReadSheetId = model.ReadSheetId,
                    ReadSheetRange = model.ReadSheetRange,
                    WriteSheetCredentialUrl = model.WriteSheetCredentialUrl,
                    WriteSheetId = model.WriteSheetId,
                    IsDividedByZone = model.IsDividedByZone ?? false,
                    IsCheckTeam = model.IsCheckTeam ?? false,
                    IsAmountVisible = model.IsAmountVisible ?? false,
                };
                await _settingBaoKhachRepository.AddAsync(newConfig);
                return ApiResponse<string>.Success("Initialization successful", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
        public async Task<ApiResponse<string>> UpdateAsync(ObjectId id, SettingBaoKhachRequest model)
        {
            try
            {
                var existingConfig = await _settingBaoKhachRepository.GetByIdAsync(id);

                if (existingConfig == null)
                {
                    return ApiResponse<string>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }

                var existingBotToken = await _settingBaoKhachRepository.GetByBotToken(model.BotToken);

                if (existingBotToken != null && existingConfig.BotToken != model.BotToken)
                {
                    return ApiResponse<string>.Fail("Bot token already exists", StatusCodeEnum.Conflict);
                }


                if (existingConfig.Status == (int)SettingBaoKhachEnum.InProgress)
                {
                    return ApiResponse<string>.Fail("Can not update. Setting is already in progress", StatusCodeEnum.Conflict);
                }

                try
                {
                    var testBotClient = new TelegramBotClient(model.BotToken);
                    var me = await testBotClient.GetMe();
                }
                catch (Exception ex)
                {
                    return ApiResponse<string>.Fail("Failed to Update bot token: " + ex.Message, StatusCodeEnum.Invalid);
                }



                existingConfig.ChatId = model.ChatId;
                existingConfig.BotName = model.BotName;
                existingConfig.BotToken = model.BotToken;
                existingConfig.Name = model.Name;
                existingConfig.ReadSheetCredentialUrl = model.ReadSheetCredentialUrl;
                existingConfig.ReadSheetId = model.ReadSheetId;
                existingConfig.ReadSheetRange = model.ReadSheetRange;
                existingConfig.WriteSheetCredentialUrl = model.WriteSheetCredentialUrl;
                existingConfig.WriteSheetId = model.WriteSheetId;
                existingConfig.Sites = model.Sites;
                existingConfig.IsDividedByZone = model.IsDividedByZone ?? false;
                existingConfig.IsCheckTeam = model.IsCheckTeam ?? false;
                existingConfig.IsAmountVisible = model.IsAmountVisible ?? false;


                //var cacheKey = $"Setting_{model.ChatId}";
                //_cache.Set(cacheKey, existingConfig, TimeSpan.FromMinutes(15));

                await _settingBaoKhachRepository.UpdateAsync(existingConfig.Id, existingConfig);

                return ApiResponse<string>.Success("Update successful", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
        public async Task<ApiResponse<string>> DeleteAsync(ObjectId id)
        {
            try
            {
                var tl = await _settingBaoKhachRepository.GetByIdAsync(id);

                if (tl == null)
                {
                    return ApiResponse<string>.Fail("Setting not found", StatusCodeEnum.NotFound);
                }
                if (tl.Status == (int)SettingBaoKhachEnum.InProgress)
                {
                    return ApiResponse<string>.Fail("Can not delete. Setting is already in progress", StatusCodeEnum.Conflict);
                }
                await _settingBaoKhachRepository.DeleteAsync(id);

                return ApiResponse<string>.Success("Setting deleted successfully", StatusCodeEnum.None);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }


        public async Task<ApiResponse<string>> Start(ObjectId id)
        {
            try
            {
                var existingConfig = await _settingBaoKhachRepository.GetByIdAsync(id);
                if (existingConfig == null)
                {
                    return ApiResponse<string>.Fail($"Setting not found", StatusCodeEnum.NotFound);
                }
                if (existingConfig.Status == (int)SettingBaoKhachEnum.InProgress)
                {
                    return ApiResponse<string>.Fail("Setting is already in progress", StatusCodeEnum.Conflict);
                }

                await _scheduler.ScheduleJobs(existingConfig);

                existingConfig.Status = (int)SettingBaoKhachEnum.InProgress;
                await _settingBaoKhachRepository.UpdateAsync(existingConfig.Id, existingConfig);

                return ApiResponse<string>.Success();
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
        public async Task<ApiResponse<string>> Stop(ObjectId id)
        {
            try
            {
                var existingConfig = await _settingBaoKhachRepository.GetByIdAsync(id);
                if (existingConfig == null)
                {
                    return ApiResponse<string>.Fail($"Setting not found", StatusCodeEnum.NotFound);
                }
                if (existingConfig.Status != (int)SettingBaoKhachEnum.InProgress)
                {
                    return ApiResponse<string>.Fail("Setting is already stop", StatusCodeEnum.Conflict);
                }
                await _scheduler.DeleteScheduledJobs(existingConfig);

                existingConfig.Status = (int)SettingBaoKhachEnum.Completed;
                await _settingBaoKhachRepository.UpdateAsync(existingConfig.Id, existingConfig);

                return ApiResponse<string>.Success();
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Fail($"An unexpected error occurred: {ex.Message}", StatusCodeEnum.InternalServerError);
            }
        }
    }
}
