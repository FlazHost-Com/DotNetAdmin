using DotNetAdmin.Modules.Access.Permission.Dtos;
using PermissionEntity = DotNetAdmin.Core.Data.Entities.Permission;

namespace DotNetAdmin.Modules.Access.Permission;

public class PermissionService : IPermissionService
{
    private readonly AppDbContext _db;

    public PermissionService(AppDbContext db) => _db = db;

    public async Task<PaginationResult<PermissionEntity>> GetAllAsync(PermissionFilterDto filter)
    {
        var query = _db.Permissions.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
            query = query.WhereCiLike(p => p.Name, filter.Search);

        if (!string.IsNullOrWhiteSpace(filter.GuardName))
            query = query.Where(p => p.GuardName == filter.GuardName);

        if (!string.IsNullOrWhiteSpace(filter.Method))
            query = query.Where(p => p.Method == filter.Method.ToUpper());

        query = query.OrderBy(p => p.Name).ThenBy(p => p.Method);
        return await PaginationHelper.PaginateAsync(query, filter.Page, filter.PageSize);
    }

    public async Task<PermissionEntity> GetByIdAsync(string id) =>
        await _db.Permissions.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundAppException("Permission not found.");

    public async Task DeleteAsync(string id)
    {
        var perm = await GetByIdAsync(id);
        _db.Permissions.Remove(perm);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteSelectedAsync(IEnumerable<string> ids)
    {
        var perms = await _db.Permissions.Where(p => ids.Contains(p.Id)).ToListAsync();
        _db.Permissions.RemoveRange(perms);
        await _db.SaveChangesAsync();
    }
}
