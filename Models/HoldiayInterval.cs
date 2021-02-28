using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HolidayOptimizer.Models
{

    public class HoldiayInterval
    {
        public HoldiayInterval(double start, double end)
        {
            Start = start;
            End = end;
        }

        public double Start { get; set; }
        public double End { get; set; }
    }
}
