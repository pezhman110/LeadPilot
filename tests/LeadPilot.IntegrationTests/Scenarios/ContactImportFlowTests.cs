using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using LeadPilot.Application.Auth;
using LeadPilot.Application.Contacts;
using LeadPilot.Application.Managers;
using LeadPilot.Application.Projects;
using LeadPilot.Application.Tenants;
using LeadPilot.Server.Contracts;
using LeadPilot.IntegrationTests.Common;

namespace LeadPilot.IntegrationTests.Scenarios;

public sealed class ContactImportFlowTests : IClassFixture<LeadPilotWebApplicationFactory>
{
    private readonly LeadPilotWebApplicationFactory _factory;

    public ContactImportFlowTests(LeadPilotWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ImportReimportListAndRevealFlowWorksEndToEnd()
    {
        using HttpClient client = _factory.CreateClient();

        AuthTokenResponse adminToken = await LoginAsync(client, "admin@leadpilot.test", "AdminPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken.AccessToken);

        TenantDto tenant = await PostAsync<CreateTenantRequest, TenantDto>(client, "/api/tenants", new CreateTenantRequest("Synthetic Tenant"));
        ProjectDto project = await PostAsync<CreateProjectRequest, ProjectDto>(client, $"/api/tenants/{tenant.Id}/projects", new CreateProjectRequest("Q3 Outreach"));
        ManagerDto manager = await PostAsync<CreateManagerRequest, ManagerDto>(client, $"/api/tenants/{tenant.Id}/managers", new CreateManagerRequest("manager@leadpilot.test", "ManagerPassword123!"));

        AuthTokenResponse managerToken = await LoginAsync(client, manager.Email, "ManagerPassword123!");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", managerToken.AccessToken);

        string csv = "email,phone\nALICE@example.com,2025550101\nalice@example.com,(202) 555-0101\n";
        HttpResponseMessage importResponse = await PostCsvAsync(client, $"/api/projects/{project.Id}/contacts/import", csv);
        Assert.Equal(HttpStatusCode.Accepted, importResponse.StatusCode);

        HttpResponseMessage reimportResponse = await PostCsvAsync(client, $"/api/projects/{project.Id}/contacts/import", csv);
        Assert.Equal(HttpStatusCode.Accepted, reimportResponse.StatusCode);

        IReadOnlyList<ContactListItemDto>? contacts = await client.GetFromJsonAsync<IReadOnlyList<ContactListItemDto>>($"/api/projects/{project.Id}/contacts");

        Assert.NotNull(contacts);
        Assert.Equal(2, contacts!.Count);
        Assert.All(contacts, contact => Assert.DoesNotContain("alice", contact.MaskedValue, StringComparison.OrdinalIgnoreCase));
        Assert.Contains(contacts, contact => contact.MaskedValue == "a***@example.com");
        Assert.Contains(contacts, contact => contact.MaskedValue == "***-***-0101");

        ContactListItemDto emailContact = contacts.Single(contact => contact.ContactType == LeadPilot.Domain.Enums.ContactType.Email);
        HttpResponseMessage revealResponse = await client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/contacts/{emailContact.Id}/reveal",
            new RevealContactRequest("Synthetic verification"));

        Assert.Equal(HttpStatusCode.OK, revealResponse.StatusCode);
        RevealedContactDto? revealed = await revealResponse.Content.ReadFromJsonAsync<RevealedContactDto>();
        Assert.NotNull(revealed);
        Assert.Equal("ALICE@example.com", revealed!.Value);

        int auditCount = await _factory.CountRevealAuditsAsync();
        Assert.Equal(1, auditCount);
    }

    private static async Task<AuthTokenResponse> LoginAsync(HttpClient client, string email, string password)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/auth/token", new AuthRequest(email, password));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthTokenResponse>())!;
    }

    private static async Task<TResponse> PostAsync<TRequest, TResponse>(HttpClient client, string url, TRequest request)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<TResponse>())!;
    }

    private static async Task<HttpResponseMessage> PostCsvAsync(HttpClient client, string url, string csv)
    {
        using MultipartFormDataContent form = new();
        ByteArrayContent fileContent = new(Encoding.UTF8.GetBytes(csv));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
        form.Add(fileContent, "file", "contacts.csv");
        return await client.PostAsync(url, form);
    }
}