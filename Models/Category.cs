﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public class Category : BaseModel
    {
        [StringLength(100)]
        [Required]
        public string Name { get; set; }
    }
}