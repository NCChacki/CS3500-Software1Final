﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SnakeGame;


namespace Model
{

    public class Wall
    {
        int wall;
        Vector2D p1;
        Vector2D p2;


        [JsonConstructor]
         public Wall(int wall, Vector2D p1, Vector2D p2) 
        {
            this.wall = wall;
            this.p1 = p1;
            this.p2 = p2;
        
        }

       public Wall() { }
    }
}
