using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("SurveyInstance")]
public partial class SurveyInstance
{
    [Key]
    public int SurveyInstanceID { get; set; }

    public int UserWebAppID { get; set; }

    public int SurveyID { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    [InverseProperty("SurveyInstance")]
    public virtual ICollection<GivenAnswer> GivenAnswers { get; set; } = new List<GivenAnswer>();

    [ForeignKey("SurveyID")]
    [InverseProperty("SurveyInstances")]
    public virtual Survey Survey { get; set; } = null!;

    [InverseProperty("SurveyInstance")]
    public virtual ICollection<SurveyCategoryStatus> SurveyCategoryStatuses { get; set; } = new List<SurveyCategoryStatus>();

    [ForeignKey("UserWebAppID")]
    [InverseProperty("SurveyInstances")]
    public virtual UserWebApp UserWebApp { get; set; } = null!;
}
