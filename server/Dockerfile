# Step 1: build the C# code, using an image with the full .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Download the packages first, so Docker can reuse this step if only the code changes
COPY Server.csproj .
RUN dotnet restore

# Copy the code and build it into /app
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# Step 2: run the built app, using a smaller image that can only run .NET apps (not build them)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .

# A folder for the SQLite database file
RUN mkdir /data && chown app:app /data

# Run as the "app" user instead of root, for safety
USER app

# The app listens on port 8080 (the default for .NET in Docker)
ENTRYPOINT ["dotnet", "Server.dll"]
