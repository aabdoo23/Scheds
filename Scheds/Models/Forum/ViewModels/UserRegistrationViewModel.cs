using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Scheds.Models.Forum.ViewModels
{
    public class UserRegistrationViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PhoneNumber { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Required]
        public int MajorId { get; set; }
        public IEnumerable<SelectListItem> Majors { get; set; }
        [Required]
        public int FacultyId { get; set; } // Faculty selection

        public IEnumerable<SelectListItem> Faculties { get; set; } // Faculties dropdown

    }
}
