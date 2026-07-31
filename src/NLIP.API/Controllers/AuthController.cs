using MediatR;
using Microsoft.AspNetCore.Mvc;
using NLIP.Application.Features.Auth.Commands.Login;
using NLIP.Application.Features.Auth.Commands.RefreshUserToken;

namespace NLIP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    public record LoginRequest(string Username, string Password);
    public record RefreshRequest(string RefreshToken);

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new LoginCommand(request.Username, request.Password, ip), cancellationToken);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken, ip), cancellationToken);
        return Ok(result);
    }
}
