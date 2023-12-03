﻿using Model;
using NetworkUtil;
using SnakeGame;
using System.Diagnostics;
using System.IO.Pipes;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;

namespace Server
{
    public class Server
    {

        static private Dictionary<long, SocketState> clients =  new Dictionary<long, SocketState>();
        static  private World world = new World(worldSize, 0);


        //settings object that holds the settings
        static Settings? settings { get; set; }
        //settings file
        static private int worldSize;
       



        static void Main(string[] args)
        {

            Server snakeServer = new Server();

            DataContractSerializer ser = new(typeof(Settings));
            XmlReader reader = XmlReader.Create("Settings.xml");


            if ((Settings?)ser.ReadObject(reader) != null)
            {
                settings = (Settings)ser.ReadObject(reader);
            }
            else
            {
                //i dunno what to do here yet. 
            }



            StartServer();

            Stopwatch watch = new Stopwatch();


            while (true)
            {
                while(watch.ElapsedMilliseconds< settings.MSPerFrame)
                {
                    //do nothing
                }
                watch.Restart();
                
                //TODO:updateWorld, should moving snakes, checking for collsions, checks diconnects
                UpdateWorld(world);
            }
        }

        /// <summary>
        /// Start accepting Tcp sockets connections from clients
        /// </summary>
        public static void StartServer()
        {
            // This begins an "event loop"
            Networking.StartServer(NewClientConnected, 11000);

            Console.WriteLine("Server is running");
        }

        /// <summary>
        /// Updates all the items in the world. checks for collisions
        /// </summary>
        /// <param name="world"></param>
        public static void UpdateWorld(World world)
        {
            //TODO: need to check if someone disconnects
            //update all the locations of the snakes
            foreach(Snake snake in world.Players.Values)
            {
                UpdateSnake(snake);
                //then check to see if any of the snakes have collided with anything.
                //TODO: Might be an issues with equality here. What if snakes collide at the same time? The one ealier in the Players.Values will always lose.
                Vector2D head = snake.body.Last();

                //check every wall, see if it is within colliding distance
                foreach(Wall wall in world.Walls.Values)
                {
                    if (checkForCollsion(head, wall.p1,wall.p2,25))
                    {
                        //kill the snake so it isn't drawn.
                        snake.alive = false;
                    }
                }

                //check every powerup, see if it is within colliding distance
                foreach (Power powerUp in world.Powerups.Values)
                {
                    //check if its close enough to the powerup.
                    Vector2D betweenPowerUp = powerUp.loc - head;

                    if (betweenPowerUp.Length() <= 20)
                    {
                        snake.EatenPower = true;
                        snake.WaitFramesPower = 0;
                        powerUp.died = true;
                    }
                }

                //check to see if the snakes have collided with anouther snake.
                foreach(Snake snake1 in world.Players.Values)
                {
                    if(snake1 == snake)
                    {
                        continue;
                    }
                    SnakeCollide(head, snake1);

                }



            }
        }


        /// <summary>
        /// This is a helper method that updates a snakes position after one frame.
        /// </summary>
        /// <param name="snake"></param>
       public static void UpdateSnake(Snake snake)
       {
            //get the dirction vector.
            //get the head of the snake and move it.
            // Check for issues with assigning head.
            
            Vector2D newHead = MoveTowardDirection(snake.dir, snake.body.Last<Vector2D>(), settings.SnakeGrowth);

            snake.body[snake.body.Count - 1] = newHead;


            //move the tail only if the snake is not under the effects of a powerup.
            if (snake.EatenPower == false)
            {
                //now move the tail.
                Vector2D tail = snake.tail;
                Vector2D tailDirection = tail - snake.body[1];

                //move the tail in the correct direction and reasign the new tail if it catches up with a bend.
                //TODO: Get the speed from the XML again.
                Vector2D newTail = MoveTowardDirection(tailDirection, tail, settings.SnakeGrowth);


                if (newTail == snake.body[1])
                {
                    snake.tail = snake.body[1];
                    snake.body.RemoveAt(0);
                }
                else
                {
                    snake.tail = newTail;
                }
            }
            else
            {
                //update the wait counter for the snake eating the powerup.
                snake.WaitFramesPower += 1;
                if(snake.WaitFramesPower == 24)
                {
                    snake.EatenPower = false;   
                }
            }
       }

