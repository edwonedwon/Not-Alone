using UnityEngine;
using System.Collections;

public class NetworkManagerScript : MonoBehaviour
{
	string gameTypeName = "DipeChange";
	
	private bool refreshing;
	private HostData[] hostData;
	
	void Start ()
	{
		DontDestroyOnLoad(this);
	}
	
	void Update ()
	{
		if (refreshing) 
		{
			if (MasterServer.PollHostList().Length > 0)
			{
				refreshing = false;	
				print(MasterServer.PollHostList().Length);
				hostData = MasterServer.PollHostList();
			}
			else
			{
				hostData = null;
			}
		}
	}
	
	void StartServer () 
	{
		Network.InitializeServer(2, 25001, !Network.HavePublicAddress());
		MasterServer.RegisterHost(gameTypeName, "Game " + System.DateTime.Now.TimeOfDay.ToString(), "This is a test network game");
	}
	
	void RefreshHostList ()
	{
		MasterServer.RequestHostList(gameTypeName);
		refreshing = true;
	}
	
	void Disconnect ()
	{
		Network.Disconnect();
		MasterServer.UnregisterHost();
	}
	
	//Messages
	void OnServerInitialized ()
	{
		print("Server Initialized!");	
	}
	
	void OnConnectedToServer()
	{
		
	}
	
	void OnMasterServerEvent (MasterServerEvent mse)
	{
		if (mse == MasterServerEvent.RegistrationSucceeded)
		{
			print("registered server");	
		}
	}
	
	void OnGUI () 
	{		
		GUILayout.BeginArea (new Rect (50, 50, 200, Screen.height - 2*50));
		{
			if (!Network.isClient && !Network.isServer)
			{
				if (GUILayout.Button("Start Server")) 
				{	
					StartServer();
				}
		
				if (GUILayout.Button("Refresh Hosts")) 
				{
					RefreshHostList();
				}
				
				if (!(hostData == null)) 
				{					
					for (int i = 0; i< hostData.Length; i++)
					{
						if (GUILayout.Button("Join "+ hostData[i].gameName)) 
						{
							print("Network.Connect: " + Network.Connect(hostData[i]).ToString());	
						}
					}
				}
			}
			
			if (Network.isClient || Network.isServer) 
			{
				if (GUILayout.Button("Disconnect")) 
				{
					Disconnect();
				}
			}
		}
		
		GUILayout.EndArea();
		
	}
}
