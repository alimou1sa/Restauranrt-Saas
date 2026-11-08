using RestaurantSaaS.Infrastructure.Auth;
using Xunit;

namespace RestaurantSaaS.Tests;

/// <summary>
/// Tests for <see cref="RefreshTokenHasher"/>.
/// No database and no TestSupport are needed: it only creates and hashes strings.
/// </summary>
public class RefreshTokenHasherTests
{
    private readonly RefreshTokenHasher _hasher = new();

    [Fact]
    public void Generated_tokens_are_different()
    {
        var first = _hasher.GenerateRawToken();
        var second = _hasher.GenerateRawToken();

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void Hashing_the_same_token_gives_the_same_result()
    {
        var token = _hasher.GenerateRawToken();

        Assert.Equal(_hasher.Hash(token), _hasher.Hash(token));
    }

    [Fact]
    public void The_hash_is_not_the_raw_token()
    {
        var token = _hasher.GenerateRawToken();

        Assert.NotEqual(token, _hasher.Hash(token));
    }
}
