using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;
using MiniJSON;

// received message event -- fire when we get a message
// so others can listen for the messages
public delegate void ReceivedMessageEventHandler(object sender, 
                               int command, object properties);


/**
 * Web socket client
 * For receiving commands from a remote controller or teleop
 * and to allow us to send back log messages
 * */
public class RosbridgeWebSocketClient
{
	private string SERVER = "";
	private string PORT_NUM = null;
	public event ReceivedMessageEventHandler receivedMsgEvent;
	private WebSocket clientSocket; // client websocket
	
	
	 /// <summary>
	 /// Initializes a new instance of the <see cref="RosbridgeWebSocketClient"/> 
	 /// class.
	 /// </summary>
	 /// <param name="rosIP">IP address of websocket server</param>
	 /// <param name="portNum">Port number or null if none</param>
	public RosbridgeWebSocketClient(string rosIP, string portNum)
	{
		// TODO do some kind of validation of IP address and port?
	     this.SERVER = rosIP;	
	     this.PORT_NUM = portNum;
	}
	
     /// <summary>
     /// Releases unmanaged resources and performs other cleanup operations 
     /// before the <see cref="RosbridgeWebSocketClient"/> is reclaimed by
     /// garbage collection. Closes web socket properly.
     /// </summary>
	~RosbridgeWebSocketClient()
	{
		try
		{
		 	// close socket
			if (this.clientSocket != null)
            {
				this.clientSocket.Close();
                this.clientSocket.OnOpen -= HandleOnOpen;
                this.clientSocket.OnClose -= HandleOnClose;
                this.clientSocket.OnError -= HandleOnError;
                this.clientSocket.OnMessage -= HandleOnMessage;
            }
		}
		catch (Exception e)
		{
			Debug.Log(e.ToString());
		}
	}
	
    /// <summary>
    /// Set up the web socket for communication through rosbridge
    /// and register handlers for messages
    /// </summary>
	public void SetupSocket()
	{
        if (this.SERVER == "")
            return;
    
		// create new websocket that listens and sends to the
		// specified server on the specified port
		try
		{
            Debug.Log("creating new websocket... ");
            this.clientSocket = new WebSocket(("ws://" + SERVER +
			    (PORT_NUM == null ? "" : ":" + PORT_NUM)));
            
            // so if the IP address is one on the current network, throws error properly
            // but if it doesn't exist, hangs
		  
			// OnOpen event occurs when the websocket connection is established
			this.clientSocket.OnOpen += HandleOnOpen;

			// OnMessage event occurs when we receive a message
			this.clientSocket.OnMessage += HandleOnMessage;
			
			// OnError event occurs when there's an error
			this.clientSocket.OnError += HandleOnError; 
				
			// OnClose event occurs when the connection has been closed
			this.clientSocket.OnClose += HandleOnClose;
			
            Debug.Log("connecting to websocket...");
			// connect to the server
			this.clientSocket.Connect();
		} catch (Exception e)
		{
			Debug.Log ("Error starting websocket: " + e);
		}
	}


    /// <summary>
    /// public request to close the socket
    /// </summary>
	public void CloseSocket()
	{
		// close the socket
        if (this.clientSocket != null)
        {
    		this.clientSocket.Close(WebSocketSharp.CloseStatusCode.Normal,
    		                        "Closing normally");
            this.clientSocket.OnOpen -= HandleOnOpen;
            this.clientSocket.OnClose -= HandleOnClose;
            this.clientSocket.OnError -= HandleOnError;
            this.clientSocket.OnMessage -= HandleOnMessage;
        }
	}

    /// <summary>
    /// Public request to send message 
    /// </summary>
    /// <returns><c>true</c>, if message was sent, <c>false</c> otherwise.</returns>
    /// <param name="msg">Message.</param>
	public bool SendMessage(String msg)
	{
		if (this.clientSocket.IsAlive)
		{
			return this.SendToServer(msg);
		}
		else
		{
			Debug.Log ("Can't send message - client socket dead!");
			return false;
		}
	}

    /// <summary>
    /// Sends string message to server
    /// </summary>
    /// <returns><c>true</c>, if message was sent, <c>false</c> otherwise.</returns>
    /// <param name="msg">Message.</param>
	private bool SendToServer(String msg)
	{
		Debug.Log ("sending message: " + msg);
		
		// try sending to server
		try
		{
			// write to socket
			this.clientSocket.Send(msg); 
			return true; // success!
		}
		catch (Exception e)
		{
			Debug.Log("ERROR: failed to send " + e.ToString());
			return false; // fail :(
		}
	}
	
    /// <summary>
    /// Handle OnOpen events, which occur when the websocket connection
    /// has been established
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">E.</param>
	void HandleOnOpen (object sender, EventArgs e)
	{
	    // connection opened
	    Debug.Log("---- Opened WebSocket ----");
	}
	
    /// <summary>
    /// Handle OnMessage events, which occur when we receive a message
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">E.</param>
	 void HandleOnMessage (object sender, MessageEventArgs e)
	{
		Debug.Log ("Received message: " + e.Data);
		
        // use rosbridge utilities to decode and parse message
        int command = -1;
        object properties = null;
        RosbridgeUtilities.DecodeROSJsonCommand(e.Data, out command, out properties);
        
		// got a command!
		// we let the game controller sort out if it's a real command or not
		// as well as what to do with the extra properties, if any
			
		// fire event indicating that we received a message
		if (this.receivedMsgEvent != null)
		{
			// only send subset of msg that is actual message
			this.receivedMsgEvent(this, command, properties);
		}
	}
	
    /// <summary>
    /// OnError event occurs when there's an error
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">E.</param>
	void HandleOnError (object sender, ErrorEventArgs e)
	{
		Debug.LogError("Error in websocket! " + e.Message + "\n" +
		               e.Exception);
	}
	
	/// <summary>
    /// Handle OnClose events, which occur when the websocket connection
    /// has been closed
    /// </summary>
    /// <param name="sender">Sender.</param>
    /// <param name="e">E.</param>
	void HandleOnClose (object sender, CloseEventArgs e)
	{
		Debug.Log ("Websocket closed");
	}
	
}