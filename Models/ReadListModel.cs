using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BotBaoKhach.Models
{
    public class ReadListModel
    {
        public string Id { get; set; }
        public string SettingId { get; set; }
        public string Data { get; set; }
        public DateTime LastModified { get; set; }
    }
}
