using DesafioBenner.Repositories;
using DesafioBenner.Repositories.Interfaces;
using DesafioBenner.Services;
using DesafioBenner.Services.Interfaces;
using Infrastructure.DataBase;
using Infrastructure.DTO;
using Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddPolicy("MyPolicy", policy =>
{
    policy.WithOrigins("http://localhost:3000")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
}));

// Add services to the container.

builder.Services.Configure<ConnectionDataBaseDTO>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Injection Dependencies Services
builder.Services.AddTransient<IParkingService, ParkingService>();
builder.Services.AddTransient<IPriceService, PriceService>();

// Injection Dependencies Repositories
builder.Services.AddTransient<IParkingRepository, ParkingRepository>();
builder.Services.AddTransient<IPriceRepository, PriceRepository>();

builder.Services.AddTransient<Utils>();

builder.Services.AddDbContext<Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DesafioBenner"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    }));

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    Context context = scope.ServiceProvider.GetRequiredService<Context>();

    context.Database.EnsureCreated();
}

app.UseCors("MyPolicy");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
