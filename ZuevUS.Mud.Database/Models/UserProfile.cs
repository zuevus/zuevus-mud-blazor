using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZuevUS.Mud.Database.Enum;

namespace ZuevUS.Mud.Database.Models;

[Table("UserProfiles")]
public class UserProfile
{
    [Key]
    [MaxLength(450)]
    [Column("UserId")]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("UserName")]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("Role")]
    public UserRole Role { get; set; } = UserRole.User;

    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Column("LastLoginDate")]
    public DateTime? LastLoginDate { get; set; }
}
