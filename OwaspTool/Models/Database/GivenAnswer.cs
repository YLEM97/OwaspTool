using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("GivenAnswer")]
public partial class GivenAnswer
{
    [Key]
    public int GivenAnswerID { get; set; }

    public int SurveyInstanceID { get; set; }

    public int? AnswerOptionID { get; set; }

    public string? OtherText { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? Modified { get; set; }

    public DateOnly? Date { get; set; }

    [ForeignKey("AnswerOptionID")]
    [InverseProperty("GivenAnswers")]
    public virtual AnswerOption? AnswerOption { get; set; }

    [ForeignKey("SurveyInstanceID")]
    [InverseProperty("GivenAnswers")]
    public virtual SurveyInstance SurveyInstance { get; set; } = null!;
}
