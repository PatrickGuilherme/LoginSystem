using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LoginSystem.ViewModel
{
    public class ForgotPasswordModel
    {
        [Display(Name = "E-mail", Description = "E-mail de recuperação")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "E-mail inválido.")]
        [StringLength(250, MinimumLength = 5, ErrorMessage = "O e-mail deve de 5 a 250 dígitos")]
        [Required(ErrorMessage = "Preencha este campo")]
        public string Email { get; set; }
    }
}
