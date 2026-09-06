using System.Text;
using System.Text.Json;
using backend.Data;
using backend.Data.Seed;
using backend.Jobs;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("DbConnection"))    
);

builder.Services.AddControllers()
    // Json serialization
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;        
    }
);

builder.Services.AddOpenApi();

// Cors
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
        )
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("admin_access_token"))
            {
                context.Token = context.Request.Cookies["admin_access_token"];
            }
            else if (context.Request.Cookies.ContainsKey("client_access_token"))
            {
                context.Token = context.Request.Cookies["client_access_token"];
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// Quartz
builder.Services.AddQuartz(q =>
{
    var tokenCleanupJobKey = new JobKey("TokenCleanupKey");

    q.AddJob<TokenCleanupJob>(options => options.WithIdentity(tokenCleanupJobKey));

    q.AddTrigger(options => options
        .ForJob(tokenCleanupJobKey)
        .WithIdentity("TokenCleanupCronTrigger")
        .WithCronSchedule("0 0 3 * * ?")    
    );
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

await AdminSeeder.SeedInitialAdminAccount(app.Services.GetRequiredService<IServiceScopeFactory>());

app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference("docs", options =>
    {
        options.WithTitle("Restaurant API")
            .WithTheme(ScalarTheme.DeepSpace)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// app.UseHttpsRedirection();

app.UseCors("FrontendPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
