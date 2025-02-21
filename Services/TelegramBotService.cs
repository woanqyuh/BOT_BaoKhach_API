using BotBaoKhach.Models;
using BotBaoKhach.Repositories;
using Telegram.Bot;
using Newtonsoft.Json;
using AutoMapper;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using BotBaoKhach.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using MongoDB.Bson;


namespace BotBaoKhach.Services
{
    public interface ITelegramBotService
    {
        Task<ApiResponse<string>> HandleWebHook(string botToken, dynamic data);


    }
    public class TelegramBotService : ITelegramBotService
    {
        private readonly IConfiguration _configuration;
        private readonly IGoogleSheetService _googleSheetService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ISettingBaoKhachRepository _settingBaoKhachRepository;
        private readonly IReadListRepository _readListRepository;
        private readonly IWriteListRepository _writeListRepository;

        public TelegramBotService(
            IUserRepository userRepository,
            IMapper mapper,
            IConfiguration configuration,
            ISettingBaoKhachRepository settingBaoKhachRepository,
            IGoogleSheetService googleSheetService,
            IReadListRepository readListRepository,
            IWriteListRepository writeListRepository
        )

        {
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
            _settingBaoKhachRepository = settingBaoKhachRepository;
            _googleSheetService = googleSheetService;
            _readListRepository = readListRepository;
            _writeListRepository = writeListRepository;
        }

        #region HandleWebHook old
        //public async Task<ApiResponse<string>> HandleWebHook(string botToken, dynamic data)
        //{
        //    try
        //    {
        //        var botClient = new TelegramBotClient(botToken);
        //        var webhookData = JsonConvert.DeserializeObject<WebhookDataDto>(data.ToString());
        //        var message = webhookData?.Message;
        //        if (message == null || string.IsNullOrWhiteSpace(message.Text))
        //            return ApiResponse<string>.Success("No valid message.", StatusCodeEnum.None);

        //        long chatId = message.Chat.Id;
        //        int messageId = message.MessageId;
        //        string messageText = message.Text.Trim();

        //        if (messageText.Equals("/getchatid", StringComparison.OrdinalIgnoreCase))
        //        {
        //            await botClient.SendMessage(chatId, $"Xin chào! ChatId của bạn là: {chatId}", replyParameters: messageId);
        //            return ApiResponse<string>.Success("ChatId sent.", StatusCodeEnum.None);
        //        }
        //        var settingMatch = await _settingBaoKhachRepository.GetByChatId(chatId.ToString());
        //        if(settingMatch?.Status != (int)SettingBaoKhachEnum.InProgress)
        //        {
        //            return ApiResponse<string>.Success("Setting not run. End issue", StatusCodeEnum.None);
        //        }

        //        if (settingMatch?.ReadSavedList == null || !settingMatch.ReadSavedList.Any())
        //        {
        //            await botClient.SendMessage(chatId, "⚠ Bạn chưa tạo thiết lập hoặc danh sách nhân viên không tồn tại.", replyParameters: messageId);
        //            return ApiResponse<string>.Success("No settings found.", StatusCodeEnum.None);
        //        }


        //        var syntax = "TÊN KHÁCH - TÊN NV - TỔ";

        //        string pattern = ConvertSyntaxToRegex(syntax);

        //        Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);

        //        var match = regex.Match(message.Text.Trim());
        //        if (!match.Success)
        //        {
        //            await botClient.SendMessage(chatId, "⚠️ Sai cú pháp! Vui lòng nhập đúng format: TÊN KHÁCH - TÊN NV - TỔ ( - SỐ TIỀN)", replyParameters: messageId);
        //            return ApiResponse<string>.Success("Invalid syntax.", StatusCodeEnum.None);
        //        }


        //        var (customer, employee, team, amount) = (
        //            match.Groups[1].Value.Trim(),
        //            match.Groups[2].Value.Trim(),
        //            match.Groups[3].Value.Trim(),
        //            match.Groups.Count > 4 ? match.Groups[4].Value.Trim() : " "
        //        );
        //        string teamOnlyLetters = Regex.Match(team, "[a-zA-Z]+").Value;

        //        var matchedEntry = settingMatch.ReadSavedList
        //            .FirstOrDefault(entry => entry.Contains(employee, StringComparison.OrdinalIgnoreCase));

