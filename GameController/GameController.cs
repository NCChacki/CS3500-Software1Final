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
        public string? playerName;

        public int playerID;
        public int worldSize;


        //TODO: Figure out how the view is gonna repsond to these
        public delegate void ConnectedHandler();
        public event ConnectedHandler? Connected;

        //The client should not be able send commands before the world is built, this event will
        // enable the entry for commands once the world is built
        public delegate void WorldBuiltHandler();
        public event ConnectedHandler? WorldBuilt;

        //bools used for keeping track of the state of process messages
        bool firstMessageArrived;
        bool secondMessageArrived; 

        /// <summary>
        /// Socket State representing the connection with the server
        /// </summary>
        SocketState? theServer = null;


        //A world obejct (model i think)
        public World world;

      

        public GameController(string playerName) 
        {
            playerName = playerName!;
            firstMessageArrived = false;
            secondMessageArrived = false;
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

            if (!firstMessageArrived)
            {
                //pull message from states buffer and split it
                string totalData = state.GetData();
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");
              

                int numberOfCompleteParts = 0;
                List<string> newMessages = new List<string>();

                //loop through the split messaegs
                foreach (string p in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0)
                        continue;
                    
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (p[p.Length - 1] != '\n')
                        break;

                    //checks to see if we already have all the parts we need for the complete message. 
                    if(numberOfCompleteParts < 2)
                    {
                        newMessages.Add(p);
                        numberOfCompleteParts++;
                       
                        // Then remove it from the SocketState's growable buffer
                        state.RemoveData(0, p.Length);

                    }
                }

                //// inform the view
                //MessagesArrived?.Invoke(newMessages);


                //check to see if there are two arrrived parts, if not return invoking another get data call. 
                if (newMessages.Count<2)
                {
                    return;
                }

                //once there are two messages
                firstMessageArrived = true;

               

                //creat a world object of the size passed through the message also the player ID the world corresponds too
                 World world = new World(int.Parse(newMessages[1]), int.Parse(newMessages[0]));

             
            }
            else if (!secondMessageArrived)
            {

                string totalData = state.GetData();
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");
                
                foreach (string p in parts)
                {

                    JsonDocument doc = JsonDocument.Parse(p);

                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0)
                        continue;

                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (p[p.Length - 1] != '\n')
                        break;

                    //if it has gotten here then the piece is a complete message and can be eamed for what type of json it is
                    //if the parsed json doc conatins a wall property then the json is most likely a wall, treat accordingly
                    if (doc.RootElement.TryGetProperty("wall", out _))
                    {
                        //deserialize the wall json
                        Wall wall = JsonSerializer.Deserialize<Wall>(p);

                        //add to the worlds list of walls
                        world.Walls.Add(wall.wall, wall);

                        // Then remove it from the SocketState's growable buffer
                        state.RemoveData(0, p.Length);

                    }
                    
                    //checks to see if snakes or powerups are being sent, if so that means all walls have been received and the server is now sending updates. 
                    if (doc.RootElement.TryGetProperty("snake", out _) || doc.RootElement.TryGetProperty("power", out _))
                    {
                        //if the next message is a json of a snake or power up, do not add the string form or clear the buffer of the info as
                        // we want this info later
                      
                        continue;
                    }
                    
                        
                }
                //second set of infomtion has arrivd and has been parsed, now move onto processing update infomation
                secondMessageArrived = true;

            }
            //if the code gets to this else statment, that means the world size/playername message and the wall messages have been recived. 
            else if(secondMessageArrived && firstMessageArrived) 
            {
                string totalData = state.GetData();
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");
           
                foreach (string p in parts)
                {

                    JsonDocument doc = JsonDocument.Parse(p);

                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0)
                        continue;
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (p[p.Length - 1] != '\n')
                        break;

                    //if the parsed doc conatins a snake property then its most likely a snake json and can be aded to the player list
                    if (doc.RootElement.TryGetProperty("snake", out _))
                    {
                        
                        
                        // Then remove it from the SocketState's growable buffer
                        state.RemoveData(0, p.Length);

                        //deserialize the snake
                        Snake player = JsonSerializer.Deserialize<Snake>(p);

                        if (player.died)
                        {
                            world.Players.Remove(player.snake);
                            //trigger an explosion?
                        }
                        else if(player.alive) 
                            world.Players[player.snake] = player;
                       

                      
                    }

                 
                    if (doc.RootElement.TryGetProperty("power", out _) || doc.RootElement.TryGetProperty("power", out _))
                    {
                       
                        
                        // Then remove it from the SocketState's growable buffer
                        state.RemoveData(0, p.Length);

                        //deserialize the powerUp
                        Power power = JsonSerializer.Deserialize<Power>(p);

                        if (power.died)
                            world.Powerups.Remove(power.power);
                        else
                            world.Powerups[power.power] = power;
                    }

                    //TODO, how do you know if a update message is done. And allow the client to start sending commands

                    //no idea so far on the first one
                    //for the last problem think of having a event like WorldBuilt, then allowing the command entry to be enabled. 




                }

               
               



            }


        }
    }




}