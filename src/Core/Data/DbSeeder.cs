namespace DotNetAdmin.Core.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Setting singleton
        if (!await db.Settings.AnyAsync())
        {
            db.Settings.Add(new Setting
            {
                Id = Guid.NewGuid().ToString(),
                Initial = "DA",
                Name = "DotNetAdmin",
                Theme = "Blue",
                FeTemplate = "agency-consulting-002-creative-agency",
                CreatedBy = "system",
                UpdatedBy = "system"
            });
            await db.SaveChangesAsync();
        }

        // Administrator role
        if (!await db.Roles.AnyAsync(r => r.Name == "Administrator"))
        {
            db.Roles.Add(new Role
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Administrator",
                GuardName = "web",
                Status = "Active",
                CreatedBy = "system",
                UpdatedBy = "system"
            });
            await db.SaveChangesAsync();
        }

        // Admin user (admin@admin.com / 12345678)
        if (!await db.Users.AnyAsync(u => u.Email == "admin@admin.com"))
        {
            var adminRole = await db.Roles.FirstAsync(r => r.Name == "Administrator");
            var user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Code = "ADM001",
                Name = "Administrator",
                Email = "admin@admin.com",
                Password = BCrypt.Net.BCrypt.HashPassword("12345678"),
                Status = "Active",
                Timezone = "UTC",
                CreatedBy = "system",
                UpdatedBy = "system"
            };
            db.Users.Add(user);
            await db.SaveChangesAsync();

            db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = adminRole.Id });
            await db.SaveChangesAsync();
        }
    }
}
