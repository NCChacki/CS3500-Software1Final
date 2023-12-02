using Model;
using NetworkUtil;

namespace ServerController
{
    public class ServerConroller
    {

        /// <summary>
        /// Socket State representing the connection with the server
        /// </summary>
        SocketState? theServer = null;


        /// <summary>
        /// The object repersenting the current game world.
        /// </summary>
        public World world;

        public ServerConroller()
        {

        }


        /// <summary>
        /// Will process current data in the state's buffer and then call for more data
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessageFromClient(SocketState state)
        {
            if (state.ErrorOccurred)
            {
                //TODO: Find a way to handle errors in the recieve.(Ignore, or disconnect the client)
            }

            ProcessDataFromClient(state);

            //look for more data from the server
            Networking.GetData(state);
        }

        private void ProcessDataFromClient(SocketState state)
        {
            

        }


    }
}