        public static bool checkForCollsion(Vector2D head, Vector2D p1, Vector2D p2, int collsionRange)
        {
            double lowerXrange;
            double upperXrange;
            double lowerYrange;
            double upperYrange;


            //Get the x-range
            if(p1.X <= p2.X)
            {
                lowerXrange = p1.X;
                upperXrange = p2.X;
            }
            else
            {
                lowerXrange = p2.X;
                upperXrange = p1.X;
            }

            //Get the y-range
            if(p1.Y <= p2.Y)
            {
                lowerYrange = p1.Y;
                upperYrange = p2.Y;
            }
            else
            {
                lowerYrange = p2.Y;
                upperYrange = p1.Y;
            }

            //now check to see if the collision is true.
            if((head.X >= lowerXrange-collsionRange && head.X <= upperXrange+ collsionRange) && (head.Y >= lowerYrange- collsionRange && head.Y <= upperYrange+ collsionRange)){
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check to see if the head of a snake is colliding with another snake.
        /// </summary>
        /// <param name="head"></param>
        /// <param name="snake"></param>
        /// <returns></returns>
        public static bool SnakeCollide(Vector2D head, Snake snake)
        {
            List<Vector2D> body = snake.body;

            for(int i = 0; i < body.Count - 1; i++)
            {
                Vector2D point1 = body[i];
                Vector2D point2 = body[i + 1];

                if(checkForCollsion(head, point1, point2,5))
                {
                    return true;
                }
            }
            return false;
        }

        

        public static Vector2D MoveTowardDirection(Vector2D direction, Vector2D currentPos, double UnitsMoved)
        {
            direction.Normalize();


            //check if the direction is on the y axis
            if (direction.X == 0)
            {
                //if the direction up or down?
                //check if the direction is up
                if(direction.Y == -1)
                {
                    //move the currentPos correctly.
                    currentPos.Y = currentPos.Y - UnitsMoved;
                    return currentPos;

                }
                //otherwise the direction is down.
                else
                {
                    //move the currentPos correctly
                    currentPos.Y = currentPos.Y + UnitsMoved;
                    return currentPos;
                }

            }
            //otherwise the direction is on the Y axis.
            else
            {
                //if the direction left or right?
                //check if the direction is right
                if (direction.X == 1)
                {
                    //move the currentPos correctly.
                    currentPos.X = currentPos.X + UnitsMoved;
                    return currentPos;

                }
                //otherwise the direction left.
                else
                {
                    //move the currentPos correctly
                    currentPos.X = currentPos.X - UnitsMoved;
                    return currentPos;
                }
            }




        }



        /// <summary>
        /// This is a helper method that updates a snakes position and length after it eats
        /// a powerup.
        /// </summary>
        /// <param name="snake"></param>
        public static void UpdateSnakePowerUP(Snake snake)
        {

        }

        /// <summary>
        /// Method to be invoked by the networking library
        /// when a new client connects (see line 41)
        /// </summary>
        /// <param name="state">The SocketState representing the new client</param>
        private static void NewClientConnected(SocketState state)
        {
            if (state.ErrorOccurred)
                return;

           

            // change the state's network action to the 
            // receive handler so we can process data when something
            // happens on the network
            state.OnNetworkAction = receivePlayerName;
            Networking.GetData(state);

            
        }

        private static void receivePlayerName(SocketState state)
        {
            //get player name out of the state's buffer
            //creat a snake for that player


            //make a new snake add to servers world

            //send the world size and player id back to the SocketStates socket send

            //send the walls
            
           


            //^^^ this has to be done before we send and more information
            //sneding infomration is just adding the client to the client list

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (clients)
            {
                clients[state.ID] = state;
            }
            
            state.OnNetworkAction = receiveCommandRequests;
            Networking.GetData(state);
        }

        private static void receiveCommandRequests(SocketState state)
        {
            //parse moving objects and apply that to the snake that belongs to the id of the socket 
            //update the snakes direction so that when udate world is call the snake is changed correectly
            
            Networking.GetData(state);

        }


    }
}