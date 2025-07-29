using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("Survey")]
public partial class Survey
{
    [Key]
    public int SurveyID { get; set; }

    public string? Description { get; set; }

    [InverseProperty("Survey")]
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    [InverseProperty("Survey")]
    public virtual ICollection<SurveyInstance> SurveyInstances { get; set; } = new List<SurveyInstance>();
}
