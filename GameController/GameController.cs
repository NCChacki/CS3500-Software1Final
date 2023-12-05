//Game Controller for Snake Game. Implemneted by Chase CANNNING and Jack MCINTYRE for CS3500, Fall of 2023

using NetworkUtil;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Model;
using SnakeGame;
using System.Numerics;


namespace GameController
{
    public class GameController
    {
        /// <summary>
        /// Name of the player the GC belongs to
        /// </summary>
        public string playerName;

        /// <summary>
        /// The Game World x cordinate of the player
        /// </summary>
        public double playerX;

        /// <summary>
        /// the Game World y cordinate of the player
        /// </summary>
        public double playerY;

        /// <summary>
        /// the ID of the player
        /// </summary>
        public int playerID;

        /// <summary>
        /// the size of the world object designated by the server
        /// </summary>
        public int worldSize;

        //The client should not be able send commands before the world is built, this event will
        // enable the entry for commands once the world is built
        public delegate void WorldBuiltHandler();
        public event WorldBuiltHandler? WorldBuilt;


        // A delegate and event to fire when the controller
        // has received and processed new info from the server
        public delegate void GameUpdateHandler();
        public event GameUpdateHandler UpdateArrived;

        //a delegate and event to fire when the keyBoardHack
        //text box has been changed and a command needs to be sent to the server
        public delegate void OnTextChangedHandler();
        public event OnTextChangedHandler OnTextChanged;

        //a delegate and event to be fired when a connection error has occured
        public delegate void ErrorArrivedHandler();
        public event ErrorArrivedHandler ErrorArrived;

        /// <summary>
        /// Number of messages received by the client from the server, only used to identify the first two messages for building the world.
        /// not a realiable way of keeping track of total messages, as it is not increamneted after first two messages are received.
        /// </summary>
        int numberOfMessages;


        /// <summary>
        /// Socket State representing the connection with the server
        /// </summary>
        SocketState? theServer = null;


        /// <summary>
        /// The object repersenting the current game world.
        /// </summary>
        public World world;


        /// <summary>
        /// game controller assigned to the current client, will comminicate with view through events. The player name is 
        /// default "player" playerName but can be changed by user before connect is pushed. 
        /// </summary>
        /// <param name="playerName"></param>
        /// 
        public GameController(string playerName)
        {
            this.playerName = playerName;
            numberOfMessages = 0;

        }

        /// <summary>
        /// Method used to handel the connect button being clicked
        /// Will call the networking libraries connectToServer using adress in the server text box. 
        /// </summary>
        /// <param name="addr"></param>
        public void Connect(string addr)
        {
            Networking.ConnectToServer(OnConnect, addr, 11000);
        }


        /// <summary>
        /// Call back for the connectToServer Called by the Game Controllers Connect method.
        /// Will check and rspond to a errorState, Send the player name to the server then intaite the receive loop.
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                ErrorArrived.Invoke();
                return;
            }

            //send the server the player name
            Networking.Send(state.TheSocket, playerName + "\n");



            theServer = state;

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveMessageFromServer;
            Networking.GetData(state);
        }
        
        /// <summary>
        /// Will process current data in the state's buffer and then call for more data
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessageFromServer(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                ErrorArrived.Invoke();
                return;
            }

            ProcessDataFromServer(state);

            //look for more data from the server
            Networking.GetData(state);
        }


        /// <summary>
        /// Process any buffered messages separated by '\n
        /// </summary>
        /// <param name="state"></param>
        private void ProcessDataFromServer(SocketState state)
        {

            if (state.ErrorOccurred)
            {
                ErrorArrived.Invoke();
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
            foreach (string p in parts)
            {

                // Ignore empty strings added by the regex splitter
                if (p.Length == 0) continue;

                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n') break;

                //used to check if the part is a json of a Player, Wall, or Power
                JsonDocument doc = JsonDocument.Parse(p);


                //checks to see if we already have all the parts we need for the complete a world.
                if (numberOfMessages < 2)
                {
                    numberOfMessages++;

                    worldDetails.Add(p);
                    // Then remove it from the SocketState's growable buffer
                    state.RemoveData(0, p.Length);
                    if (numberOfMessages == 2)
                    {
                        world = new World(int.Parse(worldDetails[1]), int.Parse(worldDetails[0]));
                        worldSize = world.size;

                        //inform veiw the world has been built and the client can start sending commands
                        WorldBuilt?.Invoke();

                        //inform view an update has arrived and to starting updating the view
                        UpdateArrived.Invoke();
                    }

                    continue;
                }

                if (doc.RootElement.TryGetProperty("wall", out _))
                {
                    //deserialize the wall json

                    Wall wall = JsonSerializer.Deserialize<Wall>(p);

                    //add to the worlds list of walls
                    lock (world.Walls)
                    {
                        world.Walls.Add(wall.wall, wall);
                    }


                    // Then remove it from the SocketState's growable buffer
                    state.RemoveData(0, p.Length);

                    continue;

                }

                //if the parsed doc conatins a snake property then its most likely a snake json and can be aded to the player list
                if (doc.RootElement.TryGetProperty("snake", out _))
                {


                    // Then remove it from the SocketState's growable buffer
                    state.RemoveData(0, p.Length);

                    //deserialize the snake
                    Snake player = JsonSerializer.Deserialize<Snake>(p);

                    //lock players, and remove snakes that have died and add in living ones. 
                    lock (world.Players)
                    {
                        if (player.died)
                        {
                            world.Players.Remove(player.name);
                          
                        }
                        else if (player.alive)
                            world.Players[player.name] = player;

                        //update current x and y of the player the GC belongs too
                        if (player.name == playerName)
                        {
                            playerX = player.body.Last<Vector2D>().X;
                            playerY = player.body.Last<Vector2D>().Y;
                        }

                    }

                }


                if (doc.RootElement.TryGetProperty("power", out _) || doc.RootElement.TryGetProperty("power", out _))
                {


                    // Then remove it from the SocketState's growable buffer
                    state.RemoveData(0, p.Length);

                    //deserialize the powerUp
                    Power power = JsonSerializer.Deserialize<Power>(p);

                    //lock powerups, and remove powerups if they have died. 
                    lock (world.Powerups)
                    {
                        if (power.died)
                            world.Powerups.Remove(power.power);
                        else
                            world.Powerups[power.power] = power;
                    }
                }
                continue;
            }

            //all information has been processed, now update the view. 
            UpdateArrived.Invoke();
        }

        /// <summary>
        /// Method called by the view when the text inside of the command entry has changed. If the string movemnt is a 
        /// valid command, a moving object is created then serilazed to be sent to the server.
        /// </summary>
        /// <param name="movement"></param>
        public void textChanged(string movement)
        {

            Moving moving = new Moving(movement);
            string message = JsonSerializer.Serialize(moving) + "\n";

            Networking.Send(theServer!.TheSocket, message);
        }



    }
}




