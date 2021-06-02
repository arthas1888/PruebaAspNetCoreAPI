using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Movie : BaseModel
    {
        [Required]
        public int? Year { get; set; }

        [Required]
        public string Title { get; set; }

        public int Duration { get; set; }


        public int? CategoryForeignKey { get; set; }

        [ForeignKey("CategoryForeignKey")]
        public Category Category { get; set; }
    }
}
