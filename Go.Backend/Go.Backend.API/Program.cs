using Go.Backend.Infrastructure.Persistence;
using Go.Backend.Infrastructure.AI;
using Go.Backend.Application.Interfaces;
using Go.Backend.Application.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 2. Cấu hình Database (SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GoDbContext>(options =>
    options.UseSqlite(connectionString));

// 3. Đăng ký các lớp Repositories & Services
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<GameService>(); // Service chính

// 4. Đăng ký AI Service (Singleton vì load model ONNX rất nặng, chỉ load 1 lần)
var modelPath = builder.Configuration["BotModel:ModelPath"];
if (string.IsNullOrEmpty(modelPath)) 
    throw new Exception("BotModel:ModelPath is missing in appsettings.json");

builder.Services.AddSingleton<IGoAiService>(sp => new OnnxGoAiService(modelPath));

// 5. Cấu hình CORS (Để ReactJS gọi được API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:5173") // Port mặc định của Vite
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// --- HTTP Request Pipeline ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Tự động chạy Migration khi start app (Tiện cho SQLite)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GoDbContext>();
    db.Database.EnsureCreated(); // Tự tạo file .db nếu chưa có
}

app.Run();