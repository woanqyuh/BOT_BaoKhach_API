using BotBaoKhach.Dtos;
using BotBaoKhach.Repositories;
using BotBaoKhach.Services;
using Google.Apis.Sheets.v4.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using Quartz;


namespace BotBaoKhach.Jobs
{
    public class Job : IJob
    {
        private readonly ISettingBaoKhachRepository _repository;
        private readonly IGoogleSheetService _googleSheetService;
        private readonly IReadListRepository _readListRepository;
        public Job(ISettingBaoKhachRepository repository, IGoogleSheetService googleSheetService, IReadListRepository readListRepository)
        {
            _repository = repository;
            _googleSheetService = googleSheetService;
            _readListRepository = readListRepository;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"📌 Job started at {DateTime.UtcNow}");
            var jobDataMap = context.JobDetail.JobDataMap;
            var settingId = jobDataMap.Get("SettingId") as string;
            var setting = await _repository.GetByIdAsync(ObjectId.Parse(settingId));
            if (setting == null)
            {
                Console.WriteLine("❌ Setting is null. Exiting job.");
                return;
            }
            try
            {
                var sheetsService = await _googleSheetService.GetServiceAsync(setting.ReadSheetCredentialUrl);

                // Đọc dữ liệu từ Google Sheets
                string spreadsheetId = setting.ReadSheetId;
                string sheetRange = setting.ReadSheetRange;

                Console.WriteLine($"📋 Fetching data from range: {sheetRange}");

                var request = sheetsService.Spreadsheets.Values.Get(spreadsheetId, sheetRange);
                ValueRange response = await request.ExecuteAsync();

                if (response.Values == null || response.Values.Count == 0)
                {
                    Console.WriteLine("⚠️ No data found in the sheet.");
                    return;
                }


                var columns = GetColumnCountFromRange(sheetRange);
                Console.WriteLine($"📊 Detected {columns} columns.");

                var outputList = new List<string>();
                var readListItems = new List<ReadListDto>();
                foreach (var row in response.Values)
                {
                    List<string> rowData = new List<string>();
                    for (int i = 0; i < columns; i++)
                    {
                        rowData.Add(row.Count > i ? row[i].ToString() : "N/A");
                    }

                    var readList = new ReadListDto
                    {
                        Data = string.Join(" - ", rowData),
                        LastModified = DateTime.UtcNow,
                        SettingId = setting.Id
                    };

                    readListItems.Add(readList);
                }
                if (readListItems.Count > 0)
                {
                    await _readListRepository.DeleteAllBySettingAsync(setting.Id);
                    await _readListRepository.InsertManyAsync(readListItems);

                    Console.WriteLine("✅ All data inserted into MongoDB in one go.");
                }
                else
                {
                    Console.WriteLine("⚠️ No new data to insert. Skipping MongoDB update.");
                }

                //foreach (var row in response.Values)
                //{
                //    List<string> rowData = new List<string>();

                //    for (int i = 0; i < columns; i++)
                //    {
                //        string columnValue = row.Count > i ? row[i].ToString() : "N/A";
                //        rowData.Add(columnValue);
                //    }

                //    outputList.Add(string.Join(" - ", rowData));
                //}
                //Console.WriteLine("✅ Data from Google Sheets:");
                //foreach (var line in outputList)
                //{
                //    Console.WriteLine(line);
                //}
                //setting.ReadSavedList = outputList;
                //await _repository.UpdateAsync(setting.Id, setting);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reading Google Sheets: {ex.Message}");
            }
        }

        private int GetColumnCountFromRange(string sheetRange)
        {
            if (string.IsNullOrEmpty(sheetRange) || !sheetRange.Contains("!"))
                return 1;

            var rangePart = sheetRange.Split('!')[1];
            var columns = rangePart.Split(':');

            if (columns.Length == 1)
                return 1;

            // Lấy tên cột từ tọa độ (VD: "A1" -> "A", "AB12" -> "AB")
            string startColumn = GetColumnName(columns[0]);
            string endColumn = GetColumnName(columns[1]);

            // Chuyển cột từ chữ cái sang số
            int startColumnIndex = ExcelColumnToNumber(startColumn);
            int endColumnIndex = ExcelColumnToNumber(endColumn);

            return endColumnIndex - startColumnIndex + 1;
        }

        // Hàm lấy tên cột từ tọa độ (VD: "AB12" -> "AB")
        private string GetColumnName(string cell)
        {
            return new string(cell.TakeWhile(char.IsLetter).ToArray());
        }

        // Hàm chuyển cột từ chữ cái sang số (VD: "A" -> 1, "AB" -> 28)
        private int ExcelColumnToNumber(string column)
        {
            int sum = 0;
            foreach (char c in column)
            {
                sum = sum * 26 + (c - 'A' + 1);
            }
            return sum;
        }

    }
}
