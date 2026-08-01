using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
[Collection("SharedIntegrationCollection")]
    public abstract class IntegrationTestsBase: IAsyncLifetime
    {
        protected readonly HttpClient _client;
        protected readonly IntegrationTestsWebAppFactory _factory;

protected IntegrationTestsBase( IntegrationTestsWebAppFactory factory )
        {
        _client = factory.CreateClient();
        _factory = factory;
        }

        public Task DisposeAsync()
        {
        return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
        await _factory.ResetDatabaseAsync();
        }

        protected void AuthenticateClient( string userId)
        {
        var dummySecret = IntegrationTestsWebAppFactory.DummyJwtSecretKey;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(dummySecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, userId),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        var token = new JwtSecurityToken(
            issuer: "UniTodoApi",
            audience: "UniTodoApi",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtString = tokenHandler.WriteToken(token);

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtString);
        }
    }
}
