using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Belt_calculation_Tg_bot.Models
{
    public class Belt
    {
        public int Id { get; set; }               // ID ремня в базе
        public string Name { get; set; }          // Название ремня
        public double Weight { get; set; }        // Вес
        public double K1 { get; set; }            // Коэффициент 1
        public double K2 { get; set; }            // Коэффициент 2
        public double K3 { get; set; }            // Коэффициент 3
        public double K4 { get; set; }            // Коэффициент 4
        public double L0 { get; set; }            // Исходная длина
    }
}
