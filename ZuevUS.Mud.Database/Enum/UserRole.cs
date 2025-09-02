using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZuevUS.Mud.Database.Enum;
public enum UserRole
{
    [Display(Name = "Administrator")]
    Admin,

    [Display(Name = "User")]
    User
}
