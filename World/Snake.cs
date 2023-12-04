//Snake Object Class for Snake Game. Implemneted by Chase CANNNING and Jack MCINTYRE for CS3500, Fall of 2023

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
        /// <summary>
        /// ID of the snake
        /// </summary>
        public int snake { get; set; }
        /// <summary>
        /// Name of the player the snake repersents 
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// List of vectors repersenting the segments of the snakes body.
        /// </summary>
        public List<Vector2D> body { get; set; }
        /// <summary>
        /// Direction vector of the snake. 
        /// </summary>
        public Vector2D dir { get; set; }
        /// <summary>
        /// Score of the snake
        /// </summary>
        public int score { get; set; }
        /// <summary>
        /// Bool repersenting if the snake has died on the current frame
        /// </summary>
        public  bool died { get; set; }
        /// <summary>
        /// Bool to indicate the snake is still alive and loc should be updated on screen
        /// </summary>
        public bool alive { get; set; }
        /// <summary>
        /// If the player has disconnected on the current frame this will be set to true
        /// </summary>
        public bool dc { get; set; }
        /// <summary>
        /// If the player has joined on the current frame then this is set to true
        /// </summary>
        public bool join { get; set; }

        /// <summary>
        /// The tail of the snake.
        /// </summary>
        [JsonIgnore]
        public Vector2D tail { get; set; }

        /// <summary>
        /// A bool flag that indicates if a snake has eaten a powerup.
        /// </summary>
        [JsonIgnore]
        public bool EatenPower { get; set; }

        /// <summary>
        /// The number of frames the tail has "waited" before moving after eating powerup.
        /// Determines how much larger a snake will get after it has eaten a powerup.
        /// </summary>
        [JsonIgnore]
        public int WaitFramesPower { get; set; }


        /// <summary>
        /// Snake constructor
        /// </summary>
        /// <param name="snake"></param>
        /// <param name="name"></param>
        /// <param name="body"></param>
        /// <param name="dir"></param>
        /// <param name="score"></param>
        /// <param name="died"></param>
        /// <param name="alive"></param>
        /// <param name="dc"></param>
        /// <param name="join"></param>
        [JsonConstructor]
        public Snake(int snake, string name, List<Vector2D> body,Vector2D dir,int score,bool died,bool alive,bool dc, bool join) :base()
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
            tail = body.First();

        }

        public Snake() { }



    }
}
