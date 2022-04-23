using FootballStatsApi.Data.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddDbContext<FootballStatsContext>(
                //options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, "Server=localhost;Database=football-stats;User=sa;Password=123456789;"));
                options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, "Server=tcp:football-stats.database.windows.net,1433;Initial Catalog=football-stats-db2;Persist Security Info=False;User ID=abattassini;Password=u73hndsfiu;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FootballStatsCorsPolicy", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();

