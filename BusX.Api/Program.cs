using BusX.Api.Middlewares;
using BusX.Domain.Interfaces;
using BusX.Infrastructure.Persistence;
using BusX.Infrastructure.Services;
using BusX.Infrastructure.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BusXDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("BusX.Infrastructure")));

builder.Services.AddHealthChecks();
builder.Services.AddMemoryCache();
builder.Services.AddLogging();

builder.Services.AddScoped<IBusBookingService, BusBookingService>();

builder.Services.AddValidatorsFromAssemblyContaining<CheckoutRequestValidator>();

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

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
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabaný oluþturulurken/seed edilirken bir hata oluþtu.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseMiddleware<CorrelationIdMiddleware>();

app.MapControllers();

app.MapHealthChecks("/health");

app.Run();