        //        if (matchedEntry == null)
        //        {
        //            await botClient.SendMessage(
        //                chatId: chatId,
        //                text: $"⚠ Nhân viên '{employee}' không hợp lệ. Vui lòng kiểm tra lại danh sách!",
        //                replyParameters: messageId
        //            );
        //            return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
        //        }

        //        if (!matchedEntry.Contains(team, StringComparison.OrdinalIgnoreCase))
        //        {
        //            await botClient.SendMessage(
        //                chatId: chatId,
        //                text: $"⚠ Team '{team}' không hợp lệ cho nhân viên '{employee}'. Vui lòng kiểm tra lại danh sách!",
        //                replyParameters: messageId
        //            );
        //            return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
        //        }

        //        await _googleSheetService.InitializeAsync(settingMatch.WriteSheetCredentialUrl);
        //        var sheetsService = _googleSheetService.GetService();
        //        var sheetName = settingMatch.IsDividedByZone ? $"KHU {teamOnlyLetters} T{DateTime.Now.Month}.{DateTime.Now.Year}" : $"T{DateTime.Now.Month}/{DateTime.Now.Year}";
        //        var sheetRange = $"{sheetName}!A:E";

        //        var spreadsheet = await sheetsService.Spreadsheets.Get(settingMatch.WriteSheetId).ExecuteAsync();
        //        if (!spreadsheet.Sheets.Any(sheet => sheet.Properties.Title == sheetName))
        //            await CreateSheetAsync(sheetName, settingMatch.WriteSheetId);

        //        var existingEntries = (await sheetsService.Spreadsheets.Values.Get(settingMatch.WriteSheetId, sheetRange).ExecuteAsync()).Values;

        //        if (existingEntries != null && 
        //            existingEntries.Any(row => row.Count > 0 && row[0] != null && 
        //            row[0].ToString().Trim().Equals(customer.Trim(), StringComparison.OrdinalIgnoreCase)))
        //        {
        //            await botClient.SendMessage(chatId, $"⚠ Tên khách {customer} đã tồn tại!", replyParameters: messageId);
        //            return ApiResponse<string>.Success("Customer already exists.", StatusCodeEnum.None);
        //        }

        //        var row = new List<object> { customer, employee, amount, team ,DateTimeOffset.FromUnixTimeSeconds(message.Date).ToLocalTime().ToString("dd/MM/yyyy HH:mm") };
        //        var appendRequest = sheetsService.Spreadsheets.Values.Append(new ValueRange { Values = new List<IList<object>> { row } }, settingMatch.WriteSheetId, sheetRange);
        //        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
        //        await appendRequest.ExecuteAsync();
        //        await botClient.SendMessage(chatId, "✅ OKVIP 👌", replyParameters: messageId);

