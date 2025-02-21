using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using BotBaoKhach.Repositories;
using System.ComponentModel.DataAnnotations;

namespace BotBaoKhach.Models
{
    public class SettingBaoKhachModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string BotName { get; set; }
        public string BotToken { get; set; }
        public string ChatId { get; set; }
        public string ReadSheetCredentialUrl { get; set; }
        public string ReadSheetId { get; set; }
        public string ReadSheetRange { get; set; }
        public string WriteSheetCredentialUrl { get; set; }
        public bool IsDividedByZone { get; set; }
        public bool IsCheckTeam { get; set; }
        public bool IsAmountVisible { get; set; }
        public string WriteSheetId { get; set; }
        public List<PermissionModel> Sites { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Status { get; set; }
        public List<ReadListModel> ReadList { get; set; }
        public List<WriteListModel> WriteList { get; set; }
    }
    public class SettingBaoKhachRequest
    {
        public string Name { get; set; }
        public string BotName { get; set; }
        public string BotToken { get; set; }
        public string ChatId { get; set; }
        public string ReadSheetCredentialUrl { get; set; }
        public string ReadSheetId { get; set; }
        public string ReadSheetRange { get; set; }
        public string WriteSheetCredentialUrl { get; set; }
        public string WriteSheetId { get; set; }
        public List<string> Sites { get; set; }
        [Required]
        public bool? IsDividedByZone { get; set; }
        [Required]
        public bool? IsCheckTeam { get; set; }
        [Required]
        public bool? IsAmountVisible { get; set; }
    }
}
