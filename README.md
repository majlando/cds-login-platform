# Kvist & Bjælke Login Platform

A simple login platform with user roles.

Made with:
- **React + TypeScript** + **Bun**
- **C# .NET** 
- **Docker**

## Contents

### CDS.security
1. [How it works](#how-it-works)
2. [How to run it](#how-to-run-it)
   - [Settings](#settings)
   - [Users](#users)
3. [How to run it without Docker](#how-to-run-it-without-docker)
4. [Roles](#roles)
5. [API](#api)
6. [Project structure](#project-structure)
7. [Security decisions](#security-decisions)
8. [Screenshots](#screenshots)
9. [Glossary](#glossary)

# CD.security

## How it works

```mermaid
flowchart LR
    Browser -->|http://localhost:PORT | Client["Client<br/>React + Bun"]
    Client -->|/api requests| Server["Server<br/>C# .NET"]
    Server --> DB[("SQLite<br/>Database")]
```

The project has two parts:

- **Client**: The website (React + TypeScript + Bun)
- **Server**: The backend (C# ASP.NET Core Web API + SQLite)

### Docker
- Each part runs in its own **Docker container**.
- **Docker Compose** starts both containers with one command.

## How to run it

You must install Docker and have it running. 

1. Create your `.env` file from the example

```bash
cp .env.example .env
```

2. Start Docker Compose

```bash
docker compose up -d --build
```

3. Open http://localhost:8001

**To stop**

```bash
docker compose down      # stop
```

**To stop and delete the database**
```bash
docker compose down -v   # stop and delete the database
```

### Settings

Settings in `.env`:

| Setting         | What it does                                                           | Example     |
|-----------------|------------------------------------------------------------------------|-------------|
| `JWT_KEY`       | Random secret (at least 32 characters) used to sign login tokens       | Random text |
| `SEED_PASSWORD` | The password that users will have                                      | `password`  |
| `SEED_USERS`    | `true` will seed (create) demo users, `false` will only create `admin` | `true`      |

### Users

| Username | Role      | Created when      |
|----------|-----------|-------------------|
| admin    | Admin     | Always            |
| Bo       | SuperUser | `SEED_USERS=true` |
| Majland  | SuperUser | `SEED_USERS=true` |
| Bent     | User      | `SEED_USERS=true` |
| Gert     | User      | `SEED_USERS=true` |

## How to run it without Docker (for development)

**Terminal 1**: start the server on http://localhost:5000

```bash
cd server
dotnet run
```

**Terminal 2**: start the website on http://localhost:3000

```bash
cd client
bun install
bun run dev
```

> [!NOTE]
> The server will read [`server/appsettings.Development.json`](server/appsettings.Development.json) instead of `.env`.

## Roles

| Role      | Can read users | Can create, update and delete |
|-----------|----------------|-------------------------------|
| User      | Yes            | No                            |
| SuperUser | Yes            | Users                         |
| Admin     | Yes            | Users, SuperUsers, Admins     |

## API

HTTP methods:
- `POST` Create
- `GET` Read
- `PUT` Update
- `DELETE` Delete

**Endpoints**

| Method   | Endpoint          | What it does   | Who can use it   |
|----------|-------------------|----------------|------------------|
| `POST`   | `/api/login`      | Log in         | Everyone         |
| `GET`    | `/api/users`      | List all users | All roles        |
| `POST`   | `/api/users`      | Create user    | SuperUser, Admin |
| `PUT`    | `/api/users/{id}` | Update user    | SuperUser, Admin |
| `DELETE` | `/api/users/{id}` | Delete user    | SuperUser, Admin |

You can try it from the terminal while the app is running in Docker (without Docker, use port 3000):

To test the API, you can use `curl` or a browser.

Use port 3000 when not using Docker.

```bash
# Log in and get a token
curl -X POST http://localhost:8001/api/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "password"}'

# Without a token the server answers 401 Unauthorized
curl -i http://localhost:8001/api/users
```

**Status code**

| Code  | Meaning                                                        |
|-------|----------------------------------------------------------------|
| `200` | OK                                                             |
| `204` | OK      |
| `400` | Bad request           |
| `401` | Unauthorized   |
| `403` | Forbidden |
| `404` | Not found                             |
| `409` | Conflict                         |

## Project structure

```
client/src/
├── index.ts               Bun server
├── index.html             
├── main.tsx               
├── App.tsx                Shows the pages
├── api/api.ts             API
├── pages/LoginPage.tsx    Login form
├── pages/UsersPage.tsx    Users table
└── styles/index.css       Cascading Style Sheet

server/
├── Program.cs                       Startup
├── Controllers/AuthController.cs    POST /api/login
├── Controllers/UsersController.cs   POST, GET, PUT and DELETE /api/users
├── Models/User.cs                   User table and roles
├── Models/Dtos.cs                   Data Transfer Objects
└── Data/AppDbContext.cs             Database connection
```

## Security decisions

| Topic          | Decision                                                   |
|----------------|------------------------------------------------------------|
| Authentication | Username and password with a JWT that expires after 1 hour |
| Authorization  | Role is in JWT and checked on every request                |
| Passwords      | Stored as a hash, using PBKDF2 with a random salt          |
| Secrets        | Kept in `.env`, which is never committed to git            |

### Authentication

We use **JWT** (JSON Web Tokens)

```mermaid
sequenceDiagram
    participant B as Browser
    participant S as Server
    B->>S: POST /api/login (username + password)
    S->>S: Check password against hash
    S-->>B: JWT (username + role, signed with JWT_KEY)
    B->>S: GET /api/users + "Authorization: Bearer <token>"
    S->>S: Checksignature and role
    S-->>B: List of users
```

The Token is signed with **HMAC-SHA256**

- [`AuthController.cs`](server/Controllers/AuthController.cs) (create the token)
- [`Program.cs`](server/Program.cs) (check the token).

### Authorization: what are you allowed to do?

The server reads the role from the token on every request. You can see this in [`UsersController.cs`](server/Controllers/UsersController.cs):

- `[Authorize]` means you must be logged in. All roles can read users.
- `[Authorize(Roles = "SuperUser,Admin")]` means only those roles can create, update and delete.
- `CanManage` adds one more rule: a SuperUser can only manage users with the User role. This stops a SuperUser from creating Admins or giving someone a higher role.

### Passwords

- Passwords are **never stored in plaintext** (as they were typed). We only store a **hash**: a scrambled version that can't be turned back into the password.
- We use ASP.NET Core's built-in `PasswordHasher`. It uses an algorithm called **PBKDF2** (with HMAC-SHA512), which hashes the password 100,000 times, together with a random **salt**:
  - Hashing 100,000 times makes it slow on purpose. If an attacker steals the database, guessing passwords takes a very long time.
  - The salt is random data added to each password before hashing. Two users with the same password still get different hashes.
- When someone logs in, the typed password is hashed the same way and compared to the stored hash.
- The API never sends password hashes to the browser.

All five users have the same password, but every hash is different.

### Secrets

The JWT key and the password for the first users live in `.env`, which is listed in `.gitignore` so it is never committed to git. [`.env.example`](.env.example) shows which values are needed.

### Known limitations

- The Docker setup uses plain HTTP. On a real server it must use HTTPS, so passwords and tokens can't be read by others on the network.
- A JWT can't be cancelled. If a user is deleted, their token still works until it expires (at most 1 hour).

## Screenshots

<table>
  <tr>
    <td width="50%" valign="top">
      <img src="docs/screenshots/01-login.png" alt="Login page" />
      <p><b>Login page.</b> Users log in with a username and password.</p>
    </td>
    <td width="50%" valign="top">
      <img src="docs/screenshots/02-wrong-password.png" alt="Wrong password" />
      <p><b>Wrong password.</b> The same message is shown whether the username or the password is wrong.</p>
    </td>
  </tr>
  <tr>
    <td width="50%" valign="top">
      <img src="docs/screenshots/03-admin.png" alt="Admin view" />
      <p><b>Logged in as Admin.</b> An Admin can create users with any role, and can edit and delete everyone.</p>
    </td>
    <td width="50%" valign="top">
      <img src="docs/screenshots/04-admin-edit.png" alt="Admin editing a user" />
      <p><b>Admin editing a user.</b> Leaving the password empty keeps the old password.</p>
    </td>
  </tr>
  <tr>
    <td width="50%" valign="top">
      <img src="docs/screenshots/05-superuser.png" alt="SuperUser view" />
      <p><b>Logged in as SuperUser.</b> A SuperUser can only create, edit and delete users with the User role. There are no buttons for the Admin and the SuperUsers, and "User" is the only role in the dropdown.</p>
    </td>
    <td width="50%" valign="top">
      <img src="docs/screenshots/06-user.png" alt="User view" />
      <p><b>Logged in as User.</b> A User can only read the table. There is no form and there are no buttons.</p>
    </td>
  </tr>
  <tr>
    <td colspan="2">
      <img src="docs/screenshots/07-database.png" alt="Users table in the database" />
      <p><b>The database.</b> Only password hashes are stored, never the real passwords. All five users have the same password, but because of the random salt their hashes are completely different.</p>
    </td>
  </tr>
</table>

## Glossary

| Word           | Meaning                                                                       |
|----------------|-------------------------------------------------------------------------------|
| API            | The part of the server that the website talks to                              |
| Endpoint       | One URL on the API, like `/api/users`                                         |
| Authentication | Checking who you are (logging in)                                             |
| Authorization  | Checking what you are allowed to do (your role)                               |
| JWT            | A signed token that proves who you are and what role you have                 |
| Claim          | One piece of information inside a JWT, like the role                          |
| Hash           | A scrambled version of a password that can't be turned back                   |
| Salt           | Random data added to a password before hashing                                |
| Container      | A small, separate environment that runs one program with everything it needs  |


## CDS.infrastructure

[TODO]

## Final reflection

[TODO]