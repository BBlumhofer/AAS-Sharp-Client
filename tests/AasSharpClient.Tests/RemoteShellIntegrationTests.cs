using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BaSyx.Clients.AdminShell.Http;
using BaSyx.Models.AdminShell;
using BaSyx.Utils.ResultHandling;
using Xunit;
using Xunit.Sdk;

namespace AasSharpClient.Tests;

/// <summary>
/// Integration tests that talk to a locally running BaSyx server (http://localhost:8080).
/// These tests are skipped automatically when the server is unavailable.
/// </summary>
public class RemoteShellIntegrationTests
{
    private static readonly Uri ShellRepositoryUri = new("http://localhost:8080/shells", UriKind.Absolute);
    private static readonly Uri SubmodelRepositoryUri = new("http://localhost:8080/submodels", UriKind.Absolute);
    private static readonly Identifier ShellIdentifier = new("https://smartfactory.de/shells/mio_fDp69z");
    private const string ProductIdentificationSubmodelId = "https://smartfactory.de/submodels/Q_nJW61FiN";

    [Fact]
    public async Task Reads_ProductIdentification_From_Remote_Shell()
    {


        var aasClient = new AssetAdministrationShellRepositoryHttpClient(ShellRepositoryUri);
        var shellResult = await aasClient.RetrieveAssetAdministrationShellAsync(ShellIdentifier);
        Assert.True(shellResult.Success, FormatMessages(shellResult.Messages));

        var shell = Assert.IsAssignableFrom<IAssetAdministrationShell>(shellResult.Entity);
        var productReference = shell.SubmodelReferences?
            .FirstOrDefault(r => r.Keys?.Any(k => k.Type == KeyType.Submodel &&
                                                  string.Equals(k.Value, ProductIdentificationSubmodelId, StringComparison.OrdinalIgnoreCase)) == true);

        Assert.NotNull(productReference);

        var submodelClient = new SubmodelRepositoryHttpClient(SubmodelRepositoryUri);
        var submodelResult = await submodelClient.RetrieveSubmodelAsync(new Identifier(ProductIdentificationSubmodelId));
        Assert.True(submodelResult.Success, FormatMessages(submodelResult.Messages));

        var retrieved = Assert.IsAssignableFrom<ISubmodel>(submodelResult.Entity);
        var typedSubmodel = Assert.IsAssignableFrom<Submodel>(retrieved);

        var identifier = GetStringProperty(typedSubmodel, "Identifier");
        var productName = GetStringProperty(typedSubmodel, "ProductName");

        Assert.False(string.IsNullOrWhiteSpace(identifier));
        Assert.False(string.IsNullOrWhiteSpace(productName));
    }

    private static async Task<bool> EnsureServerAvailableAsync()
    {
        if (Environment.GetEnvironmentVariable("BXS_SKIP_INTEGRATION_TESTS") == "1")
        {
            return false;
        }

        using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
        try
        {
            using var response = await http.GetAsync(ShellRepositoryUri);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }

        return true;
    }

    private static string FormatMessages(MessageCollection? messages)
    {
        return messages is null || messages.Count == 0
            ? "No diagnostic messages returned by repository."
            : messages.ToString();
    }

    private static string? GetStringProperty(Submodel submodel, string idShort)
    {
        var property = submodel.SubmodelElements
            .OfType<Property>()
            .FirstOrDefault(p => string.Equals(p.IdShort, idShort, StringComparison.OrdinalIgnoreCase));

        return property?.Value?.Value?.ToString();
    }
}
