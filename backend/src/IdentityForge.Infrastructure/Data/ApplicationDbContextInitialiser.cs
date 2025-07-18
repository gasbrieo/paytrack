using IdentityForge.Domain.Users;
using IdentityForge.Infrastructure.Identity;

namespace IdentityForge.Infrastructure.Data;

public class ApplicationDbContextInitialiser(
    ILogger<ApplicationDbContextInitialiser> logger,
    IOptions<AdminUserOptions> options,
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager)
{
    private readonly AdminUserOptions _adminUser = options.Value;

    public async Task InitialiseAsync()
    {
        try
        {
            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initialising the database");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        var roles = new List<ApplicationRole>
        {
            new(Roles.Administrator),
            new(Roles.User)
        };

        foreach (var role in roles)
        {
            if (await roleManager.Roles.AllAsync(r => r.Name != role.Name))
            {
                await roleManager.CreateAsync(role);
            }
        }

        var administrator = new ApplicationUser { UserName = _adminUser.Email, Email = _adminUser.Email };

        if (await userManager.Users.AllAsync(u => u.UserName != administrator.UserName))
        {
            await userManager.CreateAsync(administrator, _adminUser.Password);
            await userManager.AddToRolesAsync(administrator, [Roles.Administrator]);
        }
    }
}
