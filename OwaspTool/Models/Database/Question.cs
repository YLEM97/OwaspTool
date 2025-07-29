using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("Question")]
public partial class Question
{
    [Key]
    public int QuestionID { get; set; }

    public int CategoryID { get; set; }

    public string Text { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    [StringLength(50)]
    public string? InputType { get; set; }

    [InverseProperty("GoToQuestion")]
    public virtual ICollection<AnswerOption> AnswerOptionGoToQuestions { get; set; } = new List<AnswerOption>();

    [InverseProperty("Question")]
    public virtual ICollection<AnswerOption> AnswerOptionQuestions { get; set; } = new List<AnswerOption>();

    [ForeignKey("CategoryID")]
    [InverseProperty("Questions")]
    public virtual Category Category { get; set; } = null!;
}
