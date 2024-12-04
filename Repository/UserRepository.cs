using EcommApp.Models.DTO;
using Microsoft.AspNetCore.Identity;
using System.Net;

namespace EcommApp.Repository
{
    public class UserRepository
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly IEmailService emailService;
        private readonly RoleManager<IdentityRole> roleManager;


        public UserRepository(UserManager<IdentityUser> userManager, IEmailService emailService, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.emailService = emailService;
            this.roleManager = roleManager;
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string resetPasswordUrl)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = $"{resetPasswordUrl}?token={WebUtility.UrlEncode(token)}&email={WebUtility.UrlEncode(email)}";

            await emailService.SendEmailAsync(email, "Reset Your Password", $"Please reset your password by clicking <a href=\"{resetLink}\">here</a>.");

            return true;
        }

        public async Task<IdentityResult> ResetPasswordAsync(ResetPasswordRequestDto model)
        {
            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Optionally, you might want to avoid disclosing that a user was not found
                // and return a generic success result instead.
                return IdentityResult.Success;
            }

            return await userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
        }



        public async Task SeedUserAsync()
        {
            var userName = "benjamin.leyian@agilebiz.co.ke";
            var password = "Password123";
            var roleName = "Writer"; // Define the role name

            // Ensure the role exists
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // Check if the user exists
            var user = await userManager.FindByEmailAsync(userName);
            if (user == null)
            {
                user = new IdentityUser { UserName = userName, Email = userName };
                var createUserResult = await userManager.CreateAsync(user, password);

                if (createUserResult.Succeeded)
                {
                    // Assign the role to the user
                    await userManager.AddToRoleAsync(user, roleName);
                }
                else
                {
                    // Handle any errors
                    throw new InvalidOperationException("Failed to create seed user: " + string.Join(", ", createUserResult.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // If the user already exists, ensure they have the role
                if (!await userManager.IsInRoleAsync(user, roleName))
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
        }
    }




}
