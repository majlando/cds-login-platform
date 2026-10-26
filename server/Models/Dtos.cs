namespace Server.Models;

// The data sent to and from the API. DTO means "Data Transfer Object".
// A "record" is a simple class that just holds data.

// What the browser sends to log in
public record LoginRequest(string Username, string Password);

// What the server sends back after a successful login
public record LoginResponse(string Token, string Username, string Role);

// What the browser sends to create or update a user.
// When updating, an empty password means "keep the old password".
public record UserRequest(string Username, string? Password, string Role);

// What the server sends back about a user. It never includes the password hash.
public record UserDto(int Id, string Username, string Role);
