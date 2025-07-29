using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("UserWebApp")]
public partial class UserWebApp
{
    [Key]
    public int UserWebAppID { get; set; }

    public Guid UserID { get; set; }

    public int WebApplicationID { get; set; }

    public int? LevelID { get; set; }

    public string? Note { get; set; }

    [ForeignKey("LevelID")]
    [InverseProperty("UserWebApps")]
    public virtual Level? Level { get; set; }

    [InverseProperty("UserWebApp")]
    public virtual ICollection<SurveyInstance> SurveyInstances { get; set; } = new List<SurveyInstance>();

    [ForeignKey("UserID")]
    [InverseProperty("UserWebApps")]
    public virtual User User { get; set; } = null!;

    [ForeignKey("WebApplicationID")]
    [InverseProperty("UserWebApps")]
    public virtual WebApplication WebApplication { get; set; } = null!;
}
