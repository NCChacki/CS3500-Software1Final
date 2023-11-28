//Power object class for Snake Game. Implemneted by Chase CANNNING and Jack MCINTYRE for CS3500, Fall of 2023

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
        /// <summary>
        /// the ID of the current power
        /// </summary>
        public int power { get; set; }
        /// <summary>
        /// location of the power
        /// </summary>
        public Vector2D loc { get; set; }
        /// <summary>
        /// true if the power has died on the current screen
        /// </summary>
        public bool died { get; set; }

        public Power()
        {

        }
        /// <summary>
        /// Power Constructor
        /// </summary>
        /// <param name="power"></param>
        /// <param name="loc"></param>
        /// <param name="died"></param>
        [JsonConstructor]
        public Power(int power, Vector2D loc, bool died) :base()
        {
            this.power = power;
            this.loc = loc;
            this.died = died;
        
        }

       
    }
}
