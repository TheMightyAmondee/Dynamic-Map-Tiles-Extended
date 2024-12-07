using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMT.Data
{
    public record SecondUpdateData
    {
        public int Loops { get; set; } = 0;

        public int Value { get; set; } = 0;

        public float FloatValue { get; set; } = 0f;

        public Farmer Who { get; set; }

        public bool IsHealth { get; set; } = true;
    }
}
