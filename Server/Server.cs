using Model;
using NetworkUtil;
using SnakeGame;
using System.Diagnostics;
using System.IO.Pipes;
using System.Numerics;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Windows.Markup;
using System.Xml;
using System.Xml.Linq;

namespace Server
{
    public class Server
    {
        //set up the dictionary of clients and the servers's world.
        static private Dictionary<long, SocketState> clients =  new Dictionary<long, SocketState>();
        static  private World world = new World(worldSize, 0);
        static private Dictionary<long, string> socketPlayerNameRelations = new Dictionary<long, string>();


        //settings object that holds the settings
        static Settings? settings { get; set; }
        //settings file
        static private int worldSize;
       
        static void Main(string[] args)
        {

            Server snakeServer = new Server();

            ////Set up the reader to read the setting file.
            //DataContractSerializer ser = new(typeof(Settings));
            //XmlReader reader = XmlReader.Create("C:\\Users\\Norman Canning\\source\\repos\\game-jcpenny\\Server\\Settings.xml");

            ////read the values from the settings file.
            //if ((Settings?)ser.ReadObject(reader) != null)
            //{
            //    settings = (Settings)ser.ReadObject(reader);
            //}
            //else
            //{
            //    //i dunno what to do here yet. 
            //}

            List<Wall> vector2Ds = new List<Wall>();
            vector2Ds.Add(new Wall(0,new Vector2D(-975,-975),new Vector2D(975,-975)));
            vector2Ds.Add(new Wall(1, new Vector2D(-975, -975), new Vector2D(-975, 975)));
            vector2Ds.Add(new Wall(2, new Vector2D(975, 975), new Vector2D(975, -975)));
            vector2Ds.Add(new Wall(3, new Vector2D(975, 975), new Vector2D(-975, 975)));

            settings = new Settings(75,20,34,100,24,6,120,2000,vector2Ds);

            world.Walls.Add(settings.Walls[0].wall, settings.Walls[0]);
            world.Walls.Add(settings.Walls[1].wall, settings.Walls[1]);
            world.Walls.Add(settings.Walls[2].wall, settings.Walls[2]);
            world.Walls.Add(settings.Walls[3].wall, settings.Walls[3]);

            //Start the server and begin looking for connections.
            StartServer();

            Stopwatch watch = new Stopwatch();
            watch.Start();

            //this loop updates the world at regualar intervals dictated by the settings file.
            while (true)
            {

                while (watch.ElapsedMilliseconds < 1000)
                {
                  
                }
                watch.Restart();

                //Updates the world(sets and changes objects, checks for collisions, sends Json information, etc).
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
                Vector2D head = snake.body.Last();

                //check every wall, see if it is within colliding distance
                foreach(Wall wall in world.Walls.Values)
                {
                    if (checkForCollsion(head, wall.p1,wall.p2,25))
                    {
                        //kill the snake so it isn't drawn.
                        snake.alive = false;
                        snake.died = true;



                        //world.Players.Remove(snake.name);
                        //continue;
                    }
                }

                //check every powerup, see if it is within colliding distance
                foreach (Power powerUp in world.Powerups.Values)
                {
                   
                    if (checkForCollsion(head,powerUp.loc,powerUp.loc,5))
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
                    
                    if(SnakeCollide(head, snake1))
                    {
                        snake.died = true;
                        snake.alive = false;

                        world.Players.Remove(snake.name);
                    }
                }


            }

            foreach (SocketState client in clients.Values)
            {
                foreach (Snake snake in world.Players.Values)
                {
                    Networking.Send(client.TheSocket, JsonSerializer.Serialize(snake) + "\n");
                    Console.WriteLine(JsonSerializer.Serialize(snake));
                }
                foreach (Power powerup in world.Powerups.Values)
                {
                    Networking.Send(client.TheSocket, JsonSerializer.Serialize(powerup) + "\n");
                } 
            }

        }

