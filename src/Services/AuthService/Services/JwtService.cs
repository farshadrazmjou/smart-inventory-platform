using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Models;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services;
public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config=config;
    }

    public string GenerateToken(User user)
    {
        var JwtSettings=_config.GetSection("JwtSettings");
        var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings["Key"]!));
        var creds=new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

        var claims= new[]
        {
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
            new Claim(ClaimTypes.Name,user.Username),
            new Claim(ClaimTypes.Email,user.Email),
        };

        var token=new JwtSecurityToken(
            issuer:JwtSettings["issuer"],
            audience:JwtSettings["audience"],
            claims: claims,
            expires:DateTime.UtcNow.AddMinutes(int.Parse(JwtSettings["ExpiryMinutes"]!)),
            signingCredentials:creds
            );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}