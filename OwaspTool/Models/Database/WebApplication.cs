using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("WebApplication")]
public partial class WebApplication
{
    [Key]
    public int WebApplicationID { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [InverseProperty("WebApplication")]
    public virtual ICollection<UserWebApp> UserWebApps { get; set; } = new List<UserWebApp>();
}
