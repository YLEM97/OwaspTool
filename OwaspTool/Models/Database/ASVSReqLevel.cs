using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("ASVSReqLevel")]
public partial class ASVSReqLevel
{
    [Key]
    public int ASVSReqLevelID { get; set; }

    public int ASVSRequirementID { get; set; }

    public int LevelID { get; set; }

    public bool? Active { get; set; }

    [InverseProperty("ASVSReqLevel")]
    public virtual ICollection<ASVSReqAnswer> ASVSReqAnswers { get; set; } = new List<ASVSReqAnswer>();

    [ForeignKey("ASVSRequirementID")]
    [InverseProperty("ASVSReqLevels")]
    public virtual ASVSRequirement ASVSRequirement { get; set; } = null!;

    [ForeignKey("LevelID")]
    [InverseProperty("ASVSReqLevels")]
    public virtual Level Level { get; set; } = null!;
}
