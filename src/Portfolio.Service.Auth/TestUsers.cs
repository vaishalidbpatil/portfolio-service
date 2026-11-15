using Duende.IdentityServer.Test;
using System.Security.Claims;

namespace Portfolio.Service.Auth;

public static class TestUsers
{
    public static List<TestUser> Users =>
    [
        new TestUser
        {
            SubjectId = "1",
            Username = "demo",
            Password = "demo123",
            Claims =
            [
                new Claim("name", "Demo User"),
                new Claim("email", "demo@tradeorders.local")
            ]
        }
    ];
}
