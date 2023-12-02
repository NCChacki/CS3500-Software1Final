//Wall Object Class for Snake Game. Implemneted by Chase CANNNING and Jack MCINTYRE for CS3500, Fall of 2023

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;
using SnakeGame;


namespace Model
{
    [DataContract(Namespace ="")]
    public class Wall
    {

        /// <summary>
        /// Wall's ID
        /// </summary>
        [DataMember(Name = "ID")]
        public int wall { get; set; }

        /// <summary>
        /// Vector Indicating one end of the wall
        /// </summary>

        [DataMember(Name= "p1")]
        public Vector2D p1 { get; set; }

        /// <summary>
        /// Vector indicating one end of the wall
        /// </summary>
        [DataMember(Name = "p2")]
        public Vector2D p2 { get; set; }

        public Wall()
        { 
        }

        /// <summary>
        /// Wall constructor
        /// </summary>
        /// <param name="wall"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>

        [JsonConstructor]
        public Wall(int wall, Vector2D p1, Vector2D p2) : base()
        {
            this.wall = wall;
             this.p1 = p1;
             this.p2 = p2;


        }
    }
}
