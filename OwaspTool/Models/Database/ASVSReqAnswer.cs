using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("ASVSReqAnswer")]
public partial class ASVSReqAnswer
{
    [Key]
    public int ASVSReqAnswerID { get; set; }

    public int ASVSReqLevelID { get; set; }

    public int AnswerOptionID { get; set; }

    public int? DisplayOrder { get; set; }

    [ForeignKey("ASVSReqLevelID")]
    [InverseProperty("ASVSReqAnswers")]
    public virtual ASVSReqLevel ASVSReqLevel { get; set; } = null!;

    [ForeignKey("AnswerOptionID")]
    [InverseProperty("ASVSReqAnswers")]
    public virtual AnswerOption AnswerOption { get; set; } = null!;
}
