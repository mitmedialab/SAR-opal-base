using System;

// received message event -- fire when we get a message
// so others can listen for the messages
public delegate void ReceivedMessageEventHandler(object sender, String msg);


/**
 * TODO
 * rosbridge web socket client
 * for communication with the teleop / remote controller of app
 * */
public class RosbridgeWebSocketClient
{
	private const int PORT_NUM = 8080;
	public event ReceivedMessageEventHandler receivedMsgEvent;
	
	/**
	 * constructor
	 * */
	public RosbridgeWebSocketClient()
	{
	}
	
	
	
	
}