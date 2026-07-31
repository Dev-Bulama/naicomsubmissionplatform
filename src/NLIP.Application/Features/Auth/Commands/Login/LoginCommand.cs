using MediatR;

namespace NLIP.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Username, string Password, string? IpAddress) : IRequest<Dtos.AuthResultDto>;
