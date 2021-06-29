﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
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
using WebApplication1.Managers;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;
using WebApplication1.Utilities;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UsuarioManager _usuarioManager;
        private readonly IConfiguration _config;

        public UserController(ApplicationDbContext context, IConfiguration config, UsuarioManager usuarioManager)
        {
            _context = context;
            _config = config;
            _usuarioManager = usuarioManager;
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

        // POST api/user/RegisterUser
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> RegisterUser(Usuario model)
        {
            var res = await _usuarioManager.Register(model);
            if (!res.Succeeded)
            {
                res.Errors.ToList().ForEach(x => ModelState.AddModelError(x.Code, x.Description));
                return BadRequest(ModelState);
            }
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

        // POST api/user/LoginUser
        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("[action]")]
        [AllowAnonymous]
        public async Task<IActionResult> LoginUser(LoginViewModel model)
        {
            var res = await _usuarioManager.Login(model);
            if (res == null) return NotFound(); // 404
            if (res is Microsoft.AspNetCore.Identity.SignInResult @sign)
                if (!@sign.Succeeded)
                {
                    if (@sign.IsLockedOut) ModelState.AddModelError("Error", "IsLockedOut");
                    if (@sign.IsNotAllowed) ModelState.AddModelError("Error", "IsNotAllowed");
                    return BadRequest(ModelState);
                }
            var accessToken = GenerateJWT(res as ApplicationUser);
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

        private string GenerateJWT(ApplicationUser model)
        {
            var claims = new[]
            {
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
