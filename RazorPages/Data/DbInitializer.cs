using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMenu.Data
{
    public class DbInitializer
    {
     
        public static void Initialize(ApplicationDbContext context, UserManager<VmUser> userManager)
        {
            try
            {
                context.Database.EnsureCreated();
                
                //save default roles
                if (!context.Roles.Any())
                {
                    List<IdentityRole> defaultRoles = new List<IdentityRole>()
                {
                    new IdentityRole {Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = Guid.NewGuid().ToString()},
                    new IdentityRole {Name = "Manager", NormalizedName = "MANAGER", ConcurrencyStamp = Guid.NewGuid().ToString()},
                    new IdentityRole {Name = "Customer", NormalizedName = "CUSTOMER", ConcurrencyStamp = Guid.NewGuid().ToString()},
                    new IdentityRole {Name = "User", NormalizedName = "USER", ConcurrencyStamp = Guid.NewGuid().ToString()}
                };

                    foreach (var s in defaultRoles)
                    {
                        context.Roles.Add(s);
                    }
                    context.SaveChanges();
                }

                if (!context.Users.Any())
                {
                   AddUser(context, userManager);
                }

                //add default admin user
                ;
            }
            catch (Exception e)
            {
                throw;
                
            }
            
        }
        private static async void AddUser(ApplicationDbContext context, UserManager<VmUser> userManager)
        {

            var adminemai = "sefer.kilic@gmail.com";
            var user = new VmUser
            {
                UserName = adminemai,
                Email = adminemai,
                EmailConfirmed = true,
                LastName = "Kilic",
                FirstName="Sefer",
                TwoFactorEnabled = false,
                
            };
            
            var res =  await userManager.CreateAsync(user, "i3sGzeplBjQfnes4aHLG");
           
            var userRole = new IdentityUserRole<string>
            {
                UserId = context.Users.FirstOrDefault(r => r.Email == adminemai).Id,
                RoleId = context.Roles.FirstOrDefault(r => r.Name == "Admin").Id
            };
            context.UserRoles.Add(userRole);
            context.SaveChanges();

        }
    }
}
