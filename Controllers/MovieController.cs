using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenIddict.Validation.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using WebApplication1.Utilities;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public class MovieController : ControllerBase
    {
        //private readonly MovieSingleton _movieSingleton;
        private readonly ILogger _logger;
        private readonly ApplicationDbContext _context;
        private readonly IGenericCRUD<Movie> _service;

        public MovieController(ILogger<MovieController> logger, ApplicationDbContext context, IGenericCRUD<Movie> service)
        {
            _context = context;
            _logger = logger;
            _service = service;
        }

        // GET: api/movie/
        [HttpGet]
        public IEnumerable<Movie> All(int? year)
        {
            _logger.LogInformation($"year {year} user {HttpContext.User.Identity.Name}");
            //throw new Exception();
            return year != null ? _context.Movies
                .Include(x => x.Category).Where(x => x.Year == year).ToList() : _context.Movies
                .Include(x => x.Category)
                .ToList();
        }

        // GET:  api/movie/Sum
        [HttpGet("[action]")]
        public IActionResult Sum(int number1, int number2)
        {
            //var res = OperationExtensions.CustomSum(number1, number2);
            return Ok(new
            {
                Resultado = number1.CustomSum(number2)
            });
        }


        // PUT: api/Movie/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> PutMovie(int id, Movie movie)
        {
            movie = await _service.Update(id, movie);
            if (movie == null) return NotFound();
            return Ok(movie);
        }

        // POST: api/Movie
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<Movie>> PostMovie(Movie movie)
        {
            return Ok(await _service.Create(movie));
        }
    }
}
