using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskManagement.Application.DTOs;

namespace TaskManagement.Api.Tests;

public class AuthEndpointsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public AuthEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
    }

    private static string UniqueEmail() => $"u-{Guid.NewGuid()}@tasks.test";

    [Fact]
    public async Task Register_ShouldReturnToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(UniqueEmail(), "Passw0rd!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409()
    {
        var client = _factory.CreateClient();
        var email = UniqueEmail();
        (await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Passw0rd!"))).EnsureSuccessStatusCode();

        var duplicate = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Passw0rd!"));

        duplicate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturn400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest(UniqueEmail(), "weak"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var client = _factory.CreateClient();
        var email = UniqueEmail();
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Passw0rd!"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "Passw0rd!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldReturn401()
    {
        var client = _factory.CreateClient();
        var email = UniqueEmail();
        await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, "Passw0rd!"));

        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "WrongPass1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
