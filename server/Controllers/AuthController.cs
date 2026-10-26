using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("api")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly SymmetricSecurityKey _signingKey;

    // ASP.NET Core passes these in for us (they are registered in Program.cs)
    public AuthController(AppDbContext db, IPasswordHasher<User> passwordHasher, SymmetricSecurityKey signingKey)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _signingKey = signingKey;
    }

    // POST /api/login
    // Checks the username and password. If they are correct, it returns a login token.
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        User? user = await _db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);

        // We give the same error whether the username or the password is wrong,
        // so nobody can find out which usernames exist.
        if (user == null)
        {
            return Unauthorized("Wrong username or password."); // 401
        }

        // Hash the typed password and compare it with the stored hash
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Wrong username or password."); // 401
        }

        string token = CreateToken(user);
        return new LoginResponse(token, user.Username, user.Role);
    }

    // Creates a JWT that contains the username and role.
    // It is signed with our secret key, so the user can't change it, and it stops working after 1 hour.
    private string CreateToken(User user)
    {
        var claims = new Dictionary<string, object>();
        claims["name"] = user.Username;
        claims["role"] = user.Role;

        var tokenDescription = new SecurityTokenDescriptor();
        tokenDescription.Claims = claims;
        tokenDescription.Expires = DateTime.UtcNow.AddHours(1);
        tokenDescription.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        return new JsonWebTokenHandler().CreateToken(tokenDescription);
    }
}
