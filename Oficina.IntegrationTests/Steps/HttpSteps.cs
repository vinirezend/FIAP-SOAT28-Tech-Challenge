using Oficina.IntegrationTests.Support;
using Reqnroll;

namespace Oficina.IntegrationTests.Steps;

[Binding]
public class HttpSteps(ApiContext api)
{
    [Then(@"devo receber o status (\d+)")]
    public void EntaoDevoReceberOStatus(int status) =>
        Assert.Equal(status, (int)api.Response.StatusCode);
}
