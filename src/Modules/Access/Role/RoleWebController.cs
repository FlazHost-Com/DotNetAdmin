using DotNetAdmin.Core.Filters;
using DotNetAdmin.Modules.Access.Role.Dtos;

namespace DotNetAdmin.Modules.Access.Role;

[Authorize(AuthenticationSchemes = "WebCookie")]
[ServiceFilter(typeof(AdminViewDataFilter))]
[ServiceFilter(typeof(AccessFilterAttribute))]
public class RoleWebController : Controller
{
    private readonly IRoleService _roleService;

    public RoleWebController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("/admin/v1/access/role", Name = "admin.v1.access.role.index")]
    public async Task<IActionResult> Index([FromQuery] RoleFilterDto filter)
    {
        var result = await _roleService.GetAllAsync(filter);
        ViewBag.Title = "Role Management";
        ViewBag.Result = result;
        ViewBag.Filter = filter;
        return View("~/Views/AccessRole/Index.cshtml");
    }

    [HttpGet("/admin/v1/access/role/create", Name = "admin.v1.access.role.create")]
    public IActionResult Create()
    {
        var (errors, old) = HttpContext.Session.GetFieldErrors();
        ViewBag.Title = "Create Role";
        ViewBag.FieldErrors = errors;
        ViewBag.OldInput = old;
        return View("~/Views/AccessRole/Create.cshtml");
    }

    [HttpPost("/admin/v1/access/role", Name = "admin.v1.access.role.store")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Store([FromForm] RoleCreateDto dto)
    {
        try
        {
            var actorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await _roleService.CreateAsync(dto, actorId);
            HttpContext.Session.SetSuccess("Create Role Success.");
            return RedirectToRoute("admin.v1.access.role.index");
        }
        catch (AppException ex)
        {
            HttpContext.Session.SetFieldErrors(
                ex.Errors ?? new Dictionary<string, string> { ["_"] = ex.Message },
                new Dictionary<string, string> { ["name"] = dto.Name, ["status"] = dto.Status, ["desc"] = dto.Desc ?? "" });
            HttpContext.Session.SetError(ex.Message);
            return RedirectToRoute("admin.v1.access.role.create");
        }
    }

    [HttpGet("/admin/v1/access/role/{id}/edit", Name = "admin.v1.access.role.edit")]
    public async Task<IActionResult> Edit(string id)
    {
        var role = await _roleService.GetByIdAsync(id);
        var (errors, old) = HttpContext.Session.GetFieldErrors();
        ViewBag.Title = "Edit Role";
        ViewBag.Role = role;
        ViewBag.FieldErrors = errors;
        ViewBag.OldInput = old;
        return View("~/Views/AccessRole/Edit.cshtml");
    }

    [HttpPut("/admin/v1/access/role/{id}", Name = "admin.v1.access.role.update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string id, [FromForm] RoleUpdateDto dto)
    {
        try
        {
            var actorId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await _roleService.UpdateAsync(id, dto, actorId);
            HttpContext.Session.SetSuccess("Update Role Success.");
            return RedirectToRoute("admin.v1.access.role.index");
        }
        catch (AppException ex)
        {
            HttpContext.Session.SetFieldErrors(
                ex.Errors ?? new Dictionary<string, string> { ["_"] = ex.Message },
                new Dictionary<string, string> { ["name"] = dto.Name, ["status"] = dto.Status, ["desc"] = dto.Desc ?? "" });
            HttpContext.Session.SetError(ex.Message);
            return RedirectToRoute("admin.v1.access.role.edit", new { id });
        }
    }

    [HttpDelete("/admin/v1/access/role/{id}", Name = "admin.v1.access.role.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _roleService.DeleteAsync(id);
            HttpContext.Session.SetSuccess("Delete Role Success.");
        }
        catch (AppException ex)
        {
            HttpContext.Session.SetError(ex.Message);
        }
        return RedirectToRoute("admin.v1.access.role.index");
    }

    [HttpDelete("/admin/v1/access/role/delete_selected", Name = "admin.v1.access.role.delete_selected")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSelected([FromForm] string ids)
    {
        try
        {
            var idList = (ids ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            await _roleService.DeleteSelectedAsync(idList);
            HttpContext.Session.SetSuccess("Delete Role Success.");
        }
        catch (AppException ex)
        {
            HttpContext.Session.SetError(ex.Message);
        }
        return RedirectToRoute("admin.v1.access.role.index");
    }

    // ── Role → Permission management ────────────────────────────────────────────

    [HttpGet("/admin/v1/access/role/{id}/permissions", Name = "admin.v1.access.role.permission.index")]
    public async Task<IActionResult> Permissions(string id)
    {
        var role = await _roleService.GetByIdAsync(id);
        var allPerms = await _roleService.GetAllPermissionsAsync();
        var assigned = await _roleService.GetPermissionsForRoleAsync(id);
        var assignedIds = assigned.Select(p => p.Id).ToHashSet();

        ViewBag.Title = $"Permissions — {role.Name}";
        ViewBag.Role = role;
        ViewBag.AllPermissions = allPerms;
        ViewBag.AssignedIds = assignedIds;
        return View("~/Views/AccessRole/Permissions.cshtml");
    }

    [HttpPut("/admin/v1/access/role/{id}/permissions", Name = "admin.v1.access.role.permission.sync")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SyncPermissions(string id, [FromForm] string permissions)
    {
        try
        {
            var permIds = (permissions ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            await _roleService.SyncPermissionsAsync(id, permIds);
            HttpContext.Session.SetSuccess("Assign Permission Success.");
        }
        catch (AppException ex)
        {
            HttpContext.Session.SetError(ex.Message);
        }
        return RedirectToRoute("admin.v1.access.role.permission.index", new { id });
    }
}
