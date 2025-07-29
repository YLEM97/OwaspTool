using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("Status")]
public partial class Status
{
    [Key]
    public int StatusID { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [InverseProperty("Status")]
    public virtual ICollection<SurveyCategoryStatus> SurveyCategoryStatuses { get; set; } = new List<SurveyCategoryStatus>();
}
