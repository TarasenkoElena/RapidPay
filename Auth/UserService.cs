using Microsoft.AspNetCore.Identity;
using RapidPay.Utils;

namespace RapidPay.Auth;

public interface IUserService
{
    Task<bool> TryCreateUserAsync(string userName, string password);
    Task<string?> GenerateJwtTokenAsync(string userName, string password);
}

public class UserService(
    UserManager<ApiUser> userManager, 
    SignInManager<ApiUser> signInManager,
    ISaltGenerator saltGenerator, 
    IJwtTokenGenerator jwtTokenGenerator, 
    ILogger<UserService> logger) : IUserService
{
    public async Task<bool> TryCreateUserAsync(string userName, string password)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user != null)
        {
            logger.LogWarning("Trying to crete user with already used username='{username}'", userName);
            return false;
        }

        var newUser = new ApiUser { UserName = userName, Salt = saltGenerator.GenerateSalt() };

        var result = await userManager.CreateAsync(newUser, password);
        logger.LogInformation("User with username='{username}' created", userName);

        return result.Succeeded;
    }

    public async Task<string?> GenerateJwtTokenAsync(string userName, string password)
    {
        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            logger.LogWarning("Trying to find user by not exist username='{username}'", userName);
            return null;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            logger.LogWarning("Wrong password usage attempt for username='{username}'", userName);
            return null;
        }

        return jwtTokenGenerator.GenerateJwtToken(userName);
    }
}