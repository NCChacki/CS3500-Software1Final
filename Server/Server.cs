using Model;
using NetworkUtil;
using System.IO.Pipes;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace Server
{
    internal class Server
    {
        // A map of clients that are connected, each with an ID
        private  Dictionary<long, SocketState> clients;
        private World world;

        //settings file
        private int worldSize;

        static void Main(string[] args)
        {
            Server snakeServer = new Server();
            snakeServer.StartServer();

            // Sleep to prevent the program from closing,
            // since all the real work is done in separate threads.
            // StartServer is non-blocking.
            Console.Read();


            while(true)
            {
                foreach (SocketState client in snakeServer.clients.Values)
                {
                   foreach(Snake snake in snakeServer.world.Players.Values)
                    {
                        string wallmessage = JsonSerializer.Serialize(snake)+ "\n";

                        client.TheSocket.Send(wallmessage);
                    }

                   foreach(Power powerUp in  snakeServer.world.Powerups.Values)
                    {
                        string powermessage = JsonSerializer.Serialize(powerUp) + "\n";

                        client.TheSocket.Send(powermessage);

                    }
                    
                }
            }
        }

        /// <summary>
        /// Initialized the server's state
        /// </summary>
        public Server()
        {
            clients = new Dictionary<long, SocketState>();
            world = new World(worldSize, 0);

            // add walls to world objects 
        }

        /// <summary>
        /// Start accepting Tcp sockets connections from clients
        /// </summary>
        public void StartServer()
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
        private void NewClientConnected(SocketState state)
        {
            if (state.ErrorOccurred)
                return;

           

            // change the state's network action to the 
            // receive handler so we can process data when something
            // happens on the network
            state.OnNetworkAction = receivePlayerName;
            Networking.GetData(state);

            
        }

        private void receivePlayerName(SocketState state)
        {

            state.OnNetworkAction = receiveCommandRequests;
            Networking.GetData(state);


            
            //make a new snake add to servers world, to be serialized and sent of later

            //send the world size and player id back to the SocketStates socket send


            //^^^ this has to be done before we send and more information
            //sneding infomration is just adding the client to the client list

            // Save the client state
            // Need to lock here because clients can disconnect at any time
            lock (clients)
            {
                clients[state.ID] = state;
            }
        }

        private void receiveCommandRequests(SocketState state)
        {
            //parse moving objects and apply that to the snake that belongs to the id of the socket 
            
            Networking.GetData(state);

        }


    }
}