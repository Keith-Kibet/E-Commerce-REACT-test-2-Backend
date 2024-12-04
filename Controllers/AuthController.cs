using EcommApp.Models.DTO;
using EcommApp.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly ITokenRepository tokenRepository;
        private readonly UserRepository userRepository;

        public AuthController(UserManager<IdentityUser> userManager, ITokenRepository tokenRepository,UserRepository userRepository)
        {
            this.userManager = userManager;
            this.tokenRepository = tokenRepository;
            this.userRepository = userRepository;
        }

        //Post: /api/Auth/Register
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto registerRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var identityUser = new IdentityUser
            {
                UserName = registerRequestDto.Username,
                Email = registerRequestDto.Username,
            };

         
             
            var identityResult = await userManager.CreateAsync(identityUser, registerRequestDto.Password);

            if(identityResult.Succeeded)
            {
                //Add Roles to the user
               
                  identityResult =   await userManager.AddToRoleAsync(identityUser, "Reader");



                if (identityResult.Succeeded)
                {
                    return Ok(new { Message = "User was registered! Please login!" });
                }

            }

            var errors = identityResult.Errors.Select(e => e.Description);
            return BadRequest(new { Errors = errors });

        }

        // POST: /api/Auth/Login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            var user =  await userManager.FindByEmailAsync(loginRequestDto.Username);

            if(user != null)
            {
              var checkPasswordResult = await userManager.CheckPasswordAsync(user, loginRequestDto.Password);

                if (checkPasswordResult)
                {
                    //Get Roles for this user
                   var roles =  await userManager.GetRolesAsync(user);

                    if(roles != null) 
                    {
                        //Create Token 
                      var jwtToken =   tokenRepository.CreateJwtToken(user, roles.ToList());

                        var response = new LoginResponseDto
                        {
                            UserId = user.Id,
                            Email = user.Email,
                            JwtToken = jwtToken
                        };

                        return Ok(response);
                    }
                    
                }

            }

            return BadRequest(new { message = "Username or Password is incorrect !" });


        }

        [HttpPost]
        [Route("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await userRepository.SendPasswordResetEmailAsync(model.Email, "http://localhost:3000/resetPassword");

            if (!result)
                return BadRequest(new { message = "User not found" });

            return Ok(new { message = "Password reset email sent successfully" });


        }


        [HttpPost]
        [Route("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
               

            var result = await userRepository.ResetPasswordAsync(model);
            if (result.Succeeded)
            {
                return Ok(new { message = "Password reset successfully" });
            }

            return BadRequest(result.Errors);
        }






    }
}
