using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZuevUS.Mud.Database.Enum;
public enum OrderType
{
    [Display(Name = "Website Development")]
    WebsiteDevelopment,

    [Display(Name = "Mobile App")]
    MobileApp,

    [Display(Name = "API Development")]
    ApiDevelopment,

    [Display(Name = "Database Design")]
    DatabaseDesign,

    [Display(Name = "System Maintenance")]
    SystemMaintenance,

    [Display(Name = "Bug Fixing")]
    BugFixing,

    [Display(Name = "Consultation")]
    Consultation
}
