using CourseRegistration.Data;
using CourseRegistration.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<RegistrationDbContext>(options =>
    options.UseInMemoryDatabase("CourseRegistration"));
builder.Services.AddScoped<ICourseRegistrationService, CourseRegistrationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.Run();
