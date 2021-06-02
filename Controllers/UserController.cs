using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using WebApplication1.Utilities;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public UserController(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // GET: api/USER
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<IEnumerable<UsuarioViewModel>>> GetAll()
        {
            return Ok(await _context.Usuarios                
                .ToListAsync());
        }

        // POST api/user/Register
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [Authorize(Roles = "ADMIN")]
        public IActionResult Register(Usuario model)
        {
            model.Password = model.Password.EncryptHash256();
            _context.Usuarios.Add(model);
            _context.SaveChangesAsync();
            return Ok();
        }

        // POST api/user/login
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            model.Password = model.Password.EncryptHash256();
            var user = await _context.Usuarios.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user == null) return NotFound(); // 404 
            if (!user.Password.Equals(model.Password))
            {
                ModelState.AddModelError("Error", "Credenciales invalidas");
                return BadRequest(ModelState); // 400
            }
            var accessToken = GenerateJWT(user);

            return Ok(new
            {
                AccessToken = accessToken
            }); // 200
        }

        #region helper
        /// <summary>
        /// Json Web Token
        /// </summary>
        private string GenerateJWT(Usuario model)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, model.Name),
                new Claim(ClaimTypes.Role, model.Role.ToString()),
                new Claim(ClaimTypes.NameIdentifier, model.Email),
            };

            var key = Encoding.ASCII.GetBytes(_config["TokenValidationParameters:IssuerSigningKey"]);
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
            var token = new JwtSecurityToken(
                  _config["TokenValidationParameters:Issuer"],
                  _config["TokenValidationParameters:Audience"],
                  claims,
                  expires: DateTime.UtcNow.AddMinutes(60),
                  signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}
