using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NetworkUtil;

public static class Networking
{
    /////////////////////////////////////////////////////////////////////////////////////////
    // Server-Side Code
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Starts a TcpListener on the specified port and starts an event-loop to accept new clients.
    /// The event-loop is started with BeginAcceptSocket and uses AcceptNewClient as the callback.
    /// AcceptNewClient will continue the event-loop.
    /// </summary>
    /// <param name="toCall">The method to call when a new connection is made</param>
    /// <param name="port">The the port to listen on</param>
    public static TcpListener StartServer(Action<SocketState> toCall, int port)
    {

        //Starts a TCp listener for any IP address for the given port.
        TcpListener listener = new TcpListener(IPAddress.Any, port);

        listener.Start();


        Package ar = new Package(toCall,listener,null);
        //Begin Accept Socket and use AcceptNewClient as callback. The seoncd paramter is a tuple containing 
        //delegate so the user can take action and the tcp listener. 
        
            
        listener.BeginAcceptSocket(AcceptNewClient, ar);

        return listener;
    }

    /// <summary>
    /// To be used as the callback for accepting a new client that was initiated by StartServer, and 
    /// continues an event-loop to accept additional clients.
    ///
    /// Uses EndAcceptSocket to finalize the connection and create a new SocketState. The SocketState's
    /// OnNetworkAction should be set to the delegate that was passed to StartServer.
    /// Then invokes the OnNetworkAction delegate with the new SocketState so the user can take action. 
    /// 
    /// If anything goes wrong during the connection process (such as the server being stopped externally), 
    /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccurred flag set to true 
    /// and an appropriate message placed in its ErrorMessage field. The event-loop should not continue if
    /// an error occurs.
    ///
    /// If an error does not occur, after invoking OnNetworkAction with the new SocketState, an event-loop to accept 
    /// new clients should be continued by calling BeginAcceptSocket again with this method as the callback.
    /// </summary>
    /// <param name="ar">The object asynchronously passed via BeginAcceptSocket. It must contain a tuple with 
    /// 1) a delegate so the user can take action (a SocketState Action), and 2) the TcpListener</param>
    private static void AcceptNewClient(IAsyncResult ar)
    {


        //cast the ar AsynchState to a package, may be able to put a !
        Package temp = (Package)ar.AsyncState!;

        TcpListener listener= temp.listener!;

        try
        {
            //End Accept using the packages listener and use the retrun to form a new socket state. 
            SocketState socketState = new SocketState(temp.PassedDelegate, listener.EndAcceptSocket(ar));

            //set the socket state to the delgate passed in
            socketState.OnNetworkAction = temp.PassedDelegate;

            //invoke the onNetworkAction
            socketState.OnNetworkAction(socketState);

            
        }
        catch
        {
            SocketState errorState = new SocketState(temp.PassedDelegate, "Error when creating a incominng socket");
            
            temp.PassedDelegate(errorState);

            return;
        }


        try
        {
            listener.BeginAcceptSocket(AcceptNewClient, temp);
        }
        catch
        {
            SocketState errorState = new SocketState(temp.PassedDelegate, "Error when begining accepting new sockets");
            
            temp.PassedDelegate(errorState);
        }
    }

    /// <summary>
    /// Stops the given TcpListener.
    /// </summary>
    public static void StopServer(TcpListener listener)
    {
        listener.Stop();
    }

