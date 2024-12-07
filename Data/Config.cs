using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMT.Data
{
    public class Config
    {
        public bool Enabled { get; set; } = true;

        public bool TriggerDuringEvents { get; set; } = false;
    }
}
