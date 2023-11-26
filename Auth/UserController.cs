using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace RapidPay.Auth;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Message = "Invalid model state." });

        if (await userService.TryCreateUserAsync(model.UserName, model.Password))
            return Created();

        return BadRequest(new { Message = "User creation failed." });
    }

    [HttpPost("token")]
    public async Task<IActionResult> GetToken([FromBody] UserModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Message = "Invalid model state." });

        var token = await userService.GenerateJwtTokenAsync(model.UserName, model.Password);
        if (token is null)
            return Unauthorized(new { Message = "Invalid username or password." });
        
        return Ok(new { Token = token });
    }
}

public class UserModel
{
    [MinLength(5)]
    public string UserName { get; set; } = null!;

    [MinLength(6)]
    public string Password { get; set; } = null!;
}