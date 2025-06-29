
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MusicStreamingPrototype.UI.Services;
using System;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddControllersWithViews();


builder.Services.AddHttpContextAccessor();


var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
    throw new InvalidOperationException("ApiSettings:BaseUrl tanımsız.");

builder.Services.AddHttpClient<IApiClient, ApiClient>(c =>
{
    c.BaseAddress = new Uri(apiBaseUrl);
});


builder.Services.AddDistributedMemoryCache();


builder.Services.AddSession(opts =>
{
    opts.Cookie.HttpOnly = true;
    opts.IdleTimeout = TimeSpan.FromMinutes(60);
});


builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", opts =>
    {
        opts.LoginPath = "/Account/Login";
        opts.AccessDeniedPath = "/Account/AccessDenied";
        opts.Cookie.Name = "MusicStreaming.Auth";
        opts.Cookie.HttpOnly = true;
        opts.Cookie.SameSite = SameSiteMode.Lax;
        opts.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        opts.ExpireTimeSpan = TimeSpan.FromDays(14);
        opts.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();




var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
