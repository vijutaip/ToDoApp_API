using ToDoApp_API.Interface;
using ToDoApp_API.Repository;
using ToDoApp_API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<ITaskRepository, TaskRepository>();
builder.Services.AddSingleton<TaskService>(); // ✅ This line is important

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy.WithOrigins("http://localhost:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var error = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (error != null)
        {
            // Log the exception (use a real logger in production)
            Console.WriteLine(error.Error);

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                Success = false,
                Message = "An unexpected error occurred. Please try again later."
            }));
        }
    });
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowReactApp");

app.MapControllers();

app.Run();
