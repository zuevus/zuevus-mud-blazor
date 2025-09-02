using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZuevUS.Mud.Database.Enum;
public enum OrderStatus
{
    [Display(Name = "New")]
    New,
        
        [Display(Name = "In Progress")]
    InProgress,
        
        [Display(Name = "Completed")]
    Completed,
        
        [Display(Name = "Cancelled")]
    Cancelled
}
