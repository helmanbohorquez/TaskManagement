using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagement.Application.DTOs;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Api.Tests;

public class TasksEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public TasksEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"u-{Guid.NewGuid()}@tasks.test";
        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Passw0rd!"));
        register.EnsureSuccessStatusCode();
        var auth = (await register.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);
        return client;
    }

    [Fact]
    public async Task List_WithoutAuth_ShouldReturn401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task FullCrud_Flow_ShouldWork()
    {
        var client = await CreateAuthedClientAsync();

        var createRes = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Buy milk", "Dairy aisle", DateTime.UtcNow.AddDays(1)));
        createRes.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = (await createRes.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions.Default))!;

        var listRes = await client.GetAsync("/api/tasks");
        listRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = (await listRes.Content.ReadFromJsonAsync<List<TaskResponse>>(JsonOptions.Default))!;
        list.Should().ContainSingle(t => t.Id == created.Id);

        var getRes = await client.GetAsync($"/api/tasks/{created.Id}");
        getRes.StatusCode.Should().Be(HttpStatusCode.OK);

        var updateRes = await client.PutAsJsonAsync($"/api/tasks/{created.Id}",
            new UpdateTaskRequest("Buy milk & eggs", "Update desc", created.DueDate, TaskItemStatus.InProgress));
        updateRes.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = (await updateRes.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions.Default))!;
        updated.Title.Should().Be("Buy milk & eggs");
        updated.Status.Should().Be(TaskItemStatus.InProgress);

        var delRes = await client.DeleteAsync($"/api/tasks/{created.Id}");
        delRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDel = await client.GetAsync($"/api/tasks/{created.Id}");
        afterDel.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_WithEmptyTitle_ShouldReturn400()
    {
        var client = await CreateAuthedClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("", null, DateTime.UtcNow.AddDays(1)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithPastDueDate_ShouldReturn400()
    {
        var client = await CreateAuthedClientAsync();

        var response = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Past task", null, DateTime.UtcNow.AddDays(-1)));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithPastDueDate_ShouldReturn400()
    {
        var client = await CreateAuthedClientAsync();

        var createRes = await client.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Task", null, DateTime.UtcNow.AddDays(1)));
        var created = (await createRes.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions.Default))!;

        var updateRes = await client.PutAsJsonAsync($"/api/tasks/{created.Id}",
            new UpdateTaskRequest("Task", null, DateTime.UtcNow.AddDays(-1), TaskItemStatus.Pending));

        updateRes.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CrossUser_CannotSeeOthersTasks()
    {
        var alice = await CreateAuthedClientAsync();
        var bob = await CreateAuthedClientAsync();

        var create = await alice.PostAsJsonAsync("/api/tasks",
            new CreateTaskRequest("Alice task", "secret", DateTime.UtcNow.AddDays(1)));
        var created = (await create.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions.Default))!;

        var bobGets = await bob.GetAsync($"/api/tasks/{created.Id}");
        bobGets.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var bobList = (await (await bob.GetAsync("/api/tasks")).Content.ReadFromJsonAsync<List<TaskResponse>>(JsonOptions.Default))!;
        bobList.Should().NotContain(t => t.Id == created.Id);

        var bobDeletes = await bob.DeleteAsync($"/api/tasks/{created.Id}");
        bobDeletes.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Health_IsPublic()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
