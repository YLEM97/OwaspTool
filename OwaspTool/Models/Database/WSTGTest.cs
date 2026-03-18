using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwaspTool.Models.Database;

[Table("WSTGTest")]
public partial class WSTGTest
{
    [Key]
    public int WSTGTestID { get; set; }

    public int WSTGChapterID { get; set; }

    [StringLength(50)]
    public string Number { get; set; } = null!; // es. 4.1.1

    [StringLength(50)]
    public string NumberWSTG { get; set; } = null!; // es. WSTG-INFO-01

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public string? Text { get; set; }

    public string? Link { get; set; }

    public bool? Active { get; set; }

    [ForeignKey("WSTGChapterID")]
    public virtual WSTGChapter WSTGChapter { get; set; } = null!;

    // Navigation collection per WSTGTestAnswer — necessario per la mappatura in OwaspToolContext
    public virtual ICollection<WSTGTestAnswer> WSTGTestAnswers { get; set; } = new HashSet<WSTGTestAnswer>();
}