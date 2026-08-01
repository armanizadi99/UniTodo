using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniTodo.Tests.TodoModuleTests.IntegrationTests
{
    public class RunTests : IntegrationTestsBase
    {
        public RunTests( IntegrationTestsWebAppFactory factory ) : base(factory) { }

[Fact]
public async Task test1()
{
        AuthenticateClient(Guid.NewGuid().ToString());
        var response = await _client.GetAsync("api/runs/1");

        response.EnsureSuccessStatusCode();
        }
    }
}
