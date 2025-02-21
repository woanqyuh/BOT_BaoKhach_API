using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BotBaoKhach.Models;
using BotBaoKhach.Dtos;
using BotBaoKhach.Repositories;
using BotBaoKhach.Services;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text;
using BotBaoKhach.Jobs;
using Quartz.Spi;
using Quartz;
using Quartz.Impl;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "wwwroot")
});

// ✅ Đăng ký SignalR
builder.Services.AddSignalR();

// ✅ Cấu hình MongoDB
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDB"));

builder.Services.AddSingleton<IMongoClient>(s =>
{
    var mongoDbSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>();
    return new MongoClient(mongoDbSettings.ConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(s =>
{
    var mongoClient = s.GetRequiredService<IMongoClient>();
    var mongoDbSettings = builder.Configuration.GetSection("MongoDB").Get<MongoDbSettings>();
    return mongoClient.GetDatabase(mongoDbSettings.DatabaseName);
});

// ✅ Đăng ký IMemoryCache trước khi sử dụng
builder.Services.AddMemoryCache();

// ✅ Cấu hình JWT
var jwtSettings = builder.Configuration.GetSection("JWT");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["AccessTokenSecret"])),
            ClockSkew = TimeSpan.Zero
        };
    });

// ✅ Cấu hình Quartz
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();
});

builder.Services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

builder.Services.AddSingleton(provider =>
{
    var schedulerFactory = provider.GetRequiredService<ISchedulerFactory>();
    var scheduler = schedulerFactory.GetScheduler().Result;
    scheduler.JobFactory = provider.GetRequiredService<IJobFactory>();
    scheduler.Start().Wait();
    return scheduler;
});

builder.Services.AddSingleton<IJobFactory, ScopedJobFactory>();
builder.Services.AddScoped<JobScheduler>();
builder.Services.AddTransient<Job>();
builder.Services.AddScoped<SettingBaoKhachScheduler>();
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// ✅ Đăng ký các Service và Repository
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IGoogleSheetService, GoogleSheetService>();
builder.Services.AddScoped<ISettingBaoKhachService, SettingBaoKhachService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<ISettingBaoKhachRepository, SettingBaoKhachRepository>();
builder.Services.AddScoped<IReadListRepository, ReadListRepository>();
builder.Services.AddScoped<IWriteListRepository, WriteListRepository>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

// ✅ Cấu hình Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token JWT vào dưới dạng: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ✅ Cấu hình FormOptions (tăng giới hạn file upload)
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});

builder.Services.AddControllers();

var app = builder.Build();
var adminAccountSettings = builder.Configuration.GetSection("AdminAccount").Get<AdminAccountSettings>();

// ✅ Cấu hình Swagger UI
app.UseSwagger();
app.UseSwaggerUI();

// ✅ Cấu hình CORS
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

// ✅ Khởi tạo Scheduler và tạo tài khoản Admin nếu chưa có
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Lấy scheduler và khởi động nếu cần
    var scheduler = services.GetRequiredService<IScheduler>();
    if (!scheduler.IsStarted)
    {
        await scheduler.Start();
    }

    var tlScheduler = services.GetRequiredService<SettingBaoKhachScheduler>();
    await tlScheduler.Start();

    // Kiểm tra và tạo admin nếu chưa có
    var userRepository = services.GetRequiredService<IUserRepository>();
    var authService = services.GetRequiredService<IAuthService>();

    var existingAdmin = await userRepository.GetByUsernameAsync(adminAccountSettings.Username);

    if (existingAdmin == null)
    {
        var hashedPassword = authService.HashPassword(adminAccountSettings.Password);

        var adminUser = new UserDto
        {
            Id = ObjectId.GenerateNewId(),
            Username = adminAccountSettings.Username,
            Password = hashedPassword,
            Fullname = adminAccountSettings.Fullname,
            Role = UserRole.Admin,
            Permission = []
        };
        await userRepository.AddAsync(adminUser);
    }

    // ✅ Cấu hình Telegram Bot (nếu cần kích hoạt webhook)
    //var botConfigurationRepository = services.GetRequiredService<IBotConfigurationRepository>();
    //var botConfig = await botConfigurationRepository.GetSingleAsync();

    //if (botConfig != null && !string.IsNullOrWhiteSpace(botConfig.KeyValue))
    //{
    //    var botClient = new TelegramBotClient(botConfig.KeyValue);

    //    try
    //    {
    //        var botInfo = await botClient.GetMe();
    //        Console.WriteLine($"Bot Connected: {botInfo.FirstName}");

    //        await botClient.DeleteWebhook();
    //        await botClient.GetUpdates(offset: -1);
    //        await botClient.SetWebhook(
    //            url: adminAccountSettings.Webhook,
    //            allowedUpdates: new[] { UpdateType.Message }
    //        );

    //        Console.WriteLine("Webhook has been successfully set up!");
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine($"Error connecting to the bot: {ex.Message}. Skipping webhook setup.");
    //    }
    //}
    //else
    //{
    //    Console.WriteLine("Invalid BotConfig or missing API Key. Skipping webhook setup.");
    //}

}

// ✅ Cấu hình Middleware
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

// ✅ Map Controller
app.MapControllers();

// ✅ Chạy ứng dụng
app.Run();
