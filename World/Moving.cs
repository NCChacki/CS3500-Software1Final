//Moving object class for Snake Game.Implemneted by Chase CANNNING and Jack MCINTYRE for CS3500, Fall of 2023
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    /// <summary>
    /// A object the repsersent a movement command made by a player. 
    /// </summary>
    public class Moving
    {
        //String form of the movemnet
       public string moving {  get;}

        /// <summary>
        /// Default moving constructor
        /// </summary>
        public Moving()
        { }

        /// <summary>
        /// Moving object that repersents the movemnt command of the string movement
        /// </summary>
        /// <param name="movement"></param>
        public Moving(string movement) :base()
        {
            this.moving = movement;

        }
    }
}
