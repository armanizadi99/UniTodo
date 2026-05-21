using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using UniTodo.Modules.Auth;
using UniTodo.Modules.Todos.Infrastructure;
using UniTodo.Modules.Todos.ModuleStartup;
var builder = WebApplication.CreateBuilder(args);
// Add services to the container.
builder.Services.AddAuthentication(options =>
{
options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
options.Audience = "for my todo app";
options.ClaimsIssuer = "my own app";
options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
{
ValidAudience = "for my todo app",
ValidateAudience = true,
ValidIssuer = "my own app",
    ValidateIssuer = true,
ValidateIssuerSigningKey = false,
IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisisjut a fucking temp passwword I'll change it later, I just need to test something right now. Hardcoding such a code in my codebase might not be a big security issue because this code is running on a server trusted server, but well, because other parts of the applications might require it or well gonna right the rest."))
};
});

builder.Services.AddControllers()
.AddJsonOptions(options =>
{
options.JsonSerializerOptions.Converters
.Add(new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
});
builder.Services.AddTodoModule(builder.Configuration.GetSection("TodoModule"));
builder.Services.AddAuthModule(builder.Configuration.GetSection("AuthModule"));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
app.UseSwagger();
app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapTodoEndpoints();

app.Run();
