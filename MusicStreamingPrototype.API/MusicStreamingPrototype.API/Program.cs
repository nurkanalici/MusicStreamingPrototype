using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MusicStreamingPrototype.API.Data;
using MusicStreamingPrototype.API.Models;
using MusicStreamingPrototype.API.Repositories;
using MusicStreamingPrototype.API.Services;
using Serilog;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

#region ───────────── Serilog ─────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();
builder.Host.UseSerilog();
#endregion

#region ───────────── Services ─────────────

builder.Services.Configure<SpotifyOptions>(
    builder.Configuration.GetSection("Spotify"));
builder.Services.AddHttpClient<ISpotifyService, SpotifyService>(c =>
    c.BaseAddress = new Uri("https://api.spotify.com/v1/"));
builder.Services.AddMemoryCache();


builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));


builder.Services.AddScoped<IPlaylistRepository, PlaylistRepository>();
builder.Services.AddScoped<ITrackRepository, TrackRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IArchiveRepository, ArchiveRepository>();
builder.Services.AddScoped<IHelloService, HelloService>();


builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddApiVersioning(o =>
{
    o.DefaultApiVersion = new ApiVersion(1, 0);
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.ReportApiVersions = true;
    o.ApiVersionReader = new UrlSegmentApiVersionReader();
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MusicStreamingPrototype API", Version = "v1" });

    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "JWT Authorization header using the Bearer scheme. \"Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});


builder.Services.AddResponseCompression(opts =>
{
    opts.EnableForHttps = true;
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes
                     .Concat(new[] { "application/json" });
});
builder.Services.AddResponseCaching();


builder.Services.AddCors(o => o.AddPolicy("DefaultCors", p => p
    .WithOrigins("https://localhost:7184", "http://localhost:5151",
                 "https://localhost:5002", "http://localhost:5002")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));


var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });
builder.Services.AddAuthorization();
#endregion

var app = builder.Build();

#region ───────────── Migration + Seed ─────────────
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var spotify = scope.ServiceProvider.GetRequiredService<ISpotifyService>();
    await ctx.Database.MigrateAsync();
    await ctx.SeedAsync(spotify);
}
#endregion

#region ───────────── Middleware Pipeline ─────────────
if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.Use(async (ctx, next) =>
{
    string csp = app.Environment.IsDevelopment()
        ? "default-src 'self'; script-src 'self' 'unsafe-inline' 'unsafe-eval' blob:; " +
          "style-src 'self' 'unsafe-inline'; img-src 'self' data:; connect-src 'self' ws: blob:;"
        : "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data;";

    ctx.Response.Headers["Content-Security-Policy"] = csp;
    await next();
});

app.UseCors("DefaultCors");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json",
                      "MusicStreamingPrototype API v1");
    c.RoutePrefix = "swagger";
});


app.MapGet("/", ctx =>
{
    ctx.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.UseAuthentication();
app.UseAuthorization();

app.UseResponseCompression();
app.UseResponseCaching();


app.UseExceptionHandler(eApp =>
{
    eApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";

        var err = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        var json = JsonSerializer.Serialize(new
        {
            error = err?.Message,
            detail = err?.StackTrace
        });
        await context.Response.WriteAsync(json);
    });
});


app.Use(async (ctx, next) =>
{
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();
    Log.Information("Request {Method} {Path} => {Status} in {Elapsed} ms",
        ctx.Request.Method, ctx.Request.Path, ctx.Response.StatusCode,
        sw.ElapsedMilliseconds);
});

app.MapControllers();
#endregion

await app.RunAsync();
