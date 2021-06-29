using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Graphql.Models
{

    public class MovieType : ObjectGraphType<Movie>
    {
        public MovieType()
        {
            Field(x => x.Id).Description("The Id of the Movie.");
            Field(x => x.Title).Description("The title of the Movie.");
            //Field<>(nameof(Movie.Year));
            Field(d => d.Year, nullable: true);
            Field(x => x.Duration).Description("The duration of the Movie.");
            Field<CategoryType>("Category", "Which movie they appear in.");
        }
    }
}
