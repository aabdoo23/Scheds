using System.ComponentModel.DataAnnotations;

namespace Scheds.Models.Forum.ViewModels
{
    public class SignInViewModel
    {
        [Required]
        public string UserName { get; set; } 

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
