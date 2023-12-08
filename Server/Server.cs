using Model;
using NetworkUtil;
using SnakeGame;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq.Expressions;
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
        static private Dictionary<long, SocketState> clients = new Dictionary<long, SocketState>();
        static private World world = new World(worldSize, 0);
        static private Dictionary<long, string> socketPlayerNameRelations = new Dictionary<long, string>();


        //settings object that holds the settings
        static Settings? settings { get; set; }
        //settings file
        static private int worldSize;


        static private int powerUpDelay;
        static private int WaitFramesRespawnPower = 0;
        static private int numberOfAlivePowerUps;

        static void Main(string[] args)
        {

            Server snakeServer = new Server();


            DataContractSerializer ser = new(typeof(Settings));



            XmlReader reader = XmlReader.Create("C:\\Users\\jtmc2\\source\\repos\\game-jcpenny\\Server\\Settings.xml");
            settings = (Settings)ser.ReadObject(reader);

            

            Random random = new Random();
            powerUpDelay = random.Next(settings.MaxPowerUpDelay);
            numberOfAlivePowerUps = 0;

            settings.Walls = new List<Wall>();

            int i = 0;
            foreach (Wall wall in settings.Walls)
            {
                world.Walls.Add(i, wall);
                i++;
            }


            for (int j = 0; j < settings.MaxPowerUps; j++)
            {
                while (true)
                {
                    var rand = new Random();



                    Vector2D possiblePowLoc = new Vector2D(rand.Next(-1000, 1000), rand.Next(-1000, 1000));

                    bool locValid = true;

                    foreach (Wall wall in world.Walls.Values)
                    {
                        if (!checkForCollsion(possiblePowLoc, wall.p1, wall.p2, 25))
                        {
                            continue;
                        }
                        else
                        {
                            locValid = false;
                            break;
                        }

                    }
                    if (locValid)
                    {
                        world.Powerups.Add(j, new Power(j, possiblePowLoc, false));
                        numberOfAlivePowerUps += 1;
                        break;
                    }

                }
            }


            //Start the server and begin looking for connections.
            StartServer();



            Stopwatch watch = new Stopwatch();
            watch.Start();

            //this loop updates the world at regualar intervals dictated by the settings file.
            while (true)
            {

                while (watch.ElapsedMilliseconds < settings.MSPerFrame)
                {

                }
                watch.Restart();

                //Updates the world(sets and changes objects, checks for collisions, sends Json information, etc).


                lock (world)
                {
                    UpdateWorld(world);
                }

            }

        }


        public static bool CheckOutBounds(Vector2D head)
        {
            return (head.X > 1000 || head.X < -1000) || (head.Y > 1000 || head.Y < -1000);
        }


        public static Vector2D PopOut(Vector2D point)
        {
            if (point.X > 1000)
            {
                return new Vector2D(point.X - 2000, point.Y);
            }
            else if (point.X < -1000)
            {
                return new Vector2D(point.X + 2000, point.Y);
            }
            else if (point.Y > 1000)
            {
                return new Vector2D(point.X, point.Y - 2000);
            }
            else
            {
                return new Vector2D(point.X, point.Y + 2000);
            }

        }

        public static Vector2D AnchorPoint(Vector2D point)
        {
            if (point.X >= 1000)
            {
                return new Vector2D(1000, point.Y);
            }
            else if (point.X <= -1000)
            {
                return new Vector2D(-1000, point.Y);
            }
            else if (point.Y >= 1000)
            {
                return new Vector2D(point.X, 1000);
            }
            else
            {
                return new Vector2D(point.X, -1000);
            }

        }


        public static Vector2D AnchorPointOtherside(Vector2D point)
        {
            if (point.X >= 1000)
            {
                return new Vector2D(-1000, point.Y);
            }
            else if (point.X <= -1000)
            {
                return new Vector2D(1000, point.Y);
            }
            else if (point.Y >= 1000)
            {
                return new Vector2D(point.X, -1000);
            }
            else
            {
                return new Vector2D(point.X, 1000);
            }

        }

        public static Vector2D OppositeDir(Vector2D point)
        {
            if(point.X == 1)
            {
                return new Vector2D(-1, 0); 
            }
            if(point.X == -1)
            {
                return new Vector2D(1, 0);
            }
            if (point.Y == 1)
            {
                return new Vector2D(0, -1);
            }
            else
            {
                return new Vector2D(0, 1);
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



                foreach (Snake snake in world.Players.Values)
                {
                    Snake itSnake = new Snake();
                    UpdateSnake(snake);
                    itSnake = world.Players[snake.name];
                    //then check to see if any of the snakes have collided with anything.
                    Vector2D head = itSnake.body.Last();

                    if (itSnake.WaitFramesRespawn > 0)
                    {
                        continue;
                    }

                    //check every wall, see if it is within colliding distance
                    foreach (Wall wall in world.Walls.Values)
                    {
                        if (checkForCollsion(head, wall.p1, wall.p2, 25))
                        {
                            Console.WriteLine(itSnake.name + " collided with a " + wall + " of cordinates" + wall.p1 + " and " + wall.p2);

                            //kill the snake so it isn't drawn.
                            itSnake.alive = false;
                            itSnake.died = true;
                            itSnake.score = 0;


                        }
                    }

                    //check every powerup, see if it is within colliding distance
                    foreach (Power powerUp in world.Powerups.Values)
                    {

                        if (powerUp.died == false)
                        {
                            if (checkForCollsion(head, powerUp.loc, powerUp.loc, 10))
                            {
                                if (settings.SuperPowerUpEnabled == 1 && powerUp.power % 4 == 0)
                                {
                                    itSnake.score += 150;
                                    itSnake.eatenSuperPower = true;
                                    itSnake.WaitFramesPower = 0;
                                    powerUp.died = true;
                                    numberOfAlivePowerUps =numberOfAlivePowerUps- 1;

                                }
                                else
                                {
                                    itSnake.score += 100;
                                    itSnake.EatenPower = true;
                                    itSnake.WaitFramesPower = 0;
                                    powerUp.died = true;
                                    numberOfAlivePowerUps = numberOfAlivePowerUps - 1;

                                }

                            }

                        }

                    }

                    //check to see if the snakes have collided with anouther snake.
                    foreach (Snake snake1 in world.Players.Values)
                    {

                        if (snake1 == itSnake)
                        {

                            if (SelfCollision(head, itSnake))
                            {

                                Console.WriteLine("Snake: " + itSnake.name);

                                itSnake.alive = false;
                                itSnake.died = true;
                                itSnake.score = 0;
                            }

                            continue;
                        }

                        if (snake1.alive == false)
                        {
                            continue;
                        }

                        if (SnakeCollide(head, snake1))
                        {
                            Console.WriteLine("Snake: " + itSnake.name + " collidd with another snake " + snake1.name);
                            itSnake.died = true;
                            itSnake.alive = false;
                            itSnake.score = 0;


                        }
                    }


                }

                foreach (SocketState client in clients.Values)
                {
                    foreach (Snake snake in world.Players.Values)
                    {
                        if (!Networking.Send(client.TheSocket, JsonSerializer.Serialize(snake) + "\n"))
                        {
                            world.Players[socketPlayerNameRelations[client.ID]].dc = true;

                        }

                    }
                    foreach (Power powerup in world.Powerups.Values)
                    {
                        if (!Networking.Send(client.TheSocket, JsonSerializer.Serialize(powerup) + "\n"))
                        {
                            world.Players[socketPlayerNameRelations[client.ID]].dc = true;

                        }

                       
                    }

                    foreach (Snake snake in world.Players.Values)
                    {
                        if (snake.dc)
                        {
                            Console.WriteLine("Snake disconnect: " + snake.name);
                            foreach (SocketState thoseInMorning in clients.Values)
                            {
                                Networking.Send(thoseInMorning.TheSocket, JsonSerializer.Serialize(snake) + "\n");
                            }
                            world.Players.Remove(snake.name);

                        }

                    }
                    if (numberOfAlivePowerUps < settings.MaxPowerUps)
                    {

                        if (WaitFramesRespawnPower < powerUpDelay)
                        {
                            WaitFramesRespawnPower++;

                            if (WaitFramesRespawnPower == powerUpDelay)
                            {
                                while (true)
                                {
                                    Random rand = new Random();
                                    Vector2D possiblePowLoc = new Vector2D(rand.Next(-1000, 1000), rand.Next(-1000, 1000));

                                    bool locValid = true;

                                    foreach (Wall wall in world.Walls.Values)
                                    {
                                        if (!checkForCollsion(possiblePowLoc, wall.p1, wall.p2, 30))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            locValid = false;
                                            break;
                                        }

                                    }

                                    foreach (Snake snake in world.Players.Values)
                                    {
                                        Vector2D firstSeg = snake.body[0];
                                        foreach (Vector2D segment in snake.body)
                                        {
                                            if (segment.Equals(firstSeg))
                                                continue;

                                            if (checkForCollsion(possiblePowLoc, firstSeg, segment, 10))
                                            {
                                                locValid = false;
                                                break;
                                            }

                                            firstSeg = segment;
                                        }
                                        if (!locValid) { break; }
                                    }
                                    if (locValid)
                                    {


                                        foreach (Power power in world.Powerups.Values)
                                        {
                                            if (power.died)
                                            {
                                                power.died = false;
                                                power.loc = possiblePowLoc;
                                                Console.WriteLine("powerUpSpawned");
                                                WaitFramesRespawnPower = 0;
                                                numberOfAlivePowerUps += 1;

                                                Random random = new Random();
                                                powerUpDelay = random.Next(settings.MaxPowerUpDelay);
                                            }
                                        }
                                        break;

                                    }
                                }
                            }
                        }
                    }
                }
            }

        

        public static bool SelfCollision(Vector2D head, Snake snake)
        {

            //find the direction of the snake.
            Vector2D direction = snake.dir;

            bool reachedOpposite = false;

            //check the first opposite segment
            for (int i = snake.body.Count - 1; i > 0; i--)
            {
                //what is the direction of the previous segment?
                Vector2D point2 = snake.body[i];
                Vector2D point1 = snake.body[i - 1];

                if(CheckTheBorder(point1) && CheckTheBorder(point2))
                {
                    continue;
                }

                Vector2D segmentOrientation = point2 - point1;
                segmentOrientation.Normalize();

                if (reachedOpposite == false && segmentOrientation.IsOppositeCardinalDirection(direction))
                {
                    reachedOpposite = true;
                }

                if (reachedOpposite)
                {
                    if (checkForCollsion(head, point1, point2, 5))
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

            if (snake.alive == false)
            {

                if (snake.WaitFramesRespawn <= settings.RespawnRate)
                {
                    snake.WaitFramesRespawn += 1;
                }
                else
                {
                    world.Players[snake.name] = SpawnSnake(world, snake.snake, snake.name);
                    snake.WaitFramesRespawn = 0;
                }
            }
            else
            {
                bool OutBounds = false;

                Vector2D newHead = MoveTowardDirection(snake.dir, snake.body.Last<Vector2D>(), 6);

                //check to see if the snake is going to be out of bounds
                if (CheckOutBounds(newHead))
                {
                    //remove old head.
                    snake.body.RemoveAt(snake.body.Count-1);

                    //need to add a new vertex at the edge
                    snake.body.Add(AnchorPoint(newHead));
                    snake.body.Add(AnchorPointOtherside(newHead));

                    //should add a snake head to the correct position on the other side
                    snake.body.Add(PopOut(newHead));
                    
                    OutBounds = true;
                }

                //when tail reaches an anchor 


                if (snake.turned)
                {
                    snake.body.Add(newHead);
                    snake.turned = false;
                }
                else
                {
                    if (!OutBounds)
                    {
                        snake.body[snake.body.Count - 1] = newHead;
                    }
                }

                //move the tail only if the snake is not under the effects of a powerup.
                if (snake.EatenPower == false && snake.eatenSuperPower == false)
                {

                    //now move the tail.
                    Vector2D tail = snake.body[0];
                    Vector2D tailDirection = snake.body[1] - tail;

                    //move the tail in the correct direction and reasign the new tail if it catches up with a bend.
                    //TODO: Get the speed from the XML again.
                    Vector2D newTail = MoveTowardDirection(tailDirection, tail, 6);

                    if (CheckOutBounds(newTail))
                    {
                        newTail = PopOut(newTail);
                        snake.body.RemoveAt(1);
                    }

                    snake.body[0] = newTail;



                    if (snake.body[0].X == snake.body[1].X && snake.body[0].Y == snake.body[1].Y)
                    {
                        snake.body.RemoveAt(1);
                    }
                    else
                    {
                        //Get rid of
                        snake.body[0] = newTail;
                    }
                }
                else
                {
                    snake.WaitFramesPower += 1;
                    if (snake.WaitFramesPower == (settings.SnakeGrowth * 2) && snake.eatenSuperPower)
                    {
                        snake.eatenSuperPower = false;

                    }


                    //update the wait counter for the snake eating the powerup.

                    if (snake.WaitFramesPower == settings.SnakeGrowth && snake.EatenPower)
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
            if (p1.X <= p2.X)
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
            if (p1.Y <= p2.Y)
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
            if ((head.X >= lowerXrange - collsionRange && head.X <= upperXrange + collsionRange) && (head.Y >= lowerYrange - collsionRange && head.Y <= upperYrange + collsionRange))
            {
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

            for (int i = 0; i < body.Count - 1; i++)
            {
                Vector2D point1 = body[i];
                Vector2D point2 = body[i + 1];

                if (checkForCollsion(head, point1, point2, 5))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckTheBorder(Vector2D point)
        {
            return (point.X == 1000 || point.X == -1000) || (point.Y == 1000 || point.Y == -1000);
        }



        public static Vector2D MoveTowardDirection(Vector2D direction, Vector2D currentPos, double UnitsMoved)
        {
            direction.Normalize();

            //check if the direction is on the y axis
            if (direction.X == 0)
            {
                //if the direction up or down?
                //check if the direction is up
                if (direction.Y == -1)
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
                //Console.WriteLine(parts[0]);

                //this creates a random location to add a new snake to the game.
                newSnake = SpawnSnake(world, (int)state.ID, parts[0][0..^1]);
                //clients.Add(newSnake.snake, state);
                //socketPlayerNameRelations.Add(newSnake.snake, newSnake.name);

                //Console.WriteLine(JsonSerializer.Serialize(newSnake));
                lock (world)
                {
                    world.Players.Add(newSnake.name, newSnake);
                }



            }
            else
            {
                //if the name hasn't been sent yet, call getData again.
                Networking.GetData(state);
            }

            //send the worlsize and then the player ID.
            Networking.Send(state.TheSocket, state.ID.ToString() + "\n" + settings.UniverseSize.ToString() + "\n");

            //send all of the walls
            StringBuilder Walls = new StringBuilder("");
            foreach (Wall wall in world.Walls.Values)
            {
                //serialize and send each wall.
                String wallSerialized = JsonSerializer.Serialize(wall);
                Walls.Append(wallSerialized);
                Walls.Append("\n");
            }
            //Console.WriteLine(Walls.ToString());
            Networking.Send(state.TheSocket, Walls.ToString());


            string playerName = parts[0][0..^1];

            //^^^ this has to be done before we send and more information
            //sneding infomration is just adding the client to the client list

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (world)
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
                bool WallCollide = false;
                foreach (Wall wall in world.Walls.Values)
                {
                    if (checkForCollsion(head, wall.p1, wall.p2, 50))
                    {
                        WallCollide = true;
                    }
                    if (checkForCollsion(tail, wall.p1, wall.p2, 50))
                    {
                        WallCollide = true;
                    }

                }
                if (WallCollide == true)
                {
                    continue;
                }

                bool SnakeCol = false;
                //check each snake to see if there is a collision with the head or the tail.
                foreach (Snake snake1 in world.Players.Values)
                {
                    if (SnakeCollide(head, snake1))
                    {
                        SnakeCol = true;
                    }
                    if (SnakeCollide(tail, snake1))
                    {
                        SnakeCol = true;
                    }
                }

                if (SnakeCol == true)
                {
                    continue;
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
                Console.WriteLine("Potential Disconnect");
                lock (world)
                {
                    world.Players[socketPlayerNameRelations[state.ID]].dc = true;
                    world.Players[socketPlayerNameRelations[state.ID]].died = true;
                    world.Players[socketPlayerNameRelations[state.ID]].alive = false;
                }

                clients.Remove(state.ID);
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
            if (parts.Length >= 1 && parts[0].Length != 0 && parts[0][parts.Length - 1] != '\n')
            {

                //used to check if the part is a json of a Player, Wall, or Power
                JsonDocument doc = JsonDocument.Parse(parts[0]);

                //Console.Write("Command From Client: " + doc.ToString());

                if (doc.RootElement.TryGetProperty("moving", out _))
                {
                    //create movement object from the command.
                    Moving movement = JsonSerializer.Deserialize<Moving>(parts[0]);
                    String s = socketPlayerNameRelations[state.ID];
                    Vector2D newdir = world.Players[s].dir;

                    bool noChange = false;

                    //check to see what the command is. Create a new dir vector for the snake.
                    if (movement.moving == "up") { newdir = new Vector2D(0, -1); }
                    else if (movement.moving == "down") { newdir = new Vector2D(0, 1); }
                    else if (movement.moving == "left") { newdir = new Vector2D(-1, 0); }
                    else if (movement.moving == "right"){ newdir = new Vector2D(1, 0); }
                    else { noChange = true; }



                    //set the snake turned variable to true.
                    if (!newdir.IsOppositeCardinalDirection(world.Players[s].dir) && !noChange)
                    {
                        lock (world)
                        {
                            world.Players[s].dir = newdir;
                            world.Players[socketPlayerNameRelations[state.ID]].turned = true;
                        }
                    }





                    // Then remove it from the SocketState's growable buffer
                    state.RemoveData(0, parts[0].Length);
                }
            }
            Networking.GetData(state);
        }


    }
}