namespace DotNetAdmin.Modules.Access.Permission.Dtos;

public class PermissionFilterDto
{
    public string? Search { get; set; }
    public string? GuardName { get; set; }
    public string? Method { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
