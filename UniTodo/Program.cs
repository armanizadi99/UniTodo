using UniTodo.Modules.Todos.Infrastructure;
using UniTodo.Modules.Todos.ModuleStartup;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddTodoModule(builder.Configuration.GetSection("TodoModule"));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapTodoEndpoints();

app.Run();
