using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SmartBotBlazorApp.Data
{
    public class SeedData
    {

        public static async Task Initialize(
            IServiceProvider serviceProvider,
            string password
            )
        {
            
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                var manager = await EnsureUser(serviceProvider, "admin@admin.pl", password);

            }
        }
        private static async Task<string> EnsureUser(
            IServiceProvider serviceProvider,
            string userName, string initPw)
        {
            var _userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await _userManager.FindByNameAsync(userName);

            if(user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    Email = userName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, initPw);

            }

            if (user == null)
                throw new Exception("User did not get created. Password Problem");

            return user.Id;
        }
            


    }


}
