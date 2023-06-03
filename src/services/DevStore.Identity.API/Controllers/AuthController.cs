using AutoMapper;
using DevStore.Identity.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using DevStore.Identity.API.Services;
using System;
using Microsoft.AspNetCore.Authorization;

namespace DevStore.Identity.API.Controllers
{
    [Authorize]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly AuthenticationService _authenticationService;
        public AuthController(IMapper mapper,  
            AuthenticationService authenticationService)
        {
            _mapper = mapper;
            _authenticationService = authenticationService;
        }

        [AllowAnonymous]
        [Route("register")]
        [HttpPost]
        public async Task<IActionResult> Register(UserRegistrationModel userModel) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

            var user = _mapper.Map<User>(userModel);
            var result = await _authenticationService.UserManager.CreateAsync(user, userModel.Password);

            if (result.Succeeded)
            {
                await _authenticationService.UserManager.AddToRoleAsync(user, UserRoles.User);

                return Ok("User created successfully!");
            }

            foreach (var error in result.Errors)
                ModelState.TryAddModelError(error.Code, error.Description);

            return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
        }

        [AllowAnonymous]
        [Route("register-admin")]
        [HttpPost]
        public async Task<IActionResult> RegisterAdmin(UserRegistrationModel userModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

            var user = _mapper.Map<User>(userModel);
            var result = await _authenticationService.UserManager.CreateAsync(user, userModel.Password);

            if (result.Succeeded)
            {
                await _authenticationService.UserManager.AddToRoleAsync(user, UserRoles.Admin);
                await _authenticationService.UserManager.AddToRoleAsync(user, UserRoles.User);

                return Ok("User created successfully!");
            }

            foreach (var error in result.Errors)
                ModelState.TryAddModelError(error.Code, error.Description);

            return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
        }

        [AllowAnonymous]
        [Route("login")]
        [HttpPost]
        public async Task<IActionResult> Login(UserLoginModel userModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

            var result = await _authenticationService.SignInManager.PasswordSignInAsync(userModel.Email, userModel.Password, userModel.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var token = await _authenticationService.GetJwt(userModel.Email);
                return Ok(token);
            }                

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", "The account is locked out");
                return Unauthorized("The account is locked out");
            }

            ModelState.AddModelError("", "Invalid login attempt");
            return Unauthorized(ModelState.Values.SelectMany(v => v.Errors));
        }

        [AllowAnonymous]
        [Route("refresh-token")]
        [HttpPost]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                ModelState.TryAddModelError("", "Invalid Refresh Token");
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            }

            var token = await _authenticationService.RefreshToken(refreshToken);

            if (token is null)
            {
                ModelState.TryAddModelError("", "Invalid Refresh Token");
                return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
            }

            return Ok(token);
        }

        [Authorize]
        [Route("logout")]
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _authenticationService.RevokeAccess();
            return NoContent();
        }
        
        [Route("test")]
        [HttpGet]
        public async Task<IActionResult> Test()
        {
            return Ok();
        }

        //[Route("forgot-password")]
        //[HttpPost]
        //public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

        //    var user = await _authenticationService.UserManager.FindByEmailAsync(forgotPasswordModel.Email);

        //    if (user == null)
        //        return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

        //    var token = await _authenticationService.UserManager.GeneratePasswordResetTokenAsync(user);

        //    //var message = new Message(new string[] { user.Email }, "Reset password token", null);
        //    //await _emailSender.SendEmailAsync(message);

        //    return Ok();
        //}

        //[Route("reset-password")]
        //[HttpPost]
        //public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState.Values.SelectMany(v => v.Errors));

        //    var user = await _authenticationService.UserManager.FindByEmailAsync(resetPasswordModel.Email);
        //    if (user == null)
        //        BadRequest(ModelState.Values.SelectMany(v => v.Errors));

        //    var resetPassResult = await _authenticationService.UserManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
        //    if (!resetPassResult.Succeeded)
        //    {
        //        foreach (var error in resetPassResult.Errors)
        //            ModelState.TryAddModelError(error.Code, error.Description);

        //        return BadRequest(ModelState.Values.SelectMany(v => v.Errors));
        //    }

        //    return Ok();
        //}

    }
}
