using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Models.ViewModels;

namespace WebApplication1.Managers
{
    public class UsuarioManager
    {
        public readonly ApplicationDbContext _context;
        public readonly UserManager<IdentityUser> _userManager;
        public readonly SignInManager<IdentityUser> _signInManager;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userManager"></param>
        /// <param name="signInManager"></param>
        public UsuarioManager(ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        /// Registro de usaurios
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<IdentityResult> Register(Usuario model)
        {
            return await _userManager.CreateAsync(new IdentityUser
            {
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.Phone
            }, model.Password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<dynamic> Login(LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user == null) return null;
            // Validate the username/password parameters and ensure the account is not locked out.
            var res = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);
            if (res.Succeeded) return user;
            return res;
        }
    }
}
