using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("Answer")]
public partial class Answer
{
    [Key]
    public int AnswerID { get; set; }

    [StringLength(50)]
    public string Acronym { get; set; } = null!;

    [StringLength(255)]
    public string Text { get; set; } = null!;

    [InverseProperty("Answer")]
    public virtual ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}
