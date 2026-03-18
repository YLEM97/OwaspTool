using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwaspTool.Models.Database;

[Table("WSTGTestAnswer")]
public partial class WSTGTestAnswer
{
    [Key]
    public int WSTGTestAnswerID { get; set; }

    public int WSTGTestID { get; set; }

    public int AnswerOptionID { get; set; }

    public int? DisplayOrder { get; set; }

    [ForeignKey("WSTGTestID")]
    public virtual WSTGTest WSTGTest { get; set; } = null!;

    [ForeignKey("AnswerOptionID")]
    public virtual AnswerOption AnswerOption { get; set; } = null!;
}