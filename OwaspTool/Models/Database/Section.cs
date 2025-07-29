using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("Section")]
public partial class Section
{
    [Key]
    public int SectionID { get; set; }

    [StringLength(50)]
    public string Number { get; set; } = null!;

    [StringLength(255)]
    public string Title { get; set; } = null!;

    [InverseProperty("Section")]
    public virtual ICollection<ASVSRequirement> ASVSRequirements { get; set; } = new List<ASVSRequirement>();
}
