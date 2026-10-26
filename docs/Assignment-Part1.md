## Development of the login platform

Kvist & Bjælke ApS wants a login platform where security is considered from the beginning of the
development process.

### Roles
- **User** role: Should only have permission to read users in a table.
- **SuperUser** role: Should have permission to create, read, update and delete users.
- **Admin** role: Should be able to create, read, update and delete users, superusers and admins.
### Requirements
- Users must be able to authenticate using a username and password.
- The platform must implement role-based authorization with the roles *user*, *superuser*, and *admin*.
- Users’ passwords must never be stored or accessible in plaintext.
- Passwords must be stored using a secure password-hashing algorithm.
- The login platform must be accessible as a web-based application.
- The solution must be capable of running inside a Docker container.

## Implementation
Based on the description you must develop the login platform.
You must ensure that the solution meets the specified requirements.
During development, you must document the most important security-related decisions.
The documentation may include:
- A description of the chosen authentication method.
- A description of how passwords are handled and stored.
- Screenshots demonstrating key functionality.

The source code must be stored in a GitHub repository.