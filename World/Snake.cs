using SnakeGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Model
{

    public class Snake
    {
        int snake;
        string name;
        List<Vector2D> body;
        Vector2D dir;
        int score;
        bool died;
        bool alive;
        bool dc;
        bool join;

        [JsonConstructor]
        public Snake(int snake, string name, List<Vector2D> body,Vector2D dir,int score,bool died,bool alive,bool dc, bool join) 
        { 
            this.snake= snake;
            this.name= name;
            this.body= body;
            this.dir= dir;
            this.score= score;
            this.died= died;
            this.alive=alive;
            this.dc= dc;
            this.join= join;

        }

        public Snake() { }



    }
}
