using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "local";
        options.LoginPath = "/login";
    })
    .AddCookie("special", options =>
    {
        options.Cookie.Name = "SpecialCookie";
        options.LoginPath = "/special";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("default", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("special", policy =>
    {
        policy.RequireAuthenticatedUser()
            .AddAuthenticationSchemes("special");
    });
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/login", async (ctx) =>
{
    // Here would be code for checking user's credentials

    var claims = new List<Claim>
    {
        new Claim("user", "martin"),
    };

    ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");
    ClaimsPrincipal user = new ClaimsPrincipal(identity);

    await ctx.SignInAsync("cookie", user);
});

app.MapGet("/special", async (ctx) =>
{
    // Here would be code to handle some different sign-in

    var claims = new List<Claim>
    {
        new Claim("ip", ctx.Connection.RemoteIpAddress == null ? "unknown" : ctx.Connection.RemoteIpAddress.ToString())
    };

    ClaimsIdentity identity = new ClaimsIdentity(claims, "cookie");
    ClaimsPrincipal user = new ClaimsPrincipal(identity);

    await ctx.SignInAsync("special", user);
});

app.MapGet("/", () => "Hello World!").RequireAuthorization("default");

app.MapGet("/papa", () => "tracked endpoint").RequireAuthorization("special");

app.MapControllers();

app.Run();