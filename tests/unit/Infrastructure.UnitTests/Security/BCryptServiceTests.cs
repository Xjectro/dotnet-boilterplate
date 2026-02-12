using Api.Infrastructure.Security.BCrypt;
using FluentAssertions;
using Xunit;

namespace Infrastructure.UnitTests.Security;

public class BCryptServiceTests
{
    private readonly BCryptService _service = new();

    [Fact]
    public void HashPassword_ShouldProduceDifferentValue()
    {
        const string password = "Pa$$w0rd!";

        string hash = _service.HashPassword(password);

        hash.Should().NotBe(password);
        hash.Should().StartWith("$2");
    }

    [Fact]
    public void VerifyPassword_ShouldReturnTrueForValidPassword()
    {
        const string password = "let-me-in";
        string hash = _service.HashPassword(password);

        bool result = _service.VerifyPassword(hash, password);

        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_ShouldReturnFalseForInvalidPassword()
    {
        string hash = _service.HashPassword("correct-horse");

        bool result = _service.VerifyPassword(hash, "battery-staple");

        result.Should().BeFalse();
    }
}
