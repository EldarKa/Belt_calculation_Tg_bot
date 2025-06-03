using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Models.Enums;
using Belt_calculation_Tg_bot.Models.State;

namespace Belt_calculation_Tg_bot.Models
{
    public class UserState
    {
        public AuthState State { get; set; } = AuthState.None;
        public CalculateState CalculateState { get; set; } = CalculateState.None;
        public AddBeltState AddBeltState { get; set; } = AddBeltState.None;
        public DeleteBeltState DeleteBeltState { get; set; } = DeleteBeltState.None;
        public Belt? SelectedBelt { get; set; }
        public Belt TempBelt { get; set; } = new Belt();
    }
}
