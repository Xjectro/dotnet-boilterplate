using Microsoft.AspNetCore.Mvc;

using Source.Models;
using Source.DTOs;
using Source.Repositories;
using Source.Services;

namespace Source.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IMemberRepository _memberRepository;
    private readonly IBCryptService _bcryptService;
    private readonly IJwtService _jwtService;

    public AuthController(IMemberRepository memberRepository, IBCryptService bcryptService, IJwtService jwtService)
    {
        _memberRepository = memberRepository;
        _bcryptService = bcryptService;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] AuthRegisterDto dto)
    {
        bool existingUsername = await _memberRepository.ExistsByUsernameAsync(dto.Username);
        if (existingUsername) return BadRequest(new
        {
            success = false,
            message = "Username already exists"
        });

        bool existingEmail = await _memberRepository.ExistsByEmailAsync(dto.Email);
        if (existingEmail) return BadRequest(new
        {
            success = false,
            message = "Email already exists"
        });

        Member member = new Member(
        dto.Username,
           dto.Email,
         _bcryptService.HashPassword(dto.Password)
        );

        await _memberRepository.AddAsync(member);

        return Ok(new { success = true });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginDto dto)
    {
        Member? member = await _memberRepository.GetByEmailAsync(dto.Email);
        if (member == null) return Unauthorized(new
        {
            success = false,
            message = "Email not exists"
        });

        if (!_bcryptService.VerifyPassword(member.Password, dto.Password))
            return Unauthorized(new
            {
                success = false,
                message = "Password incorrect"
            });

        var claims = new Dictionary<string, string>
    {
        { "id", member.Id.ToString() },
        { "username", member.Username }
    };

        var token = _jwtService.GenerateToken(claims);

        return Ok(new { success = true, token });
    }
}
