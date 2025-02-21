using MongoDB.Driver;
using BotBaoKhach.Dtos;
using MongoDB.Bson;

namespace BotBaoKhach.Repositories
{
    public interface IReadListRepository : IBaseRepository<ReadListDto>
    {
        Task InsertManyAsync(IEnumerable<ReadListDto> items);
        Task DeleteAllBySettingAsync(ObjectId settingId);

        Task<List<ReadListDto>> GetBySettingIdAsync(ObjectId settingId);
    }

    public class ReadListRepository : BaseRepository<ReadListDto>, IReadListRepository
    {
        public ReadListRepository(IMongoDatabase database) : base(database, "ReadList")
        {
        }

        public async Task InsertManyAsync(IEnumerable<ReadListDto> items)
        {
            if (items != null && items.Any())
            {
                await _collection.InsertManyAsync(items);
            }
        }

        public async Task DeleteAllBySettingAsync(ObjectId settingId)
        {
            var filter = Builders<ReadListDto>.Filter.Eq(x => x.SettingId, settingId);
            await _collection.DeleteManyAsync(filter);
        }
        public async Task<List<ReadListDto>> GetBySettingIdAsync(ObjectId settingId)
        {
            var filter = Builders<ReadListDto>.Filter.Eq(x => x.SettingId, settingId);
            return await _collection.Find(filter).ToListAsync();
        }
    }
}
