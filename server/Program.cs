using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Models;

var builder = WebApplication.CreateBuilder(args);

// The secret key used to sign login tokens (JWTs). It comes from JWT_KEY in the .env file.
// It must be at least 32 characters, because the signing method (HMAC-SHA256) needs a 256-bit key.
string? jwtKey = builder.Configuration["Jwt:Key"];
if (jwtKey == null || jwtKey.Length < 32)
{
    throw new Exception("Jwt:Key must be set and be at least 32 characters long.");
}
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

// Here we register the objects our controllers need. When a controller asks for one of them
// in its constructor, ASP.NET Core gives it automatically. This is called "dependency injection".
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddSingleton(signingKey);

// Check the login token on every request.
// A token is only accepted if it was signed with our secret key and has not expired.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // A token holds pieces of information called "claims", like the role.
        // This line keeps their names exactly as we wrote them in AuthController.
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = signingKey,
            // We only check the signature and the expiry time, not who created the token or who it is for
            ValidateIssuer = false,
            ValidateAudience = false,
            RoleClaimType = "role", // Tells [Authorize(Roles = ...)] to read the "role" claim
        };
    });

var app = builder.Build();

// Create the database and the first users.
// We need a "scope" to borrow the database for a moment outside of a web request.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

    db.Database.EnsureCreated(); // Creates the database file and tables if they don't exist

    // Adds a user with a hashed password
    void AddUser(string username, string role, string password)
    {
        var user = new User();
        user.Username = username;
        user.Role = role;
        user.PasswordHash = passwordHasher.HashPassword(user, password);
        db.Users.Add(user);
    }

    if (!db.Users.Any())
    {
        // The password for the users created here. It comes from SEED_PASSWORD in the .env file.
        string? password = app.Configuration["Seed:Password"];
        if (string.IsNullOrEmpty(password))
        {
            throw new Exception("Seed:Password must be set.");
        }

        AddUser("admin", Roles.Admin, password);

        // Example users, only when SEED_USERS=true in .env
        if (app.Configuration.GetValue<bool>("Seed:Users"))
        {
            AddUser("Bo", Roles.SuperUser, password);
            AddUser("Majland", Roles.SuperUser, password);
            AddUser("Bent", Roles.User, password);
            AddUser("Gert", Roles.User, password);
        }

        db.SaveChanges();
    }
}

app.UseAuthentication(); // Who are you? (reads the token)
app.UseAuthorization();  // Are you allowed? (checks the role)
app.MapControllers();    // Sends each request to the right controller method

app.Run();
