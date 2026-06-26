using DotNetAdmin.Core.Data.Entities;

namespace DotNetAdmin.Modules.Setting;

public class SettingUpdateDto
{
    public string? Initial { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Email { get; set; }
    public string? Copyright { get; set; }
    public string? Theme { get; set; }
    public string? FeTemplate { get; set; }
    public IFormFile? IconFile { get; set; }
    public IFormFile? LogoFile { get; set; }
    public IFormFile? FaviconFile { get; set; }
    public IFormFile? LoginImageFile { get; set; }
}

public interface ISettingService
{
    Task<Core.Data.Entities.Setting> GetAsync();
    Task<Core.Data.Entities.Setting> UpdateAsync(SettingUpdateDto dto);
}