    /////////////////////////////////////////////////////////////////////////////////////////
    // Client-Side Code
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Begins the asynchronous process of connecting to a server via BeginConnect, 
    /// and using ConnectedCallback as the method to finalize the connection once it's made.
    /// 
    /// If anything goes wrong during the connection process, toCall should be invoked 
    /// with a new SocketState with its ErrorOccurred flag set to true and an appropriate message 
    /// placed in its ErrorMessage field. Depending on when the error occurs, this should happen either
    /// in this method or in ConnectedCallback.
    ///
    /// This connection process should timeout and produce an error (as discussed above) 
    /// if a connection can't be established within 3 seconds of starting BeginConnect.
    /// 
    /// </summary>
    /// <param name="toCall">The action to take once the connection is open or an error occurs</param>
    /// <param name="hostName">The server to connect to</param>
    /// <param name="port">The port on which the server is listening</param>
    public static void ConnectToServer(Action<SocketState> toCall, string hostName, int port)
    {
        

        // Establish the remote endpoint for the socket.
        IPHostEntry ipHostInfo;
        IPAddress ipAddress = IPAddress.None;

        // Determine if the server address is a URL or an IP
        try
        {
            ipHostInfo = Dns.GetHostEntry(hostName);
            bool foundIPV4 = false;
            foreach (IPAddress addr in ipHostInfo.AddressList)
                if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    foundIPV4 = true;
                    ipAddress = addr;
                    break;
                }
            // Didn't find any IPV4 addresses
            if (!foundIPV4)
            {
                SocketState errorState = new SocketState(toCall, "Didn't find any IPV4 addresses");
               
                toCall(errorState);
            }
        }
        catch (Exception)
        {
            // see if host name is a valid ipaddress
            try
            {
                ipAddress = IPAddress.Parse(hostName);
            }
            catch (Exception)
            {
                SocketState errorState = new SocketState(toCall, "Can not parse into a Valid IP address");
               
                toCall(errorState);
            }
        }

        // Create a TCP/IP socket.
        Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        // This disables Nagle's algorithm (google if curious!)
        // Nagle's algorithm can cause problems for a latency-sensitive 
        // game like ours will be 
        socket.NoDelay = true;


        Package temp = new Package(toCall, null, socket);

       

