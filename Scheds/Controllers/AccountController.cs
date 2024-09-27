using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Scheds.DAL;
using Scheds.DAL.Repositories;
using Scheds.Models.Forum;
using Scheds.Models.Forum.ViewModels;

namespace Scheds.Controllers
{
    public class AccountController : Controller
    {
        private readonly SchedsDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly UserRepository _userRepository;

        public AccountController(SchedsDbContext context, UserRepository userRepository)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _userRepository = userRepository;
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.GetUserByUsernameAsync(model.UserName);

                if (user != null)
                {
                    var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

                    if (result == PasswordVerificationResult.Success)
                    {
                        HttpContext.Session.SetString("UserName", user.UserName);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Invalid password.";
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "User does not exist.";
                }
            }

            return View(model);
        }
        public IActionResult SignOut()
        {
            HttpContext.Session.Remove("UserName");
            return RedirectToAction("SignIn");
        }
        public IActionResult SignUp()
        {
            var faculties = _context.Faculties.Select(f => new SelectListItem
            {
                Value = f.FacultyId.ToString(),
                Text = f.FacultyName
            }).ToList();

            var majors = _context.Majors.Select(m => new SelectListItem
            {
                Value = m.MajorId.ToString(),
                Text = m.MajorName
            }).ToList();

            var viewModel = new UserRegistrationViewModel
            {
                Faculties = faculties,
                Majors = majors 
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserRegistrationViewModel model)
        {
            var user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                MajorId = model.MajorId,
                FacultyId = _context.Majors.Where(m => m.MajorId == model.MajorId)
                                           .Select(m => m.FacultyId)
                                           .FirstOrDefault()
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            try
            {
                await _userRepository.AddUserAsync(user);
            }
            catch (DbUpdateException ex)
            {
                ModelState.AddModelError("", "An error occurred while registering the user. Please try again.");
                return View(model);
            }
            return RedirectToAction("SignIn");
        }


        public IActionResult GetMajorsByFaculty(int facultyId)
        {
            var majors = _context.Majors
                .Where(m => m.FacultyId == facultyId)
                .Select(m => new SelectListItem
                {
                    Value = m.MajorId.ToString(),
                    Text = m.MajorName
                }).ToList();

            return Json(majors);
        }

    }
}
