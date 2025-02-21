using MongoDB.Driver;
using BotBaoKhach.Dtos;
using MongoDB.Bson;

namespace BotBaoKhach.Repositories
{
    public interface IWriteListRepository : IBaseRepository<WriteListDto>
    {
        Task InsertManyAsync(IEnumerable<WriteListDto> items);
        Task DeleteAllBySettingAsync(ObjectId settingId);

        Task<List<WriteListDto>> GetBySettingIdAsync(ObjectId settingId);
    }

    public class WriteListRepository : BaseRepository<WriteListDto>, IWriteListRepository
    {
        public WriteListRepository(IMongoDatabase database) : base(database, "WriteList")
        {
        }

        public async Task InsertManyAsync(IEnumerable<WriteListDto> items)
        {
            if (items != null && items.Any())
            {
                await _collection.InsertManyAsync(items);
            }
        }

        public async Task DeleteAllBySettingAsync(ObjectId settingId)
        {
            var filter = Builders<WriteListDto>.Filter.Eq(x => x.SettingId, settingId);
            await _collection.DeleteManyAsync(filter);
        }
        public async Task<List<WriteListDto>> GetBySettingIdAsync(ObjectId settingId)
        {
            var filter = Builders<WriteListDto>.Filter.Eq(x => x.SettingId, settingId);
            return await _collection.Find(filter).SortByDescending(x => x.CreatedAt).ToListAsync();
        }
    }
}
