using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HolidayOptimizer.Models
{

    public interface IHoldiayInterval
    {
   
        public double Start { get; set; }
        public double End { get; set; }
    }
}
