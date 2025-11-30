using BusX.Domain.Interfaces;
using BusX.Infrastructure.Persistence;
using BusX.Infrastructure.Services;
using BusX.Api.Middlewares;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BusXDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("BusX.Infrastructure")));

builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();

builder.Services.AddScoped<IBusBookingService, BusBookingService>();

builder.Services.AddRouting(options => options.LowercaseUrls = true); 
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BusXDbContext>();

        await context.Database.EnsureCreatedAsync();
        await BusXContextSeed.SeedAsync(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine("DB Hatasý: " + ex.Message);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseMiddleware<CorrelationIdMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health"); 

app.Run();