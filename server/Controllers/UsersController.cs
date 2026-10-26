using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("api/users")]
[Authorize] // You must be logged in to use anything in this controller
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    // ASP.NET Core passes these in for us (they are registered in Program.cs)
    public UsersController(AppDbContext db, IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    // GET /api/users
    // All roles can read the list of users.
    [HttpGet]
    public async Task<List<UserDto>> GetAll()
    {
        List<User> users = await _db.Users.ToListAsync();

        var result = new List<UserDto>();
        foreach (User user in users)
        {
            result.Add(ToDto(user)); // Never send the password hash
        }
        return result;
    }

    // POST /api/users
    // Creates a user. Only SuperUsers and Admins can do this.
    [HttpPost]
    [Authorize(Roles = "SuperUser,Admin")]
    public async Task<ActionResult<UserDto>> Create(UserRequest request)
    {
        if (!CanManage(request.Role))
        {
            return Forbid(); // 403
        }
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Username and password are required."); // 400
        }
        if (await _db.Users.AnyAsync(u => u.Username == request.Username))
        {
            return Conflict("Username is taken."); // 409
        }

        var user = new User();
        user.Username = request.Username;
        user.Role = request.Role;
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password); // Only the hash is saved

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return ToDto(user);
    }

    // PUT /api/users/5
    // Updates the user with id 5. Only SuperUsers and Admins can do this.
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperUser,Admin")]
    public async Task<ActionResult<UserDto>> Update(int id, UserRequest request)
    {
        User? user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(); // 404
        }

        // Check both the user's current role and the new role,
        // so a SuperUser can't edit an Admin or turn a User into an Admin.
        if (!CanManage(user.Role) || !CanManage(request.Role))
        {
            return Forbid(); // 403
        }
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return BadRequest("Username is required."); // 400
        }
        if (await _db.Users.AnyAsync(u => u.Username == request.Username && u.Id != id))
        {
            return Conflict("Username is taken."); // 409
        }

        user.Username = request.Username;
        user.Role = request.Role;

        // An empty password means "keep the old password"
        if (!string.IsNullOrEmpty(request.Password))
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
        }

        await _db.SaveChangesAsync();
        return ToDto(user);
    }

    // DELETE /api/users/5
    // Deletes the user with id 5. Only SuperUsers and Admins can do this.
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperUser,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        User? user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound(); // 404
        }
        if (!CanManage(user.Role))
        {
            return Forbid(); // 403
        }

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent(); // 204: it worked, nothing to send back
    }

    // Is the logged-in user allowed to manage someone with this role?
    private bool CanManage(string role)
    {
        // The role must be one of the three real roles
        if (!Roles.All.Contains(role))
        {
            return false;
        }

        // Admins can manage every role
        if (User.IsInRole(Roles.Admin))
        {
            return true;
        }

        // SuperUsers can only manage users with the User role
        return role == Roles.User;
    }

    // Turns a database user into the data we send back (without the password hash)
    private static UserDto ToDto(User user)
    {
        return new UserDto(user.Id, user.Username, user.Role);
    }
}
