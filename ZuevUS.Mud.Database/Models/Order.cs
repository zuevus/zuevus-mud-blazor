using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZuevUS.Mud.Database.Enum;

namespace ZuevUS.Mud.Database.Models;
[Table("Orders")]
public class Order
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    [Column("Title")]
    public string Title { get; set; } = string.Empty;

    [Column("Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Column("OrderType")]
    public OrderType Type { get; set; }

    [Required]
    [Column("Status")]
    public OrderStatus Status { get; set; } = OrderStatus.New;

    [Required]
    [Column("Price", TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    [Column("CreatedDate")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Required]
    [Column("Deadline")]
    public DateTime Deadline { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("ClientName")]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [Column("ClientEmail")]
    public string ClientEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(450)]
    [Column("CreatedByUserId")]
    public string CreatedByUserId { get; set; } = string.Empty;
}
