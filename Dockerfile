FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy only the project file first (cache optimization)
COPY ["UniTodo/UniTodo.csproj", "UniTodo/"]
RUN dotnet restore "UniTodo/UniTodo.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/UniTodo"
RUN dotnet publish "UniTodo.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

# ============================================
# Stage 2: Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Ensure the app user owns the WORKDIR so it can create DB and log files
RUN chown app:app /app

# Switch to the non-root user
USER app

# Copy the published output from the build stage
COPY --from=build --chown=app:app /app/publish .

# Configure the container to listen on port 8080 (the default for .NET 8+)
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Startup
ENTRYPOINT ["dotnet", "UniTodo.dll"]