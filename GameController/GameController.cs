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
        //field for the player name;
        public string playerName;

        public double playerX;
        public double playerY;

        public int playerID;
        public int worldSize;


        //TODO: Figure out how the view is gonna repsond to these
        public delegate void ConnectedHandler();
        public event ConnectedHandler? Connected;

        //The client should not be able send commands before the world is built, this event will
        // enable the entry for commands once the world is built
        public delegate void WorldBuiltHandler();
        public event ConnectedHandler? WorldBuilt;


        // A delegate and event to fire when the controller
        // has received and processed new info from the server
        public delegate void GameUpdateHandler();
        public event GameUpdateHandler UpdateArrived;

        //Int that repersents the number of messages arrived, only really important for the building of the world. 
        int numberOfMessages;
      

        /// <summary>
        /// Socket State representing the connection with the server
        /// </summary>
        SocketState? theServer = null;


        //A world obejct (model i think)
        public World world;

      

        public GameController(string playerName) 
        {
            this.playerName = playerName;
            numberOfMessages = 0;
            
        }
        public void Connect(string addr)
        {
            Networking.ConnectToServer(OnConnect, addr, 11000);
        }

        private void OnConnect(SocketState state)
        {
            if (state.ErrorOccurred)
            {
              //TODO: check for errors
            }

            //TODO: come back to the /n
            Networking.Send(state.TheSocket, playerName+"\n");
            
            // commuincate back to the view via a event
            Connected?.Invoke();

            theServer = state;

            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveMessageFromServer;
            Networking.GetData(state);
        }

        private void ReceiveMessageFromServer(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                //TODO: Check if there is a errro
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
                
            //pull message from states buffer and split it
            string totalData = state.GetData();
            if (totalData.Length == 0)
                return;
           
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
                        WorldBuilt.Invoke();
                        UpdateArrived.Invoke();
                    }
                    continue;
                }

                if (doc.RootElement.TryGetProperty("wall", out _))
                {
                    //deserialize the wall json
                    
                    Wall wall = JsonSerializer.Deserialize<Wall>(p);
                   
                    //add to the worlds list of walls
                   lock(world.Walls)
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
                    lock (world.Players)
                    {
                        if (player.died)
                        {
                            world.Players.Remove(player.name);
                            //trigger an explosion?
                        }
                        else if (player.alive)
                            world.Players[player.name] = player;

                        if(player.name==playerName)
                        {
                            playerX = player.body.FirstOrDefault().X;
                            playerY = player.body.FirstOrDefault().Y;
                        }

                    }

                }
                
                
                if (doc.RootElement.TryGetProperty("power", out _) || doc.RootElement.TryGetProperty("power", out _))
                {


                    // Then remove it from the SocketState's growable buffer
                    state.RemoveData(0, p.Length);

                    //deserialize the powerUp
                    Power power = JsonSerializer.Deserialize<Power>(p);

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


            UpdateArrived.Invoke();

            //// inform the view
            //MessagesArrived?.Invoke(newMessages);

        }

         
        }
    }




