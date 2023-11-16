using NetworkUtil;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameController
{
    public class GameController
    {
        //field for the player name;
        public string ?playerName;


        //TODO: Figure out how the view is gonna repsond to these
        public delegate void ConnectedHandler();
        public event ConnectedHandler? Connected;


        /// <summary>
        /// Socket State representing the connection with the server
        /// </summary>
        SocketState? theServer = null;

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

            Networking.Send(state.TheSocket, )
            // commuincate bakc to the view via a event
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


        bool firstMessageArrived = false;
        bool secondMessageArrived= false;
        bool updatesBeingReceived= false;
        /// <summary>
        /// Process any buffered messages separated by '\n
        /// </summary>
        /// <param name="state"></param>
        private void ProcessDataFromServer(SocketState state)
        {


          //process the name data and world size
          //process the wall 
          //treat and proceeded data as updates

        }
    }




}