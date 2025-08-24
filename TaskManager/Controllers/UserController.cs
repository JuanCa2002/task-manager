using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TaskManager.Messages.Constants;
using TaskManager.Models.ViewModels;

namespace TaskManager.Controllers
{
    public class UserController: Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        public UserController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _dbContext = dbContext;
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.AdminRole)]
        public async Task<IActionResult> RemoveAdmin(string email)
        {
            var user = await _dbContext.Users.Where(user => user.Email!.Equals(email)).FirstOrDefaultAsync();

            if (user is null)
            {
                return NotFound();
            }

            await _userManager.RemoveFromRoleAsync(user, RoleConstants.AdminRole);

            return RedirectToAction("List",
                routeValues: new { message = $"Rol removido correctamente a {email}" });
        }

        [HttpPost]
        [Authorize(Roles = RoleConstants.AdminRole)]
        public async Task<IActionResult> MakeAdmin(string email)
        {
            var user = await _dbContext.Users.Where(user => user.Email!.Equals(email)).FirstOrDefaultAsync();

            if(user is null)
            {
                return NotFound();
            }

            await _userManager.AddToRoleAsync(user, RoleConstants.AdminRole);

            return RedirectToAction("List", 
                routeValues: new { message = $"Rol asignado correctamente a {email}"});
        }

        [HttpGet]
        [Authorize(Roles = RoleConstants.AdminRole)]
        public async Task<IActionResult> List(string? message)
        {
            var users = await _dbContext
                .Users
                .Select(u => new UserViewModel
                {
                    Email = u.Email!
                }).ToListAsync();

            var model = new UserListViewModel
            {
                Users = users,
                Message = message
            };

            return View(model);

        }

        [AllowAnonymous]
        [HttpGet]
        public ChallengeResult ExternLogIn(string supplier, string? returnUrl = null)
        {
            var redirectionUrl = Url.Action("RegisterExternUser", values : new { returnUrl });
            var properties = _signInManager
                .ConfigureExternalAuthenticationProperties(supplier, redirectionUrl);
            return new ChallengeResult(supplier,properties);

        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> RegisterExternUser(string? returnUrl = null,
            string? remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            var message = "";

            if (remoteError is not null)
            {
                message = $"Error del proveedor externo: {remoteError}";
                return RedirectToAction("LogIn", routeValues: new { message });
            }

            var information = await _signInManager.GetExternalLoginInfoAsync();

            if (information is null)
            {
                message = "Error cargando la data de inicio de sesión externo.";
                return RedirectToAction("LogIn", routeValues: new { message });
            }

            var externLoginResult = await _signInManager.ExternalLoginSignInAsync(information.LoginProvider,
                information.ProviderKey, isPersistent: true, bypassTwoFactor: true);

            if (externLoginResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            string? email = "";

            if (information.Principal.HasClaim(claim => claim.Type.Equals(ClaimTypes.Email)))
            {
                email = information.Principal.FindFirstValue(ClaimTypes.Email);
            }
            else
            {
                message = "Error leyendo el correo electronico del usuario del proveedor";
                return RedirectToAction("LogIn", routeValues: new { message });
            }

            var user = new IdentityUser
            {
                Email = email,
                UserName = email,
            };

            var createdUserResult = await _userManager.CreateAsync(user);

            if(!createdUserResult.Succeeded)
            {
                message = createdUserResult.Errors.First().Description;
                return RedirectToAction("LogIn", routeValues: new { message });
            }

            var resultAddLogin = await _userManager.AddLoginAsync(user, information);

            if(resultAddLogin.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false, information.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            message = "Ha ocurrido un error agregando el login";
            return RedirectToAction("LogIn", routeValues: new { message });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult LogIn(string? message = null)
        {
            if(message is not null)
            {
                ViewData["Message"] = message;
            }
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> LogIn(LoginViewModel viewModel)
        {
            if(!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var result = await _signInManager.PasswordSignInAsync(viewModel.Email, 
                viewModel.Password, viewModel.RememberMe, lockoutOnFailure: false);

            if(result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Usuario o contraseña incorrectos.");
                return View(viewModel);
            }
        }

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return RedirectToAction("LogIn");
        }


        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]  
        public async Task<IActionResult> Register(RegisterViewModel viewModel)
        {
            if(!ModelState.IsValid)
            {
                return View(viewModel);
            }

            var user = new IdentityUser()
            {
                Email = viewModel.Email,
                UserName = viewModel.Email
            };

            var result = await _userManager.CreateAsync(user, password: viewModel.Password);

            if(result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: true);
                return RedirectToAction("Index", "Home");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(viewModel);
            }


        }
    }
}
