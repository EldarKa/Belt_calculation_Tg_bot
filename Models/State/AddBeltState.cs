using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Belt_calculation_Tg_bot.Models.Enums
{
    public enum AddBeltState
    {
        None,
        AwaitingName,
        AwaitingWeight,
        AwaitingK1,
        AwaitingK2,
        AwaitingK3,
        AwaitingK4,
        AwaitingL0,
        Completed,
        Deleting
    }
}
