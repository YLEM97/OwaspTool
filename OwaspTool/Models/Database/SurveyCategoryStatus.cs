using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("SurveyCategoryStatus")]
public partial class SurveyCategoryStatus
{
    [Key]
    public int SurveyCategoryStatusID { get; set; }

    public int SurveyInstanceID { get; set; }

    public int CategoryID { get; set; }

    public int StatusID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastSavedAt { get; set; }

    [ForeignKey("CategoryID")]
    [InverseProperty("SurveyCategoryStatuses")]
    public virtual Category Category { get; set; } = null!;

    [ForeignKey("StatusID")]
    [InverseProperty("SurveyCategoryStatuses")]
    public virtual Status Status { get; set; } = null!;

    [ForeignKey("SurveyInstanceID")]
    [InverseProperty("SurveyCategoryStatuses")]
    public virtual SurveyInstance SurveyInstance { get; set; } = null!;
}
