using Model;
using NetworkUtil;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;

namespace Server
{
    internal class Server
    {
        


        static private Dictionary<long, SocketState> clients =  new Dictionary<long, SocketState>();
        static  private World world = new World(worldSize, 0);

        //settings file
        private int worldSize;
        private int MSPerFrame;

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


                //move the foreach into update world
                foreach (SocketState client in clients.Values)
                {
                   foreach(Snake snake in world.Players.Values)
                    {
                        string wallmessage = JsonSerializer.Serialize(snake)+ "\n";

                        client.TheSocket.Send(wallmessage);
                    }

                   foreach(Power powerUp in  world.Powerups.Values)
                    {
                        string powermessage = JsonSerializer.Serialize(powerUp) + "\n";

                        client.TheSocket.Send(powermessage);

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