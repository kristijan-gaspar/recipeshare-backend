using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using RecipeShare.Application.DTOs.Auth;
using RecipeShare.Application.Exceptions;
using RecipeShare.Application.Interfaces.Repositories;
using RecipeShare.Application.Interfaces.Services;
using RecipeShare.Domain.Entities;

namespace RecipeShare.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtProvider _jwtProvider;
    private readonly IPasswordHasher<User> _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork,
        IJwtProvider jwtProvider,
        IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _jwtProvider = jwtProvider;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            throw new BadRequestException("Email is already taken.");

        if (await _userRepository.UsernameExistsAsync(request.Username))
            throw new BadRequestException("Username is already taken.");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var tokenResult = _jwtProvider.GenerateToken(user);
        var (rawRefreshToken, refreshExpiresAt) = await CreateRefreshTokenAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Token = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new UnauthorizedException("Invalid email or password.");

        if (user.IsBlocked)
            throw new ForbiddenException("Your account is blocked.");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedException("Invalid email or password.");

        var tokenResult = _jwtProvider.GenerateToken(user);
        var (rawRefreshToken, refreshExpiresAt) = await CreateRefreshTokenAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Token = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        var hash = HashToken(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(hash);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
            throw new UnauthorizedException("Invalid or expired refresh token.");

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null)
            throw new UnauthorizedException("User not found.");

        if (user.IsBlocked)
        {
            storedToken.IsRevoked = true;
            await _unitOfWork.SaveChangesAsync();
            throw new ForbiddenException("Your account is blocked.");
        }

        storedToken.IsRevoked = true;

        var tokenResult = _jwtProvider.GenerateToken(user);
        var (rawRefreshToken, refreshExpiresAt) = await CreateRefreshTokenAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponse
        {
            Token = tokenResult.Token,
            ExpiresAt = tokenResult.ExpiresAt,
            UserId = user.Id,
            Username = user.Username,
            Role = user.Role,
            RefreshToken = rawRefreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt
        };
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var hash = HashToken(refreshToken);
        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(hash);

        if (storedToken == null || storedToken.IsRevoked)
            return;

        storedToken.IsRevoked = true;
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<(string rawToken, DateTime expiresAt)> CreateRefreshTokenAsync(int userId)
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var refreshToken = new RefreshToken
        {
            TokenHash = HashToken(rawToken),
            UserId = userId,
            ExpiresAt = expiresAt
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        return (rawToken, expiresAt);
    }

    private static string HashToken(string token)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
