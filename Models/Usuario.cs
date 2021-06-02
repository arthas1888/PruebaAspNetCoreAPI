using DataAnnotationsExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Models
{
    public enum UserRole
    {
        ADMIN,
        USUARIO
    }

    public class UsuarioViewModel : BaseModel
    {
        [Email]
        [StringLength(255)]
        [Required]
        public string Email { get; set; }

        [StringLength(255)]
        [Required]
        public string Name { get; set; }
        [StringLength(255)]
        public string LastName { get; set; }

        public UserRole Role { get; set; }
    }

    public class Usuario : BaseModel
    {
        [Email]
        [StringLength(255)]
        [Required]
        public string Email { get; set; }
        [StringLength(255)]
        [Required]
        public string Password { get; set; }

        [NotMapped]
        [StringLength(255)]
        [Compare(nameof(Password), ErrorMessage = "La contrasena no es la misma")]
        public string ConfirmPassword { get; set; }


        [StringLength(255)]
        [Required]
        public string Name { get; set; }
        [StringLength(255)]
        public string LastName { get; set; }

        public UserRole Role { get; set; }
    }
}