        //        settingMatch.WriteSavedList ??= new List<string>();
        //        settingMatch.WriteSavedList.Add(messageText);
        //        await _settingBaoKhachRepository.UpdateAsync(settingMatch.Id, settingMatch);
        //        return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);

        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
        //    }
        //}
        #endregion
            public async Task<ApiResponse<string>> HandleWebHook(string botToken, dynamic data)
            {
                try
                {
                var stopwatch = new Stopwatch();
                stopwatch.Start();



                var botClient = new TelegramBotClient(botToken);
                    var message = ParseWebhookData(data);
                    if (message == null)
                        return ApiResponse<string>.Success("No valid message.", StatusCodeEnum.None);

                    long chatId = message.Chat.Id;
                    int messageId = message.MessageId;
                    string messageText = message.Text.Trim();

                    if (messageText.Equals("/getchatid", StringComparison.OrdinalIgnoreCase))
                    {
                        await SendTelegramMessage(botClient, chatId, $"Xin chào! ChatId của bạn là: {chatId}", messageId);
                        return ApiResponse<string>.Success("ChatId sent.", StatusCodeEnum.None);
                    }
                    var settingMatch = await _settingBaoKhachRepository.GetByChatId(chatId.ToString());
                    if (settingMatch == null )
                    {
                        return ApiResponse<string>.Success("Setting not found.", StatusCodeEnum.None);
                    }
                    if(settingMatch.Status != (int)SettingBaoKhachEnum.InProgress)
                    {
                        return ApiResponse<string>.Success("Setting not run.", StatusCodeEnum.None);
                    }

                    if (!TryParseMessage(messageText, settingMatch, out var customer, out var employee, out var team, out var amount))
                    {
                        string expectedFormat = settingMatch.IsAmountVisible
                            ? "TÊN KHÁCH - TÊN NV - TỔ - SỐ TIỀN"
                            : "TÊN KHÁCH - TÊN NV - TỔ";

                        await SendTelegramMessage(botClient, chatId, $"⚠️ Sai cú pháp! Format: {expectedFormat}", messageId);
                        return ApiResponse<string>.Success("Invalid syntax.", StatusCodeEnum.None);
                    }

                    if (!await ValidateEmployee(settingMatch, employee, team))
                        {
                        string warningMessage = settingMatch.IsCheckTeam
                            ? $"⚠ Nhân viên '{employee}' không hợp lệ hoặc không thuộc team '{team}'."
                            : $"⚠ Nhân viên '{employee}' không hợp lệ.";

                        await SendTelegramMessage(botClient, chatId, warningMessage, messageId);
                        return ApiResponse<string>.Success("Invalid employee.", StatusCodeEnum.None);
                    }

                    var sheetsService = await _googleSheetService.GetServiceAsync(settingMatch.WriteSheetCredentialUrl);
                    if (await CustomerExistsInSheet(settingMatch, customer, team, sheetsService))
                    {
                        await SendTelegramMessage(botClient, chatId, $"⚠ Tên khách {customer} đã tồn tại!", messageId);
                        return ApiResponse<string>.Success("Customer already exists.", StatusCodeEnum.None);
                    }

                    await SendTelegramMessage(botClient, chatId, "✅ OKVIP 👌", messageId);
                    _ = Task.Run(() => WriteToGoogleSheet(sheetsService, settingMatch, customer, employee, amount, team, message.Date));

                    var writeDto = new WriteListDto
                    {
                        SettingId = settingMatch.Id,
                        Data = messageText,
                    };
                    await _writeListRepository.AddAsync(writeDto);

                    stopwatch.Stop();
                    Console.WriteLine($"Thời gian thực thi: {stopwatch.ElapsedMilliseconds} ms");
                    return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return ApiResponse<string>.Success("Webhook processed successfully.", StatusCodeEnum.None);
                }
            }

        private MessageWebhook? ParseWebhookData(dynamic data)
        {
            var webhookData = JsonConvert.DeserializeObject<WebhookDataDto>(data.ToString());
            return webhookData?.Message;
        }
        private string ConvertSyntaxToRegex(string syntax)
        {
            if (string.IsNullOrWhiteSpace(syntax)) return string.Empty;
            string[] parts = syntax.Split('-').Select(p => p.Trim()).ToArray();

            string pattern = @"^\s*";
            pattern += string.Join(@"\s*-\s*", parts.Select(_ => @"([^\-]+)"));

            // Thêm phần số tiền (tùy chọn)
            //pattern += @"(?:\s*-\s*(\d+))?\s*$";
            //return pattern;

            return pattern + @"\s*$";
        }
        private async Task CreateSheetAsync(string sheetName,string sheetId,string sheetRange,SettingBaoKhachDto setting,SheetsService sheetsService)
        {
            var addSheetRequest = new Request
            {
                AddSheet = new AddSheetRequest
                {
                    Properties = new SheetProperties { Title = sheetName }
                }
            };

            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
            {
                Requests = new List<Request> { addSheetRequest }
            };
            var titleRow = setting.IsAmountVisible
                ? new List<object> { "TÊN KHÁCH", "TÊN NHÂN VIÊN", "SỐ TIỀN", "TỔ", "THỜI GIAN" }
                : new List<object> { "TÊN KHÁCH", "TÊN NHÂN VIÊN", "TỔ", "THỜI GIAN" };

            await sheetsService.Spreadsheets.BatchUpdate(batchUpdateRequest, sheetId).ExecuteAsync();
            await AppendRowToSheet(sheetsService, setting.WriteSheetId, sheetRange, titleRow);
        }

        private async Task SendTelegramMessage(TelegramBotClient botClient, long chatId, string text, int replyToMessageId)
        {
            await botClient.SendMessage(chatId, text, replyParameters: replyToMessageId);
        }

