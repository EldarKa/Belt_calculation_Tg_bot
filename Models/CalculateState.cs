using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Belt_calculation_Tg_bot.Models
{
    public enum CalculateState
    {
        None,
        AwaitingBeltChoice,
        AwaitingD1,
        AwaitingD2,
        AwaitingL,
        AwaitingP,
        AwaitingN
    }
}
