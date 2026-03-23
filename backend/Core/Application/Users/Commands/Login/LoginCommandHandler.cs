using MediatR;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.IdentityModel.Tokens;
using Application.Users.Models;

namespace Application.Users.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommandRequest, LoginResponse>
{
    private readonly IConfiguration _configuration;

    public LoginCommandHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<LoginResponse> Handle(LoginCommandRequest request, CancellationToken cancellationToken)
    {
        var user = request.ValidatedUser;
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Secret"]);
        var expiresInMinutes = int.Parse(_configuration["Jwt:ExpiresInMinutes"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Role, user.Profile.ToString())
            ]),
            Expires = DateTime.UtcNow.AddMinutes(expiresInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenString = tokenHandler.WriteToken(token);
        return Task.FromResult(new LoginResponse
        {
            Token = tokenString,
            User = new UserLoginResponse()
            {
                Username = user.Username,
                Profile = user.Profile
            },
            ExpiresIn = expiresInMinutes * 60
        });
    }
}
