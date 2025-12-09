using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Excid.Staas.Data;
using Excid.Staas.Security;
using Staas.Security;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IJwtSigner, FileJwtSigner>();
builder.Services.AddSingleton<IRegistrySigner, RekorRegistrySigner>();
builder.Services.AddScoped<ISecureDbAccess, SecureDbAccess>();
builder.Services.AddDbContext<StassDbContext>(options => options.UseSqlite("Data Source=staas.db"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("cookies")
.AddOpenIdConnect("oidc", options => {
    options.Authority = builder.Configuration.GetValue<string>("OpenId:Authority");
    options.ClientId = builder.Configuration.GetValue<string>("OpenId:ClientId");
    options.ClientSecret = builder.Configuration.GetValue<string>("OpenId:ClientSecret");

    options.SignInScheme = "cookies";
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.Scope.Add("openid");
    options.Scope.Add("email");
    options.SaveTokens = true;
});

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
	name: "well-known",
	pattern: ".well-known/{action}",
	defaults: new { controller = "WellKnown", action = "openid-credential-issuer" });

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
