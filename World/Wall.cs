using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Threading.Tasks;
using SnakeGame;


namespace Model
{

    public class Wall
    {
        public int wall { get; set; }
        public Vector2D p1 { get; set; }
        public Vector2D p2 { get; set; }





        public Wall()
        { 
        }


        [JsonConstructor]
        public Wall(int wall, Vector2D p1, Vector2D p2) : base()
        {
            this.wall = wall;
             this.p1 = p1;
             this.p2 = p2;


        }
    }
}
