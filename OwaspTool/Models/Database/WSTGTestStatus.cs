using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwaspTool.Models.Database;

[Table("WSTGTestStatus")]
public partial class WSTGTestStatus
{
    [Key]
    public int WSTGTestStatusID { get; set; }

    public int UserWebAppID { get; set; }

    public int WSTGTestID { get; set; }

    /// <summary>
    /// Status values:
    /// 0 = Not started
    /// 1 = Pass
    /// 2 = Issues
    /// 3 = Not Applicable
    /// </summary>
    public int Status { get; set; }

    public DateTime Modified { get; set; }

    public string? Notes { get; set; }

    public string? AiNotes { get; set; }

    [ForeignKey("UserWebAppID")]
    public virtual UserWebApp UserWebApp { get; set; } = null!;

    [ForeignKey("WSTGTestID")]
    public virtual WSTGTest WSTGTest { get; set; } = null!;
}