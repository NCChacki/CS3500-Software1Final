using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;

namespace Model
{
    public class Power
    {
        public int power { get; set; }
        public Vector2D loc { get; set; }
        public bool died { get; set; }

        public Power()
        {

        }

        [JsonConstructor]
        public Power(int power, Vector2D loc, bool died) :base()
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        
        }

       
    }
}