        private bool TryParseMessage(string message, SettingBaoKhachDto setting, out string customer, out string employee, out string team, out string amount)
        {
            string syntax = setting.IsAmountVisible
                ? "TÊN KHÁCH - TÊN NV - TỔ - SỐ TIỀN"
                : "TÊN KHÁCH - TÊN NV - TỔ";
            string pattern = ConvertSyntaxToRegex(syntax);
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            var match = regex.Match(message.Trim());

            if (match.Success)
            {
                customer = match.Groups[1].Value.Trim();
                employee = match.Groups[2].Value.Trim();
                team = match.Groups[3].Value.Trim();
                amount = match.Groups.Count > 4 ? match.Groups[4].Value.Trim() : " ";
                return true;
            }

            customer = employee = team = amount = string.Empty;
            return false;
        }
        private async Task<bool> ValidateEmployee(SettingBaoKhachDto setting, string employee, string team)
        {
            var readList = await _readListRepository.GetBySettingIdAsync(setting.Id);
            var readDataList = readList.Select(x => x.Data).ToList();
            var matchedEntry = readDataList
                .FirstOrDefault(entry => entry.Contains(employee, StringComparison.OrdinalIgnoreCase));
            if (matchedEntry == null)
                return false;
            if (!setting.IsCheckTeam)
                return true;
            return matchedEntry.Contains(team, StringComparison.OrdinalIgnoreCase);
        }


        private async Task<bool> CustomerExistsInSheet(SettingBaoKhachDto setting, string customer, string team, SheetsService sheetsService)
        {
            var sheetName = GetSheetName(setting, team);
            var sheetRange = setting.IsAmountVisible ? $"{sheetName}!A:E" : $"{sheetName}!A:D";
            var spreadsheet = await sheetsService.Spreadsheets.Get(setting.WriteSheetId).ExecuteAsync();
            bool sheetExists = spreadsheet.Sheets.Any(sheet => sheet.Properties.Title == sheetName);

            if (!sheetExists)
            {
                await CreateSheetAsync(sheetName, setting.WriteSheetId, sheetRange, setting, sheetsService);
                return false; 
            }
            var existingEntries = (await sheetsService.Spreadsheets.Values.Get(setting.WriteSheetId, sheetRange).ExecuteAsync()).Values;
            if (existingEntries == null || !existingEntries.Any()) return false;
            return existingEntries.Any(row => row.Count > 0 && (row[0]?.ToString().Trim().Equals(customer, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        private string GetSheetName(SettingBaoKhachDto setting, string team)
        {
            string teamOnlyLetters = Regex.Match(team, "[a-zA-Z]+").Value;
            return setting.IsDividedByZone
                ? $"KHU {teamOnlyLetters} T{DateTime.Now.Month}.{DateTime.Now.Year}"
                : $"T{DateTime.Now.Month}.{DateTime.Now.Year}";
        }

        private async Task WriteToGoogleSheet(SheetsService sheetsService,SettingBaoKhachDto setting, string customer, string employee, string amount, string team, int messageTimestamp)
        {
            var sheetName = GetSheetName(setting, team);
            var sheetRange = setting.IsAmountVisible ? $"{sheetName}!A:E" : $"{sheetName}!A:D";
            var row = setting.IsAmountVisible
                ? new List<object> { customer, employee, amount, team, DateTimeOffset.FromUnixTimeSeconds(messageTimestamp).ToLocalTime().ToString("dd/MM/yyyy HH:mm") }
                : new List<object> { customer, employee, team, DateTimeOffset.FromUnixTimeSeconds(messageTimestamp).ToLocalTime().ToString("dd/MM/yyyy HH:mm") };

            var appendRequest = sheetsService.Spreadsheets.Values.Append(new ValueRange { Values = new List<IList<object>> { row } }, setting.WriteSheetId, sheetRange);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            await appendRequest.ExecuteAsync();
        }
        private async Task AppendRowToSheet(SheetsService sheetsService, string sheetId, string range, List<object> rowData)
        {
            var appendRequest = sheetsService.Spreadsheets.Values.Append(
                new ValueRange { Values = new List<IList<object>> { rowData } },
                sheetId,
                range
            );

            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            await appendRequest.ExecuteAsync();
        }
    }
}

