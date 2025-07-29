using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("Chapter")]
public partial class Chapter
{
    [Key]
    public int ChapterID { get; set; }

    [StringLength(50)]
    public string Number { get; set; } = null!;

    [StringLength(255)]
    public string Title { get; set; } = null!;

    [InverseProperty("Chapter")]
    public virtual ICollection<ASVSRequirement> ASVSRequirements { get; set; } = new List<ASVSRequirement>();
}
