using BotBaoKhach.Dtos;
using MongoDB.Driver;


namespace BotBaoKhach.Repositories
{
    public interface IPermissionRepository : IBaseRepository<PermissionDto>
    {

    }

    public class PermissionRepository : BaseRepository<PermissionDto>, IPermissionRepository
    {
        public PermissionRepository(IMongoDatabase database) : base(database, "Permissions")
        {
        }
    }
}
