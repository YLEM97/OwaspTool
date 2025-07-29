using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("ASVSRequirement")]
public partial class ASVSRequirement
{
    [Key]
    public int ASVSRequirementID { get; set; }

    public int ChapterID { get; set; }

    public int SectionID { get; set; }

    [StringLength(50)]
    public string Number { get; set; } = null!;

    public string Text { get; set; } = null!;

    [InverseProperty("ASVSRequirement")]
    public virtual ICollection<ASVSReqLevel> ASVSReqLevels { get; set; } = new List<ASVSReqLevel>();

    [ForeignKey("ChapterID")]
    [InverseProperty("ASVSRequirements")]
    public virtual Chapter Chapter { get; set; } = null!;

    [ForeignKey("SectionID")]
    [InverseProperty("ASVSRequirements")]
    public virtual Section Section { get; set; } = null!;
}
