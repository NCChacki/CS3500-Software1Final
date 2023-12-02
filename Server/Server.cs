using Model;
using NetworkUtil;
using SnakeGame;
using System.Diagnostics;
using System.IO.Pipes;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;

namespace Server
{
    internal class Server
    {
        



        //settings file
        private int worldSize;
        private int MSPerFrame;

        static private Dictionary<long, SocketState> clients = new Dictionary<long, SocketState>();
        static private World world = new World(2000, 0);

        static void Main(string[] args)
        {
            //TODO: Read in the settings 

            
            Server snakeServer = new Server();
            StartServer();

            Stopwatch watch = new Stopwatch();


            while(true)
            {
                while(watch.ElapsedMilliseconds< snakeServer.MSPerFrame) { }
                watch.Restart();

                //TODO:updateWorld, should moving snakes, checking for collsions, checks diconnects
                UpdateWorld(world);


                foreach (SocketState client in clients.Values)
                {
                   foreach(Snake snake in world.Players.Values)
                    {
                        string wallmessage = JsonSerializer.Serialize(snake)+ "\n";

                        //client.TheSocket.Send(wallmessage);
                    }

                   foreach(Power powerUp in  world.Powerups.Values)
                    {
                        string powermessage = JsonSerializer.Serialize(powerUp) + "\n";

                        //client.TheSocket.Send(powermessage);

                    }
                    
                }




            }


            // Sleep to prevent the program from closing,
            // since all the real work is done in separate threads.
            // StartServer is non-blocking.
            Console.Read();
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
                    if (CollisionWithWall(head, wall))
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
            SnakeGame.Vector2D snakeDirection = snake.dir;

            //get the head of the snake and move it.
            Vector2D head = snake.body.Last();

            //TODO: Get the speed from the XML file. Also check for issues with assigning head.
            Vector2D newHead = MoveTowardDirection(snakeDirection, head, 6);
            head = newHead;


            //move the tail only if the snake is not under the effects of a powerup.
            if (snake.EatenPower == false)
            {
                //now move the tail.
                Vector2D tail = snake.tail;
                Vector2D tailDirection = tail - snake.body[1];

                //move the tail in the correct direction and reasign the new tail if it catches up with a bend.
                //TODO: Get the speed from the XML again.
                Vector2D newTail = MoveTowardDirection(tailDirection, tail, 6);
                if (newTail == snake.body[1])
                {
                    tail = snake.body[1];
                    snake.body.RemoveAt(0);
                }
                else
                {
                    tail = newTail;
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

        private static bool CollisionWithWall(Vector2D head, Wall wall)
        {
            double lowerXrange;
            double upperXrange;
            double lowerYrange;
            double upperYrange;


            //Get the x-range
            if(wall.p1.X <= wall.p2.X)
            {
                lowerXrange = wall.p1.X;
                upperXrange = wall.p2.X;
            }
            else
            {
                lowerXrange = wall.p2.Y;
                upperXrange = wall.p1.Y;
            }

            //Get the y-range
            if(wall.p1.Y <= wall.p2.Y)
            {
                lowerYrange = wall.p1.Y;
                upperYrange = wall.p2.Y;
            }
            else
            {
                lowerYrange = wall.p2.Y;
                upperYrange = wall.p1.Y;
            }

            //now check to see if the collision is true.
            if((head.X >= lowerXrange && head.X <= upperXrange) && (head.Y >= lowerYrange && head.Y <= upperYrange)){
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

                if(snakeSegmentCollide(head, point1, point2))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check to see if the head of a snake is colliding with a particular segme
        /// </summary>
        /// <param name="head"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        private static bool snakeSegmentCollide(Vector2D head, Vector2D point1, Vector2D point2)
        {
            double lowerXrange;
            double upperXrange;
            double lowerYrange;
            double upperYrange;


            //Get the x-range
            if (point1.X <= point2.X)
            {
                lowerXrange = point1.X;
                upperXrange = point2.X;
            }
            else
            {
                lowerXrange = point2.Y;
                upperXrange = point1.Y;
            }

            //Get the y-range
            if (point1.Y <= point2.Y)
            {
                lowerYrange = point1.Y;
                upperYrange = point2.Y;
            }
            else
            {
                lowerYrange = point2.Y;
                upperYrange = point1.Y;
            }

            //now check to see if the collision is true.
            if ((head.X >= lowerXrange && head.X <= upperXrange) && (head.Y >= lowerYrange && head.Y <= upperYrange))
            {
                return true;
            }
            else
            {
                return false;
            }


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