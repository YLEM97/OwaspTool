using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace OwaspTool.Models.Database;

[Table("User")]
[Index("Email", Name = "UQ__User__A9D105343DBAFF8D", IsUnique = true)]
public partial class User
{
    [Key]
    public Guid UserID { get; set; }

    [StringLength(255)]
    public string Email { get; set; } = null!;

    [InverseProperty("User")]
    public virtual ICollection<UserWebApp> UserWebApps { get; set; } = new List<UserWebApp>();
}
