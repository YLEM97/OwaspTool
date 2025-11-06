using OwaspTool.Models.Database;

namespace OwaspTool.DTOs
{
    public class UserWebAppDTO
    {
        public UserWebAppDTO() { }
        public UserWebAppDTO(UserWebApp UWA)
        {
            UserWebAppID = UWA.WebApplicationID;
            WebAppID = UWA.WebApplicationID;
            Name = UWA.WebApplication?.Name ?? string.Empty;
            LevelID = UWA.LevelID;
            LevelAcronym = UWA.Level?.Acronym ?? string.Empty;
            LevelLabel = UWA.Level?.Label ?? string.Empty;
        }
        public int UserWebAppID { get; set; }
        public int WebAppID { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? LevelID { get; set; }
        public string LevelAcronym { get; set; } = string.Empty;
        public string LevelLabel { get; set; } = string.Empty;
    }
}
