using DotNetAdmin.Modules.Access.Role.Dtos;
using RoleEntity = DotNetAdmin.Core.Data.Entities.Role;
using PermissionEntity = DotNetAdmin.Core.Data.Entities.Permission;

namespace DotNetAdmin.Modules.Access.Role;

public class RoleService : IRoleService
{
    private readonly AppDbContext _db;

    public RoleService(AppDbContext db) => _db = db;

    public async Task<PaginationResult<RoleEntity>> GetAllAsync(RoleFilterDto filter)
    {
        var query = _db.Roles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.WhereCiLike(r => r.Name, filter.Search);

        if (!string.IsNullOrWhiteSpace(filter.Status))
            query = query.Where(r => r.Status == filter.Status);

        query = query.OrderBy(r => r.Name);
        return await PaginationHelper.PaginateAsync(query, filter.Page, filter.PageSize);
    }

    public async Task<RoleEntity> GetByIdAsync(string id) =>
        await _db.Roles.FirstOrDefaultAsync(r => r.Id == id)
            ?? throw new NotFoundAppException("Role not found.");

    public async Task<RoleEntity> CreateAsync(RoleCreateDto dto, string createdBy)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationAppException("Name is required.");
        if (await _db.Roles.AnyAsync(r => r.Name == dto.Name.Trim()))
            throw new ConflictAppException("Role name already exists.");

        var role = new RoleEntity
        {
            Id = Guid.NewGuid().ToString(),
            Name = dto.Name.Trim(),
            Status = dto.Status,
            Desc = dto.Desc?.Trim(),
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task<RoleEntity> UpdateAsync(string id, RoleUpdateDto dto, string updatedBy)
    {
        var role = await GetByIdAsync(id);
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ValidationAppException("Name is required.");
        if (await _db.Roles.AnyAsync(r => r.Name == dto.Name.Trim() && r.Id != id))
            throw new ConflictAppException("Role name already exists.");

        role.Name = dto.Name.Trim();
        role.Status = dto.Status;
        role.Desc = dto.Desc?.Trim();
        role.UpdatedBy = updatedBy;
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task DeleteAsync(string id)
    {
        var role = await GetByIdAsync(id);
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteSelectedAsync(IEnumerable<string> ids)
    {
        var roles = await _db.Roles.Where(r => ids.Contains(r.Id)).ToListAsync();
        _db.Roles.RemoveRange(roles);
        await _db.SaveChangesAsync();
    }

    public async Task<List<PermissionEntity>> GetPermissionsForRoleAsync(string id)
    {
        var rolePerms = await _db.RolePermissions
            .Include(rp => rp.Permission)
            .Where(rp => rp.RoleId == id)
            .Select(rp => rp.Permission)
            .ToListAsync();
        return rolePerms;
    }

    public async Task SyncPermissionsAsync(string roleId, IEnumerable<string> permissionIds)
    {
        var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync();
        _db.RolePermissions.RemoveRange(existing);

        foreach (var permId in permissionIds.Where(p => !string.IsNullOrWhiteSpace(p)))
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permId });

        await _db.SaveChangesAsync();
    }

    public async Task<List<PermissionEntity>> GetAllPermissionsAsync() =>
        await _db.Permissions.OrderBy(p => p.Name).ThenBy(p => p.Method).ToListAsync();
}
