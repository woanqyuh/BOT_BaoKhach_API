using MongoDB.Driver;
using BotBaoKhach.Dtos;

namespace BotBaoKhach.Repositories
{
    public interface ISettingBaoKhachRepository : IBaseRepository<SettingBaoKhachDto>
    {
        Task<List<SettingBaoKhachDto>> GetActiveRecordsAsync();
        Task<SettingBaoKhachDto> GetByChatId(string chatId);
        Task<SettingBaoKhachDto> GetByBotToken(string botToken);
    }

    public class SettingBaoKhachRepository : BaseRepository<SettingBaoKhachDto>, ISettingBaoKhachRepository
    {
        public SettingBaoKhachRepository(IMongoDatabase database) : base(database, "SettingBaoKhach")
        {
        }
        public async Task<List<SettingBaoKhachDto>> GetActiveRecordsAsync()
        {
            var filter = Builders<SettingBaoKhachDto>.Filter.And(
                Builders<SettingBaoKhachDto>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SettingBaoKhachDto>.Filter.Eq(x => x.Status, (int)SettingBaoKhachEnum.InProgress)
            );
            return await _collection.Find(filter).ToListAsync();
        }
        public async  Task<SettingBaoKhachDto> GetByChatId(string chatId)
        {
            var filter = Builders<SettingBaoKhachDto>.Filter.And(
                Builders<SettingBaoKhachDto>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SettingBaoKhachDto>.Filter.Eq(x => x.ChatId, chatId),
                Builders<SettingBaoKhachDto>.Filter.Eq(x => x.Status, (int)SettingBaoKhachEnum.InProgress)
            );
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<SettingBaoKhachDto> GetByBotToken(string botToken)
        {
            var filter = Builders<SettingBaoKhachDto>.Filter.And(
                Builders<SettingBaoKhachDto>.Filter.Eq(x => x.IsDeleted, false),
                Builders<SettingBaoKhachDto>.Filter.Eq(x => x.BotToken, botToken)
            );
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }
    }
}
