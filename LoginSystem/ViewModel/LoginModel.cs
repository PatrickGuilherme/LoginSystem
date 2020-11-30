using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LoginSystem.ViewModel
{
    public class LoginModel
    {
        [Display(Name = "E-mail", Description = "E-mail de acesso")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "E-mail inválido.")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "O e-mail deve de 5 a 250 dígitos")]
        [Required(ErrorMessage = "Preencha este campo")]
        public string Email { get; set; }

        [Display(Name = "Senha", Description = "Senha de acesso")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "A senha deve ter de 8 a 20 dígitos")]
        [Required(ErrorMessage = "Preencha este campo")]
        public string Password { get; set; }
    }
}
