using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("Category")]
public partial class Category
{
    [Key]
    public int CategoryID { get; set; }

    public int SurveyID { get; set; }

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    [InverseProperty("Category")]
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    [ForeignKey("SurveyID")]
    [InverseProperty("Categories")]
    public virtual Survey Survey { get; set; } = null!;

    [InverseProperty("Category")]
    public virtual ICollection<SurveyCategoryStatus> SurveyCategoryStatuses { get; set; } = new List<SurveyCategoryStatus>();
}
