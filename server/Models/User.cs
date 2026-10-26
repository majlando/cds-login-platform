namespace Server.Models;

// A user as it is stored in the database
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = ""; // The hashed password, never the real password
    public string Role { get; set; } = "";
}

// The three roles a user can have
public static class Roles
{
    public const string User = "User";
    public const string SuperUser = "SuperUser";
    public const string Admin = "Admin";

    public static readonly string[] All = { User, SuperUser, Admin };
}
