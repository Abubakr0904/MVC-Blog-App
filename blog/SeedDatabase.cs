using blog2.Data;
using blog2.Entity;
using Microsoft.AspNetCore.Identity;

public class SeedDatabase : BackgroundService
{
    private readonly ILogger<SeedDatabase> _logger;
    private readonly IServiceProvider _serviceProvider;
    private UserManager<User> _userManager;
    private RoleManager<IdentityRole> _roleManager;
    private BlogAppDbContext _ctx;

    public SeedDatabase(ILogger<SeedDatabase> logger,
                        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateAsyncScope();
        _userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        _ctx = scope.ServiceProvider.GetRequiredService<BlogAppDbContext>();

        try
        {
            _ctx.Database.EnsureCreated();

            var adminRole = new IdentityRole("Admin");

            // if there is no role called "Admin": 
            if(!_ctx.Roles.Any())
            {
                // create role
                await _roleManager.CreateAsync(adminRole);
                _logger.LogInformation($"Role created: {adminRole}");
            }
            // else no need for any action related to role
            _logger.LogInformation($"No need to create role. Role {adminRole} already exists.");

            // if there is no admin user:
            if(!_ctx.Users.Any(u => u.UserName == "admin"))
            {
                // create admin user
                var adminUser = new User()
                {
                    UserName = "admin",
                    Email = "admin@blog.com"
                };
                var result = await _userManager.CreateAsync(adminUser, "password");
                _logger.LogInformation($"User created: {adminUser}");

                // add user to admin role
                await _userManager.AddToRoleAsync(adminUser, adminRole.Name);
                _logger.LogInformation($"User {adminUser.UserName} is added to role {adminRole.Name}");
            }
            else
            {
                // else no need for any action
                _logger.LogInformation("admin user already exist in db. No need to seed.");
            }
        }
        catch (System.Exception e)
        {
            _logger.LogError($"error while seeding users and roles. Error: {e.Message}");
        }
    }
}