using System.Text;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OnlyBurger.Api.Auth;
using OnlyBurger.Api.Common.Middleware;
using OnlyBurger.Infrastructure.Auth;
using OnlyBurger.Infrastructure.Data;
using OnlyBurger.Domain.Repositories;
using OnlyBurger.Infrastructure.Realtime;

var builder = WebApplication.CreateBuilder(args);

// ----- Persistence -----
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Unit of Work + repositories: handlers depend on IUnitOfWork, never on AppDbContext directly.
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

        // Browsers can't set Authorization headers on a WebSocket handshake, so the SignalR
        // JS client sends the JWT as an `access_token` query-string value instead. Read it
        // back for requests targeting the hub so the connection is authenticated.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
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
            .AllowAnyMethod()
            // Required for the SignalR WebSocket connection from the browser.
            .AllowCredentials());
});

// ----- CQRS via MediatR: auto-register every command/query handler from the Infrastructure assembly -----
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AppDbContext).Assembly));

// ----- SignalR: real-time order + delivery tracking -----
builder.Services.AddSignalR();
builder.Services.AddScoped<IOrderNotifier, OrderNotifier>();

// ----- API / Swagger -----
builder.Services
    .AddControllers()
    // Serialize/accept enums as their readable names ("OutForDelivery") instead of numbers,
    // so request bodies and responses stay human-readable and match the string DB storage.
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
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
app.MapHub<OrderTrackingHub>("/hubs/orders");

app.Run();
