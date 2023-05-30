using AutoMapper;
using DevStore.Identity.API.Models;
using DS.Email.Models;
using DS.Email.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using DevStore.Identity.API.Services;

namespace DevStore.Identity.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly AuthenticationService _authenticationService;
        public AuthController(IMapper mapper, 
            UserManager<User> userManager, 
            SignInManager<User> signInManager, 
            IEmailSender emailSender,
            IConfiguration configuration,
            AuthenticationService authenticationService)
        {
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _authenticationService = authenticationService;
        }

        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationModel userModel) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            }

            var user = _mapper.Map<User>(userModel);
            var result = await _userManager.CreateAsync(user, userModel.Password);

            if (result.Succeeded)
            {
                //var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                //var confirmationLink = Url.Action("ConfirmEmail", "Account", new { token, email = user.Email }, Request.Scheme);

                //var message = new Message(new string[] { user.Email }, "Confirmation email link", confirmationLink, null);

                //await _emailSender.SendEmailAsync(message);

                await _userManager.AddToRoleAsync(user, "Client");

                return Ok(_authenticationService.GetJwt(userModel.Email));
            }

            foreach (var error in result.Errors)
            {
                ModelState.TryAddModelError(error.Code, error.Description);
            }
            return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
        }

        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel userModel, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values);
            }

            var result = await _signInManager.PasswordSignInAsync(userModel.Email, userModel.Password, userModel.RememberMe, lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction("LoginTwoStep", new { userModel.Email, userModel.RememberMe, returnUrl });
            }

            if (result.IsLockedOut)
            {
                var forgotPassLink = Url.Action(nameof(ForgotPassword), "Account", new { }, Request.Scheme);

                var content = string.Format("Your account is locked out, to reset your password, please click this link: {0}", forgotPassLink);
                var message = new Message(new string[] { userModel.Email }, "Locked out account information", content, null);

                await _emailSender.SendEmailAsync(message);
                ModelState.AddModelError("", "The account is locked out");
                return BadRequest("The account is locked out");
            }

            if (result.Succeeded)
            {
                return Ok(_authenticationService.GetJwt(userModel.Email));
            }

            ModelState.AddModelError("", "Invalid Login Attempt");
            return Unauthorized(ModelState.Values.SelectMany(v => v.Errors));
        }

        [Route("login-two-steps")]
        [HttpGet]
        public async Task<IActionResult> LoginTwoStep(string email, bool rememberMe, string returnUrl = null)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest("");
            }

            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);
            if (!providers.Contains("Email"))
            {
                return BadRequest("");
            }

            var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            var message = new Message(new string[] { email }, "Authentication token", token, null);
            await _emailSender.SendEmailAsync(message);

            return Redirect(returnUrl);
        }

        [Route("login-two-steps")]
        [HttpPost]
        public async Task<IActionResult> LoginTwoStep(TwoStepModel twoStepModel, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.Values);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                return BadRequest(ModelState.Values);
            }

            var result = await _signInManager.TwoFactorSignInAsync("Email", twoStepModel.TwoFactorCode, twoStepModel.RememberMe, rememberClient: false);
            if (result.Succeeded)
            {
                return Redirect(returnUrl);
            }
            else if (result.IsLockedOut)
            {
                //Same logic as in the Login action
                ModelState.AddModelError("", "The account is locked out");
                return BadRequest(ModelState.Values);
            }
            else
            {
                ModelState.AddModelError("", "Invalid Login Attempt");
                return BadRequest(ModelState.Values);
            }
        }

        [Route("logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return Redirect("home");
        }

        [Route("forgot-password")]
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values);

            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);

            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var message = new Message(new string[] { user.Email }, "Reset password token", null);
            await _emailSender.SendEmailAsync(message);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [Route("forgot-password-confirmation")]
        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return Ok();
        }

        [Route("reset-password")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values);

            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation));

            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }

                return BadRequest(ModelState.Values);
            }

            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        [Route("reset-password")]
        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return Ok();
        }

        [Route("refresh-token")]
        [HttpPost]
        public IActionResult RefreshToken([FromBody] string refreshToken) 
        {
            return Ok();
        }
    }
}
