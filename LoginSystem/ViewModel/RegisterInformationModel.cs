using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LoginSystem.ViewModel
{
    public class RegisterInformationModel
    {
        [Display (Name = "Data de nascimento", Description = "Data de nascimento do usuário")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        [Required (ErrorMessage ="Preencha este campo")]
        public DateTime BirthData { get; set; }

        [Display(Name = "Gênero", Description = "Gênero do usuário")]
        [StringLength(1, MinimumLength = 1)]
        [Required(ErrorMessage = "Preencha este campo")]
        public string Genre { get; set; }

        [Display(Name = "Número de celular", Description = "Número de celular do usuário")]
        [StringLength(15, MinimumLength = 5, ErrorMessage = "O número de telefone deve ter de 5 a 11 digitos")]
        [Required(ErrorMessage = "Preencha este campo")]
        public string PhoneNumber { get; set; }
    }
}
