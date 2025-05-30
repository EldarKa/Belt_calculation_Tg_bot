using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Belt_calculation_Tg_bot.Models.State;

namespace Belt_calculation_Tg_bot.Models
{
    public class UserSession
    {
        public string? Username { get; set; }
        public UserState State { get; set; } = new();
        public CalculationContour? Calculation { get; set; }
        public UserRole Role { get; set; } = UserRole.Guest;
        public string PreferredLanguage { get; set; } = "ru";
    }
}
