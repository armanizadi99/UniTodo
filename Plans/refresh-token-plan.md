# Refresh‑Token Implementation Plan

**Current status:** Not started

---

## 1. Configuration – JwtSettings
- Add `RefreshTokenExpirationMinutes` (default 7 days) to `JwtSettings.cs`.

## 2. Domain – RefreshToken Entity
- New class `RefreshToken` with properties:
  - `Id` (GUID, public identifier)
  - `TokenHash` (hashed secret)
  - `UserId` (FK to user)
  - `Created`, `Expires`, `Revoked`
  - Computed `IsActive`
- Add `DbSet<RefreshToken>` to `AuthDbContext`.

## 3. Repository Layer
- Interface `IRefreshTokenRepository` extending `IRepository<RefreshToken>` with helpers:
  - `GetByIdAsync`
  - `GetActiveByIdAsync`
  - `GetActiveByUserIdAsync`
- Implementation `RefreshTokenRepository` using EF Core.

## 4. Service Layer – RefreshTokenService
- Generates cryptographically secure token `"{Guid}.{Base64UrlRandom}"`.
- Stores hash via `IPasswordHasher<ApplicationUser>`.
- Methods:
  - `CreateAsync(user)` → plain token
  - `ValidateAsync(token)` → `ApplicationUser` if active & valid
  - `RotateAsync(token)` → revokes old, creates new
  - `RevokeAsync(token)`
  - `RevokeAllAsync(userId)`
- Uses `JwtSettings.RefreshTokenExpirationMinutes` for expiry.

## 5. Dependency Injection
- Register `IRefreshTokenRepository` and `RefreshTokenService` in `AuthStartup.cs`.

## 6. DTOs
- `LoginResponseDto` (`Token`, `RefreshToken`, `Email`)
- `RefreshTokenRequestDto` (`RefreshToken`)
- `RefreshTokenResponseDto` (`Token`, `RefreshToken`, `Email`)
- `LogoutRequestDto` (`RefreshToken`)

## 7. AuthController Updates
- Inject `RefreshTokenService`.
- **LoginAsync** now returns both access and refresh tokens.
- Add **RefreshAsync** (`POST api/auth/refresh`) that validates, rotates, and returns new tokens.
- Add **LogoutAsync** (`POST api/auth/logout`) that revokes the supplied refresh token (or all tokens for the user).

## 8. Migrations
- EF Core migration `AddRefreshToken` to create `RefreshTokens` table.
- Ensure automatic migration on startup.

## 9. Unit Tests (xUnit + FluentAssertions + NSubstitute)
- Test creation stores hash, validates correctly, fails with wrong secret, revokes, rotates, respects expiration, controller actions return expected payloads.

## 10. Documentation (optional)
- XML comments for public members, brief note in repository docs.

---

*All steps follow the existing clean‑architecture conventions and naming style used throughout the project.*