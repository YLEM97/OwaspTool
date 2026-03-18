using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OwaspTool.Models.Database;

[Table("WSTGChapter")]
public partial class WSTGChapter
{
    [Key]
    public int WSTGChapterID { get; set; }

    [StringLength(50)]
    public string Number { get; set; } = null!;

    [StringLength(255)]
    public string Title { get; set; } = null!;

    public virtual ICollection<WSTGTest> WSTGTests { get; set; } = new HashSet<WSTGTest>();
}