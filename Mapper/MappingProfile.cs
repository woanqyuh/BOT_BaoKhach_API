using AutoMapper;
using BotBaoKhach.Models;
using BotBaoKhach.Dtos;


public class MappingProfile : Profile
{
    public MappingProfile()
    {
        //CreateMap<UserDto, UserModel>()
        //    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        CreateMap<PermissionDto, PermissionModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));
        CreateMap<ReadListDto, ReadListModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.SettingId, opt => opt.MapFrom(src => src.SettingId.ToString()));
        CreateMap<WriteListDto, WriteListModel>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
        .ForMember(dest => dest.SettingId, opt => opt.MapFrom(src => src.SettingId.ToString()));
        CreateMap<SettingBaoKhachDto, SettingBaoKhachModel>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()));

    }
}