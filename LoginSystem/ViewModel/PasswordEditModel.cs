using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LoginSystem.ViewModel
{
    public class PasswordEditModel
    {
        public int UserId { get; set; }

        [Display(Name = "Senha", Description = "Senha de acesso")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "A senha deve ter de 8 a 20 dígitos")]
        [Required(ErrorMessage = "Preencha este campo")]
        public string Password { get; set; }

        [Display(Name = "Confirmar senha", Description = "Comfirmar senha de acesso")]
        [DataType(DataType.Password)]
        [Required(ErrorMessage = "Preencha este campo")]
        public string PasswordConfirm { get; set; }
    }
}
