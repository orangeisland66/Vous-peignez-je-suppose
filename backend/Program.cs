// var builder = WebApplication.CreateBuilder(args);

// // Add services to the container.
// // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

// var app = builder.Build();

// // Configure the HTTP request pipeline.
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// app.UseHttpsRedirection();

// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };

// app.MapGet("/weatherforecast", () =>
// {
//     var forecast =  Enumerable.Range(1, 5).Select(index =>
//         new WeatherForecast
//         (
//             DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//             Random.Shared.Next(-20, 55),
//             summaries[Random.Shared.Next(summaries.Length)]
//         ))
//         .ToArray();
//     return forecast;
// })
// .WithName("GetWeatherForecast");

// app.Run();

// record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
// {
//     public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
// }
using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Services;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.SignalR;
using backend.Hubs;
using backend.Models;
using backend.Runtime;
var builder = WebApplication.CreateBuilder(args);

//GameRoomRuntimeManager注册为 Singleton
builder.Services.AddSingleton<GameRoomRuntimeManager>();
// 添加数据库上下文
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OurDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 注册服务
builder.Services.AddScoped<GameRoomService>();
builder.Services.AddScoped<GameService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<WordManager>();

// 添加控制器
builder.Services.AddControllers() // 或者 AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // 通常，对于 API 返回，忽略循环更简单直接。
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });
});

// 添加 SignalR 服务
builder.Services.AddSignalR(); 

var app = builder.Build();
app.MapHub<GameHub>("/gameHub"); 
// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 使用路由和控制器
app.UseRouting();
app.MapControllers();
//使用swagger进行测试
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
});

app.Run();




