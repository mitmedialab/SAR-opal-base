// Jacqueline Kory Westlund
// June 2016
//
// The MIT License (MIT)
// Copyright (c) 2016 Personal Robots Group
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Net;
using MiniJSON;

namespace opal
{

// received message event -- fire when we get a message
// so others can listen for the messages
    public delegate void ReceivedMessageEventHandler(object sender,
                               int command,object properties);


/**
 * Web socket client
 * For receiving commands from a remote controller or teleop
 * and to allow us to send back log messages
 * */
    public class RosbridgeWebSocketClient
    {
        private string SERVER = "";
        private string PORT_NUM = null;
        // create a timer to use when trying to reconnect the websocket
        private System.Timers.Timer timer = new System.Timers.Timer(1000); // in ms

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
            System.Net.IPAddress ip;
            UInt16 num;
            
            // TODO test this
            if (!System.Net.IPAddress.TryParse(rosIP, out ip))
                throw new ArgumentException("[websocket] IP address is not valid!", "rosIP");
            
            if (!UInt16.TryParse(portNum, out num))
                throw new ArgumentException("[websocket] Port number is not a port!", "portNum");
            
            this.SERVER = rosIP;    
            this.PORT_NUM = portNum;
            
            // subscribe to timer (used for reconnections)
            this.timer.Elapsed += OnTimeElapsed;
            this.timer.Enabled = false;
            this.timer.AutoReset = true;
        }
    
        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations 
        /// before the <see cref="RosbridgeWebSocketClient"/> is reclaimed by
        /// garbage collection. Closes web socket properly.
        /// </summary>
        ~RosbridgeWebSocketClient()
        {
            try {
                // close socket
                if(this.clientSocket != null) {
                    this.clientSocket.Close();
                    this.clientSocket.OnOpen -= HandleOnOpen;
                    this.clientSocket.OnClose -= HandleOnClose;
                    this.clientSocket.OnError -= HandleOnError;
                    this.clientSocket.OnMessage -= HandleOnMessage;
                }
            } catch(Exception e) {
                Debug.Log(e.ToString());
            }
        }
    
        /// <summary>
        /// Set up the web socket for communication through rosbridge
        /// and register handlers for messages
        /// </summary>
        public bool SetupSocket ()
        {
            // create new websocket that listens and sends to the
            // specified server on the specified port
            try {
                Debug.Log("[websocket] creating new websocket... ");
                this.clientSocket = new WebSocket(("ws://" + SERVER +
                    (PORT_NUM == null ? "" : ":" + PORT_NUM)));
            
                // If the specified address does not exist on the network,
                // there is a 90s timeout before it'll give up trying to connect
                // (hardcoded in the library) -- manifests as app hanging
                // BUT if you use CONNECTASYNC then it doesn't hang!
                //
                // If address does exist but you've forgotten to start 
                // rosbridge_server, the connection will be refused.
          
                // OnOpen event occurs when the websocket connection is established
                this.clientSocket.OnOpen += HandleOnOpen;

                // OnMessage event occurs when we receive a message
                this.clientSocket.OnMessage += HandleOnMessage;
            
                // OnError event occurs when there's an error
                this.clientSocket.OnError += HandleOnError; 
                
                // OnClose event occurs when the connection has been closed
                this.clientSocket.OnClose += HandleOnClose;
            
                Debug.Log("[websocket] connecting to websocket...");
                // connect to the server
                this.clientSocket.Connect(); // TODO connectasync?
                return true;
            } catch(Exception e) {
                Debug.LogError("[websocket] Error starting websocket: " + e);
                return false;
            }
        }


        /// <summary>
        /// Tries to reconnect web socket for communication through rosbridge
        /// </summary>
        public void Reconnect()
        {
            try {
                Debug.Log("[websocket] trying to connect to websocket...");
                // connect to the server
                this.clientSocket.Connect();
            } catch(Exception e) {
                Debug.LogError("[websocket] Error starting websocket: " + e);
                this.timer.Enabled = true;
            }
        }

        /// <summary>
        /// public request to close the socket
        /// </summary>
        public void CloseSocket ()
        {
            // close the socket
            if(this.clientSocket != null) {
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
        public bool SendMessage (String msg)
        {
            if(this.clientSocket.IsAlive) {
                return this.SendToServer(msg);
            } else {
                Debug.LogWarning("[websocket] Can't send message - client socket dead!"
                    + "\nWill try to reconnect to socket...");
                this.timer.Enabled = true;
                return false;
            }
        }

        /// <summary>
        /// Sends string message to server
        /// </summary>
        /// <returns><c>true</c>, if message was sent, <c>false</c> otherwise.</returns>
        /// <param name="msg">Message.</param>
        private bool SendToServer (String msg)
        {
            Debug.Log("[websocket] sending message: " + msg);
        
            // try sending to server
            try {
                // write to socket
                this.clientSocket.Send(msg); 
                return true; // success!
            } catch(Exception e) {
                Debug.LogError("[websocket] ERROR: failed to send " + e.ToString());
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
            Debug.Log("[websocket] ---- Opened WebSocket ----");
        }
    
        /// <summary>
        /// Handle OnMessage events, which occur when we receive a message
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void HandleOnMessage (object sender, MessageEventArgs e)
        {
            // if the message is a string, we can parse it
            if (e.IsText)
            {
                Debug.Log("[websocket] Received message: " + e.Data);
            
                // use rosbridge utilities to decode and parse message
                int command = -1;
                object properties = null;
                RosbridgeUtilities.DecodeROSJsonCommand(e.Data, out command, out properties);
            
                // got a command!
                // we let the game controller sort out if it's a real command or not
                // as well as what to do with the extra properties, if any
                
                // fire event indicating that we received a message
                if(this.receivedMsgEvent != null) {
                    // only send subset of msg that is actual message
                    this.receivedMsgEvent(this, command, properties);
                }
            }
            else if (e.IsBinary)
            {
                Debug.LogWarning("[websocket] Received byte array in message but we " +
                    "were expecting a string message.");
            }
        }
    
        /// <summary>
        /// OnError event occurs when there's an error
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void HandleOnError (object sender, ErrorEventArgs e)
        {
            Debug.LogError("[websocket] Error in websocket! " + e.Message + "\n" +
                e.Exception);
        }
    
        /// <summary>
        /// Handle OnClose events, which occur when the websocket connection
        /// has been closed. Also, this gets called when there is an exception 
        /// reconnecting, which is weird.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void HandleOnClose (object sender, CloseEventArgs e)
        {
           Debug.Log("[websocket] Websocket closed with status: " + e.Reason + 
                "\nCode: " + e.Code + "\nClean close? " + e.WasClean);
            
            // turn on timer so we try reconnecting later
            // probably sets timer enabled twice - here and in reconnect
            this.timer.Enabled = true;
            this.Reconnect();
        }
        
        /// <summary>
        /// called when the timer has elapsed
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">E.</param>
        void OnTimeElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Debug.Log("[websocket] Time elapsed, trying to reconnect...");
            this.timer.Enabled = false;
            this.Reconnect();
        }
    }
}