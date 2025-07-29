using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("AnswerOption")]
public partial class AnswerOption
{
    [Key]
    public int AnswerOptionID { get; set; }

    public int QuestionID { get; set; }

    public int AnswerID { get; set; }

    public int? GoToQuestionID { get; set; }

    public int? DisplayOrder { get; set; }

    [InverseProperty("AnswerOption")]
    public virtual ICollection<ASVSReqAnswer> ASVSReqAnswers { get; set; } = new List<ASVSReqAnswer>();

    [ForeignKey("AnswerID")]
    [InverseProperty("AnswerOptions")]
    public virtual Answer Answer { get; set; } = null!;

    [InverseProperty("AnswerOption")]
    public virtual ICollection<GivenAnswer> GivenAnswers { get; set; } = new List<GivenAnswer>();

    [ForeignKey("GoToQuestionID")]
    [InverseProperty("AnswerOptionGoToQuestions")]
    public virtual Question? GoToQuestion { get; set; }

    [ForeignKey("QuestionID")]
    [InverseProperty("AnswerOptionQuestions")]
    public virtual Question Question { get; set; } = null!;
}
