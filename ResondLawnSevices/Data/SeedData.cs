using Microsoft.AspNetCore.Identity;
using ResondLawnSevices.Models;

namespace ResondLawnSevices.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDBContext>();

            // Seed Roles
            await SeedRoles(roleManager);

            // Seed Users
            await SeedUsers(userManager);

            // Seed Machines
            await SeedMachines(context);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            var roles = new List<string> { "Admin", "Operator", "Customer", "ConflictManager" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsers(UserManager<AppUser> userManager)
        {
            var users = new List<(string Name, string UserName, string Email, string Address, string Password)>
        {
            ("Admin User", "admin@gmail.com", "admin@gmail.com", "123 Admin St.", "AdminPassword123!"),
            ("Operator User", "operator@gmail.com", "operator@gmail.com", "456 Operator Ave.", "OperatorPassword123!"),
            ("Customer User", "customer@gmail.com", "customer@gmail.com", "789 Customer Blvd.", "CustomerPassword123!"),
            ("ConflictManager User", "conflictManager@gmail.com", "conflictManager@gmail.com", "789 ConflictManager Blvd.", "ConflictManagerPassword123!")
        };

            foreach (var (Name, UserName, Email, Address, Password) in users)
            {
                if (userManager.Users.All(u => u.UserName != UserName))
                {
                    var user = new AppUser { Name = Name, UserName = UserName, Email = Email, Address = Address };
                    var result = await userManager.CreateAsync(user, Password);

                    if (result.Succeeded)
                    {
                        // Assign roles based on username
                        if (UserName == "admin@gmail.com")
                        {
                            await userManager.AddToRoleAsync(user, "Admin");
                        }
                        else if (UserName == "operator@gmail.com")
                        {
                            await userManager.AddToRoleAsync(user, "Operator");
                        }
                        else if (UserName == "customer@gmail.com")
                        {
                            await userManager.AddToRoleAsync(user, "Customer");
                        }
                        else if (UserName == "conflictManager@gmail.com")
                        {
                            await userManager.AddToRoleAsync(user, "ConflictManager");
                        }
                    }
                }
            }
        }

        private static async Task SeedMachines(ApplicationDBContext context)
        {
            var machines = new List<Machine>
        {
            new Machine { Name = "TurboMower 224" },
            new Machine { Name = "PowerCutter 112" },
            new Machine { Name = "GrassChopper 3000" },
            new Machine { Name = "EcoMower 150" }
        };

            foreach (var machine in machines)
            {
                // Check if the machine already exists
                if (!context.Machines.Any(m => m.Name == machine.Name))
                {
                    await context.Machines.AddAsync(machine);
                }
            }

            // Save changes to the database
            await context.SaveChangesAsync();
        }
    }
}
