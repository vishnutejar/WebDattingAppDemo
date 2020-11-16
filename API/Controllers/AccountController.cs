using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _datacontext;
        private readonly ITokenService _tokenservice;
        public AccountController(DataContext datacontext, ITokenService tokenService)
        {
            _datacontext = datacontext;
            _tokenservice = tokenService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await IsUserExists(registerDto.Username)) return BadRequest("Username is taken");
            using var hmac = new HMACSHA512();
            var user = new AppUser
            {
                UserName = registerDto.Username.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.password)),
                PasswordSalt = hmac.Key
            };

            _datacontext.Users.Add(user);
            await _datacontext.SaveChangesAsync();

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenservice.CreateToken(user)
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _datacontext.Users
            .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

            if (user == null) return Unauthorized("Invalid username");

            using var hmac = new HMACSHA512(user.PasswordSalt);

            var computeHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i <= computeHash.Length; i++)
            {
                if (computeHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenservice.CreateToken(user)
            };
        }
        private async Task<bool> IsUserExists(string username)
        => await _datacontext.Users.AnyAsync(predicate: x => x.UserName == username.ToLower());

    }
}