        public static bool SelfCollision(Vector2D head, Snake snake){

            //find the direction of the snake.
            Vector2D direction = snake.dir;

            bool reachedOpposite = false;

            //check the first opposite segment
            for(int i = snake.body.Count - 1; i < 0; i--)
            {
                //what is the direction of the previous segment?
                Vector2D point2 = snake.body[i];
                Vector2D point1 = snake.body[i - 1];

                Vector2D segmentOrientation = point1 + point2;
                segmentOrientation.Normalize();

                if (reachedOpposite == false && segmentOrientation.IsOppositeCardinalDirection(direction))
                {
                    reachedOpposite = true;
                }

                if (reachedOpposite)
                {
                    if(checkForCollsion(head, point1, point2, 5))
                    {
                        return true;
                    }
                }
            }
            return false;
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

            if(snake.alive = false)
            {
                //check the death counter
                if (snake.WaitFramesRespawn <= settings.RespawnRate)
                {
                    snake.WaitFramesRespawn += 1;
                }
                else
                {
                    snake = SpawnSnake(world, snake.snake, snake.name);
                    snake.WaitFramesRespawn = 0;
                }
            }
            else
            {

                Vector2D newHead = MoveTowardDirection(snake.dir, snake.body.Last<Vector2D>(), 6);

                if (snake.turned)
                {
                    snake.body.Add(newHead);
                    snake.turned = false;
                }
                else
                {
                    snake.body[snake.body.Count - 1] = newHead;
                }

                //move the tail only if the snake is not under the effects of a powerup.
                if (snake.EatenPower == false)
                {

                    //now move the tail.
                    Vector2D tail = snake.body[0];
                    Vector2D tailDirection = snake.body[1] - tail;

                    //move the tail in the correct direction and reasign the new tail if it catches up with a bend.
                    //TODO: Get the speed from the XML again.
                    Vector2D newTail = MoveTowardDirection(tailDirection, tail, 6);

                    snake.body[0] = newTail;



                    if (snake.body[0].X == snake.body[1].X && snake.body[0].Y == snake.body[1].Y)
                    {
                        snake.body.RemoveAt(1);
                    }
                    else
                    {
                        snake.body[0] = newTail;
                    }
                }
                else
                {
                    //update the wait counter for the snake eating the powerup.
                    snake.WaitFramesPower += 1;
                    if (snake.WaitFramesPower == 24)
                    {
                        snake.EatenPower = false;
                    }
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
                    return new Vector2D(currentPos.X, currentPos.Y - UnitsMoved);

                }
                //otherwise the direction is down.
                else
                {
                    //move the currentPos correctly
                    return new Vector2D(currentPos.X, currentPos.Y + UnitsMoved);
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
                    return new Vector2D(currentPos.X + UnitsMoved, currentPos.Y);

                }
                //otherwise the direction left.
                else
                {
                    //move the currentPos correctly
                    return new Vector2D(currentPos.X - UnitsMoved, currentPos.Y);
                }
            }
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

        /// <summary>
        /// This method handles recieving a players name from the client. This is part of the handshake
        /// between the client and the server.
        /// </summary>
        /// <param name="state">The SocketState the name is being recieved on</param>
        private static void receivePlayerName(SocketState state)
        {
            //check for errors or disconnections.
            if (state.ErrorOccurred)
            {
                //TODO: figure out how to handle these errors.
                return;
            }

            //pull message from states buffer 
            string totalData = state.GetData();

            //Split the data by the terminating charater
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            List<string> worldDetails = new List<string>();

            Snake newSnake;
            //Only the first message sent by the client is necessary, which should contain the players name.
            //this if statement ensures that at least one full message has been recieved by GetData.
            if (parts.Length >= 1 && parts[0].Length != 0 && parts[0][parts.Length - 1] != '\n')
            {
                //we have the string of the player name.
               
                state.RemoveData(0, parts[0].Length);   
                Console.WriteLine(parts[0]);

                //this creates a random location to add a new snake to the game.
                newSnake = SpawnSnake(world, (int)state.ID, parts[0][0..^1]);
                Console.WriteLine(JsonSerializer.Serialize(newSnake));

                world.Players.Add(newSnake.name, newSnake);



            }
            else
            {
                //if the name hasn't been sent yet, call getData again.
                Networking.GetData(state);
            }

            //send the worlsize and then the player ID.
            Networking.Send(state.TheSocket,  state.ID.ToString() + "\n" + settings.UniverseSize.ToString() + "\n" );

            //send all of the walls
            StringBuilder Walls = new StringBuilder("");
            foreach(Wall wall in world.Walls.Values)
            {
                //serialize and send each wall.
                String wallSerialized = JsonSerializer.Serialize(wall);
                Walls.Append(wallSerialized);
                Walls.Append("\n");
            }
            Console.WriteLine(Walls.ToString());
            Networking.Send(state.TheSocket, Walls.ToString());


            string playerName = parts[0];

            //^^^ this has to be done before we send and more information
            //sneding infomration is just adding the client to the client list

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (clients)
            {
                clients.Add(state.ID, state);
                socketPlayerNameRelations.Add(state.ID, playerName);
            }

            
            state.OnNetworkAction = receiveCommandRequests;
            Networking.GetData(state);
        }


        private static Snake SpawnSnake(World world, int ID, String snakeName)
        {
            while (true)
            {
                var rand = new Random();

                //get the random location for the head.
                int newSnakeX1 = rand.Next(-1000, 1000);
                int newSnakeY1 = rand.Next(-1000, 1000);

                Vector2D head = new Vector2D(newSnakeX1, newSnakeY1);
                Vector2D tail;

                int newSnakeX2;
                int newSnakeY2;
                Vector2D newSnakeDir;

                //Now, get the location of the tail, which dictates the direction of the snake. 
                //This loop also checks for collisons and ensures that new snakes aren't placed
                //on top of old ones or existing walls.
                if (newSnakeX1 % 4 == 0)
                {
                    //tail is above the head.
                    newSnakeY2 = newSnakeY1 - settings.SnakeStartingLength;
                    newSnakeX2 = newSnakeX1;
                    newSnakeDir = new Vector2D(0, 1);
                    tail = new Vector2D(newSnakeX2, newSnakeY2);


                }

                else if (newSnakeX1 % 4 == 1)
                {
                    //tail is to the left of the head.
                    newSnakeX2 = newSnakeX1 - settings.SnakeStartingLength;
                    newSnakeY2 = newSnakeY1;
                    newSnakeDir = new Vector2D(1, 0);
                    tail = new Vector2D(newSnakeX2, newSnakeY2);

                }

                else if (newSnakeX1 % 4 == 2)
                {
                    //tail is beneath the head.
                    newSnakeY2 = newSnakeY1 + settings.SnakeStartingLength;
                    newSnakeX2 = newSnakeX1;
                    newSnakeDir = new Vector2D(0, -1);
                    tail = new Vector2D(newSnakeX2, newSnakeY2);

                }

                else
                {
                    //tail is to the right of the head.
                    newSnakeX2 = newSnakeX1 + settings.SnakeStartingLength;
                    newSnakeY2 = newSnakeY1;
                    newSnakeDir = new Vector2D(-1, 0);
                    tail = new Vector2D(newSnakeX2, newSnakeY2);

                }

                //check to see if the random snake is colliding with anything.
                //check both tail and head.

                //check each wall to see if there is a collision with the head or the tail.
                foreach (Wall wall in world.Walls.Values)
                {
                    if (checkForCollsion(head, wall.p1, wall.p2, 50))
                    {
                        continue;
                    }
                    if (checkForCollsion(tail, wall.p1, wall.p2, 50))
                    {
                        continue;
                    }

                }


                //check each snake to see if there is a collision with the head or the tail.
                foreach (Snake snake1 in world.Players.Values)
                {
                    if (SnakeCollide(head, snake1))
                    {
                        continue;
                    }
                    if (SnakeCollide(tail, snake1))
                    {
                        continue;
                    }

                }

                //make the snake and add it to the world.
                return new Snake(ID, snakeName, new List<Vector2D> { tail, head }, newSnakeDir, 0, false, true, false, true);

            }
        }

        private static void receiveCommandRequests(SocketState state)
        {
            //parse moving objects and apply that to the snake that belongs to the id of the socket 
            //update the snakes direction so that when udate world is call the snake is changed correectly
            if (state.ErrorOccurred)
            {
                //TODO: figure out how to handle these errors.
                return;
            }


            //pull message from states buffer 
            string totalData = state.GetData();

            //if not data was in buffer call getData again
            if (totalData.Length == 0)
                return;

            //Split the data by the terminating charater
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            List<string> worldDetails = new List<string>();


            //loop through the split messaegs
            if (parts.Length >= 1&& parts[0].Length != 0 && parts[0][parts.Length-1]!='\n')
            {

                //used to check if the part is a json of a Player, Wall, or Power
                JsonDocument doc = JsonDocument.Parse(parts[0]);
                
                Console.Write("Command From Client: " + doc.ToString());

                if (doc.RootElement.TryGetProperty("moving", out _))
                {
                    //create movement object from the command.
                    Moving movement = JsonSerializer.Deserialize<Moving>(parts[0]);

                    Vector2D newdir;

                    //check to see what the command is. Create a new dir vector for the snake.
                    if (movement.moving=="up") { newdir = new Vector2D(0, -1); }
                    else if (movement.moving == "down") { newdir = new Vector2D(0, 1); }
                    else if (movement.moving == "left") { newdir = new Vector2D(-1, 0); }
                    else { newdir = new Vector2D(1, 0); }


                    
                    //set the snake turned variable to true.
                    world.Players[socketPlayerNameRelations[state.ID]].dir= newdir;
                    world.Players[socketPlayerNameRelations[state.ID]].turned = true;
                  

                    // Then remove it from the SocketState's growable buffer
                    state.RemoveData(0, parts[0].Length);
                }
            }
            Networking.GetData(state);
        }


    }
}