using DotNetAdmin.Core.Filters;
using DotNetAdmin.Modules.Access.Permission.Dtos;

namespace DotNetAdmin.Modules.Access.Permission;

[Authorize(AuthenticationSchemes = "WebCookie")]
[ServiceFilter(typeof(AdminViewDataFilter))]
[ServiceFilter(typeof(AccessFilterAttribute))]
public class PermissionWebController : Controller
{
    private readonly IPermissionService _permissionService;

    public PermissionWebController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet("/admin/v1/access/permission", Name = "admin.v1.access.permission.index")]
    public async Task<IActionResult> Index([FromQuery] PermissionFilterDto filter)
    {
        var result = await _permissionService.GetAllAsync(filter);
        ViewBag.Title = "Permission Management";
        ViewBag.Result = result;
        ViewBag.Filter = filter;
        return View("~/Views/AccessPermission/Index.cshtml");
    }

    [HttpDelete("/admin/v1/access/permission/{id}", Name = "admin.v1.access.permission.delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _permissionService.DeleteAsync(id);
            HttpContext.Session.SetSuccess("Delete Permission Success.");
        }
        catch (AppException ex)
        {
            HttpContext.Session.SetError(ex.Message);
        }
        return RedirectToRoute("admin.v1.access.permission.index");
    }

    [HttpDelete("/admin/v1/access/permission/delete_selected", Name = "admin.v1.access.permission.delete_selected")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSelected([FromForm] string ids)
    {
        try
        {
            var idList = (ids ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
            await _permissionService.DeleteSelectedAsync(idList);
            HttpContext.Session.SetSuccess("Delete Permission Success.");
        }
        catch (AppException ex)
        {
            HttpContext.Session.SetError(ex.Message);
        }
        return RedirectToRoute("admin.v1.access.permission.index");
    }
}
