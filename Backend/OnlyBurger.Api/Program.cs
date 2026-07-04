using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OnlyBurger.Api.Auth;
using OnlyBurger.Api.Common.Middleware;
using OnlyBurger.Api.Cqrs;
using OnlyBurger.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// ----- Persistence -----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ----- Auth / security services -----
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()!;
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// ----- CORS for the React/Vite dev frontend -----
const string FrontendCorsPolicy = "FrontendCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://localhost:4173",
                "http://127.0.0.1:5173",
                "http://127.0.0.1:4173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// ----- CQRS: auto-register every command/query handler -----
builder.Services.AddCqrs();

// ----- API / Swagger -----
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OnlyBurger API",
        Version = "v1",
        Description = "Online ordering of burgers and fast food. Built with ASP.NET Core, EF Core (SQLite), JWT auth and the CQRS pattern."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Paste your JWT here. The 'Bearer ' prefix is added automatically.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document, null), new List<string>() }
    });
});

var app = builder.Build();

// ----- Apply migrations and seed baseline data -----
await DbInitializer.InitializeAsync(app.Services);

// ----- HTTP pipeline -----
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OnlyBurger API v1");
        options.RoutePrefix = "swagger";
    });
}
else
{
    // In development the browser frontend talks to the API over plain HTTP, so only
    // force HTTPS outside development to avoid redirect/cert friction.
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
