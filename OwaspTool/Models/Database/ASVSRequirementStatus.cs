using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwaspTool.Models.Database;

[Table("ASVSRequirementStatus")]
public partial class ASVSRequirementStatus
{
    [Key]
    public int ASVSRequirementStatusID { get; set; }

    public int UserWebAppID { get; set; }

    public int ASVSRequirementID { get; set; }

    /// <summary>
    /// Status values:
    /// 0 = NotImplemented
    /// 1 = Implemented
    /// 2 = NotApplicable
    /// </summary>
    public int Status { get; set; }

    public DateTime Modified { get; set; }

    // New: notes per user + requirement (nullable)
    public string? Notes { get; set; }

    // New: AI conversation/notes for this user + requirement (nullable)
    public string? AiNotes { get; set; }

    [ForeignKey("UserWebAppID")]
    public virtual UserWebApp UserWebApp { get; set; } = null!;

    [ForeignKey("ASVSRequirementID")]
    public virtual ASVSRequirement ASVSRequirement { get; set; } = null!;
}