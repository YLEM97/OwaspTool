using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("Level")]
public partial class Level
{
    [Key]
    public int LevelID { get; set; }

    [StringLength(50)]
    public string Acronym { get; set; } = null!;

    [StringLength(255)]
    public string Label { get; set; } = null!;

    [InverseProperty("Level")]
    public virtual ICollection<ASVSReqLevel> ASVSReqLevels { get; set; } = new List<ASVSReqLevel>();

    [InverseProperty("Level")]
    public virtual ICollection<UserWebApp> UserWebApps { get; set; } = new List<UserWebApp>();
}
