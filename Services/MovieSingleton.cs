using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class MovieSingleton
    {

        private readonly List<Movie> Items = new();

        public List<Movie> GetAll() => Items;
        public List<Movie> All { get => Items; }

        public void Add(Movie movie)
        {
            Items.Add(movie);
        }
    }
}
