namespace DotNetAdmin.Modules.Access.User.Dtos;

public class UserUpdateDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Timezone { get; set; } = "UTC";
    public string? Password { get; set; }
    public string? PasswordConfirmation { get; set; }
    public string Status { get; set; } = "Active";
    public bool Blocked { get; set; } = false;
    public string? BlockedReason { get; set; }
    public IFormFile? Picture { get; set; }
    public List<string> Roles { get; set; } = [];
}
