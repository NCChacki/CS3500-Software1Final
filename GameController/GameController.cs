using NetworkUtil;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;

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


        /// <summary>
        /// Socket State representing the connection with the server
        /// </summary>
        SocketState? theServer = null;

      

        public GameController(string playerName) 
        {
            playerName = playerName!;
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


        bool firstMessageArrived = false;
        bool secondMessageArrived= false;
       
        /// <summary>
        /// Process any buffered messages separated by '\n
        /// </summary>
        /// <param name="state"></param>
        private void ProcessDataFromServer(SocketState state)
        {


          //process the name data and world size
          //process the wall 
          //treat and proceeded data as updates



            if (!firstMessageArrived)
            {
                string totalData = state.GetData();
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");
                // Loop until we have processed all messages.
                // We may have received more than one.

                int numberOfMessagesRecieved = 0;

                List<string> newMessages = new List<string>();

                foreach (string p in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0)
                        continue;
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (p[p.Length - 1] != '\n')
                        break;

                    // build a list of messages to send to the view
                    if(numberOfMessagesRecieved<2)
                    {
                        newMessages.Add(p);
                        numberOfMessagesRecieved++;
                        // Then remove it from the SocketState's growable buffer
                        state.RemoveData(0, p.Length);

                    }
                }

                //// inform the view
                //MessagesArrived?.Invoke(newMessages);


                //check to see if there are two arrrived messages, if not return invoking another get data call. 
                if (newMessages.Count<2)
                {
                    return;
                }

                //once there are two messages
                firstMessageArrived = true;
                playerID =int.Parse(newMessages[0]);
                worldSize= int.Parse(newMessages[1]);

             
            }
            else if (!secondMessageArrived)
            {

                string totalData = state.GetData();
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");
                // Loop until we have processed all messages.
                // We may have received more than one.

                int numberOfMessagesRecieved = 0;

                List<string> wallStringForms = new List<string>();

                foreach (string p in parts)
                {
                    // Ignore empty strings added by the regex splitter
                    if (p.Length == 0)
                        continue;
                    // The regex splitter will include the last string even if it doesn't end with a '\n',
                    // So we need to ignore it if this happens. 
                    if (p[p.Length - 1] != '\n')
                        break;

                    // build a list of messages to send to the view
                    if (p.Contains("wall"))
                    {
                        wallStringForms.Add(p);
                        numberOfMessagesRecieved++;
                        // Then remove it from the SocketState's growable buffer
                        state.RemoveData(0, p.Length);

                    }

                    if(p.Contains("snake")|| p.Contains("power"))
                        break;
                    
                        
                }

                foreach(string wallStringForm in wallStringForms)
                {
                   wal

                }

                



            }
            else
            {
                // json deserialize update infomrtaion, 

                
            }


        }
    }




}