using RestaurantSaaS.Infrastructure.PasswordHashe;
using Xunit;

namespace RestaurantSaaS.Tests;

/// <summary>
/// Tests for <see cref="PasswordHasher"/> (BCrypt).
/// No database and no TestSupport are needed: the hasher works on plain strings.
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_does_not_return_the_original_password()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.NotEqual("Secret123!", hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void Verify_accepts_the_correct_password()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.True(_hasher.Verify("Secret123!", hash));
    }

    [Fact]
    public void Verify_rejects_a_wrong_password()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.False(_hasher.Verify("WrongPassword", hash));
    }

    [Fact]
    public void The_same_password_gets_a_different_hash_each_time()
    {
        var first = _hasher.Hash("Secret123!");
        var second = _hasher.Hash("Secret123!");

        // BCrypt uses a random salt, so the two hashes must differ...
        Assert.NotEqual(first, second);

        // ...but both must still verify the original password.
        Assert.True(_hasher.Verify("Secret123!", first));
        Assert.True(_hasher.Verify("Secret123!", second));
    }
}
