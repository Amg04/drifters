using DAL.Data;
using DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Utilities;

namespace DALProject.DBInitializer
{
    public class DBInitializer : IDBInitializer
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DriftersDBContext _db;

        public DBInitializer(UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            DriftersDBContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public async Task Initialize()
        {
            // migration if they are not applied
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("DB Initialization error: " + ex.Message);
            }


            // create roles if they are not applied

            if (!await _roleManager.RoleExistsAsync(SD.ObserverRole))
            {
                // Create CurrentRole
                await _roleManager.CreateAsync(new IdentityRole(SD.AdminRole));
                await _roleManager.CreateAsync(new IdentityRole(SD.ManagerRole));
                await _roleManager.CreateAsync(new IdentityRole(SD.ObserverRole));

                // if roles are created , then we will create admin User as well
                var result = await _userManager.CreateAsync(new AppUser
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    PhoneNumber = "1234567890",
                }, "Admin123*");

                if (result.Succeeded)
                {
                    AppUser user = await _userManager.FindByEmailAsync("admin@gmail.com");
                    await _userManager.AddToRoleAsync(user, SD.AdminRole);
                }
            }

            return;
        }
    }
}