        try
        {
            IAsyncResult result=  socket.BeginConnect(ipAddress, port,ConnectedCallback,temp);


           if( !result.AsyncWaitHandle.WaitOne(3000, true))
            {
                SocketState errorState = new SocketState(toCall, "Could not connect under 3 seconds");

                toCall(errorState);
            }

        }
        catch
        {
            SocketState errorState = new SocketState(toCall, "Could not begin connect");
            
            toCall(errorState);
        }
        

        
    }

    /// <summary>
    /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
    ///
    /// Uses EndConnect to finalize the connection.
    /// 
    /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
    /// either this method or ConnectToServer should indicate the error appropriately.
    /// 
    /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
    /// with a new SocketState representing the new connection.
    /// 
    /// </summary>
    /// <param name="ar">The object asynchronously passed via BeginConnect</param>
    private static void ConnectedCallback(IAsyncResult ar)
    {
        


        Package package = (Package) ar.AsyncState!;
        try
        {
           
            package.socket!.EndConnect(ar); 
            SocketState socket = new SocketState(package.PassedDelegate, package.socket!);

            package.PassedDelegate(socket);
        }
        catch
        {
            SocketState errorState = new SocketState(package.PassedDelegate, "Could not end connection");
            package.PassedDelegate(errorState);
           
        }

       

    }


    /////////////////////////////////////////////////////////////////////////////////////////
    // Server and Client Common Code
    /////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Begins the asynchronous process of receiving data via BeginReceive, using ReceiveCallback 
    /// as the callback to finalize the receive and store data once it has arrived.
    /// The object passed to ReceiveCallback via the AsyncResult should be the SocketState.
    /// 
    /// If anything goes wrong during the receive process, the SocketState's ErrorOccurred flag should 
    /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
    /// OnNetworkAction should be invoked. Depending on when the error occurs, this should happen either
    /// in this method or in ReceiveCallback.
    /// </summary>
    /// <param name="state">The SocketState to begin receiving</param>
    public static void GetData(SocketState state)
    {
        try
        {
            state.TheSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, ReceiveCallback, state);
        }
        catch
        {
            state.ErrorOccurred = true;
            state.ErrorMessage = "Failed to excute recieve process succesfully";
            state.OnNetworkAction(state);
        }
    }

    /// <summary>
    /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
    /// 
    /// Uses EndReceive to finalize the receive.
    ///
    /// As stated in the GetData documentation, if an error occurs during the receive process,
    /// either this method or GetData should indicate the error appropriately.
    /// 
    /// If data is successfully received:
    ///  (1) Read the characters as UTF8 and put them in the SocketState's unprocessed data buffer (its string builder).
    ///      This must be done in a thread-safe manner with respect to the SocketState methods that access or modify its 
    ///      string builder.
    ///  (2) Call the saved delegate (OnNetworkAction) allowing the user to deal with this data.
    /// </summary>
    /// <param name="ar"> 
    /// This contains the SocketState that is stored with the callback when the initial BeginReceive is called.
    /// </param>
    private static void ReceiveCallback(IAsyncResult ar)
    {
        SocketState temp = (SocketState)ar.AsyncState!;
        Socket socket = temp.TheSocket;
        try
        {
            int endReceiveResult = socket.EndReceive(ar);
            if (endReceiveResult == 0) 
            { 
                temp.ErrorOccurred= true;
                temp.ErrorMessage = "Failed to excute recieve process succesfully";
                temp.OnNetworkAction(temp);
              
            }
            
            
            lock (temp.data) 
            {
                Console.WriteLine("data received:" + temp.data.Append(Encoding.UTF8.GetString(temp.buffer,0, endReceiveResult)));
            }
            temp.OnNetworkAction(temp);

        }
        catch
        {
           temp.ErrorOccurred = true;
            temp.ErrorMessage = "Failed to excute recieve process succesfully";
            temp.OnNetworkAction(temp);
        }
        

    }

    /// <summary>
    /// Begin the asynchronous process of sending data via BeginSend, using SendCallback to finalize the send process.
    /// 
    /// If the socket is closed, does not attempt to send.
    /// 
    /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
    /// </summary>
    /// <param name="socket">The socket on which to send the data</param>
    /// <param name="data">The string to send</param>
    /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
    public static bool Send(Socket socket, string data)
    {
        if (!socket.Connected) { return false; }
             

            try
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(data);
                socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, socket);
                return true;
            }
            catch
            {
               
                socket.Close();
            return false; 
            }
           
       
        
    }

    /// <summary>
    /// To be used as the callback for finalizing a send operation that was initiated by Send.
    ///
    /// Uses EndSend to finalize the send.
    /// 
    /// This method must not throw, even if an error occurred during the Send operation.
    /// </summary>
    /// <param name="ar">
    /// This is the Socket (not SocketState) that is stored with the callback when
    /// the initial BeginSend is called.
    /// </param>
    private static void SendCallback(IAsyncResult ar)
    {
        Socket socket= (Socket)ar.AsyncState!;
        socket.EndSend(ar);

    }


    /// <summary>
    /// Begin the asynchronous process of sending data via BeginSend, using SendAndCloseCallback to finalize the send process.
    /// This variant closes the socket in the callback once complete. This is useful for HTTP servers.
    /// 
    /// If the socket is closed, does not attempt to send.
    /// 
    /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
    /// </summary>
    /// <param name="socket">The socket on which to send the data</param>
    /// <param name="data">The string to send</param>
    /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
    public static bool SendAndClose(Socket socket, string data)
    {
        if (!socket.Connected) { return false; }

        byte[] messageBytes = Encoding.UTF8.GetBytes(data);
        try 
        { 
            socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendAndCloseCallback, socket);  
            return true;
        
        }
        catch 
        {

           socket.Close();
            return false;
        }

      
    }

    /// <summary>
    /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
    ///
    /// Uses EndSend to finalize the send, then closes the socket.
    /// 
    /// This method must not throw, even if an error occurred during the Send operation.
    /// 
    /// This method ensures that the socket is closed before returning.
    /// </summary>
    /// <param name="ar">
    /// This is the Socket (not SocketState) that is stored with the callback when
    /// the initial BeginSend is called.
    /// </param>
    private static void SendAndCloseCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState!;
        socket.EndSend(ar);

         socket.Close();
        
    }


 

   
}

internal class Package
{
    public TcpListener? listener;
    public Action<SocketState> PassedDelegate;
    public Socket? socket;
    public Package(Action<SocketState> PassedDelegate, TcpListener? listener, Socket? socket)
    {
        this.PassedDelegate = PassedDelegate;
        this.listener = listener;
        this.socket = socket;
    }

  


}


