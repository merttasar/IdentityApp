using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityApp.Data
{
    public class SeedData
    {
        private const string adminUser = "admin";
        private const string adminPassword = "123456";
        public static async void IdentityTestUser(IApplicationBuilder app)
        {
            var context = app.ApplicationServices.CreateScope().ServiceProvider.GetService<IdentityContext>();


            var UserManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var RolesManager = app.ApplicationServices.CreateScope().ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
            if (context != null)
            {
                if (context.Database.GetPendingMigrations().Any())
                {
                    context.Database.Migrate();

                }
            }
            var user = await UserManager.FindByNameAsync(adminUser);
            if (user == null)
            {
                user = new AppUser { FullName = "merttasar", UserName = adminUser, Email = "info@merttasar.com", PhoneNumber = "4444444444" };
                await UserManager.CreateAsync(user, adminPassword);

                var role = new AppRole { Name = "admin" };
                await RolesManager.CreateAsync(role);

                await UserManager.AddToRoleAsync(user, "admin");

            }

        }
    }
}
