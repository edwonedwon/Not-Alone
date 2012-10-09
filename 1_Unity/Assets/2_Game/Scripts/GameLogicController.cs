using UnityEngine;
using System.Collections;


public class GameLogicController : MonoBehaviour
{
	public static GameLogicController instance = null;
	
	public GameObject player1Prefab;
	public GameObject player2Prefab;
	
	private GameObject player1 = null;
	private GameObject player2 = null;	
	
	public GameObject gestureInitializer;
	public GameObject gestureManager;
	public GameObject networkManager;
		
	private int currentLevelIdx = 0;
	public string[] levelProgression;
	
	void Start()
	{
		Camera camcam = Camera.main;
		float screenHeight = camcam.GetScreenHeight()-1;		
		camcam.orthographicSize = screenHeight / 2.0f;
		
		if(instance == null)
		{
			instance = this;
			
	        DontDestroyOnLoad (gameObject);	
			
			Instantiate(gestureInitializer);
			Instantiate(gestureManager);
			Instantiate(networkManager);
			
			levelProgression = new string[] {"NotAlone", "Level2", "Level3"};
		}
		else
			DestroyImmediate(gameObject);
 
	}
	
	void OnEnable()
	{
		networkView.observed = this;
	}
	
	void OnDisable()
	{

	}
	
	public void BeginGame()
	{
		Application.LoadLevel(1);
//		MoveToNextLevel();
	}
	
	public void MoveToNextLevel()
	{
		if(Network.isServer)
		{
			if(++currentLevelIdx > 2)
				currentLevelIdx = 0;
			networkView.RPC ("SetLevel", RPCMode.AllBuffered, currentLevelIdx);
		}
	}
	
	[RPC]
	private void SetLevel(int idx)
	{
		//currentLevelIdx = idx;
		
		//turn message sending off...
		Network.SetSendingEnabled(0, false);
		Network.isMessageQueueRunning = false;
		
		Application.LoadLevel(levelProgression[idx]);
		Network.SetLevelPrefix(idx);
		
		//Okay turn us back on!
		Network.isMessageQueueRunning = true;
		Network.SetSendingEnabled(0, true);
	}

	
	void Update()
	{
		if(Network.isServer)
		{
			if(player1 == null)
			{
				if (!Application.loadedLevelName.Contains("Main Menu"))
				{
					//DebugStreamer.message = "created player 1";
					player1 = (GameObject)Network.Instantiate(player1Prefab, new Vector3(-10000, 0, 0), Quaternion.identity, 0);
					player1.GetComponent<PlayerScript>().isPlayer1 = true;
					DontDestroyOnLoad(player1);
				}
			}	
		}
		else if(Network.isClient)
		{
			if(player2 == null)
			{
				//DebugStreamer.message = "created player 2"; 
				player2 = (GameObject)Network.Instantiate(player2Prefab, new Vector3(10000, 0, 0), Quaternion.identity, 0);
			 	//player2.GetComponent<PlayerScript>().isLocalPlayer = false;
				DontDestroyOnLoad(player2);
			}
		}
		
		//DebugStreamer.message = "in game logic..." + Time.realtimeSinceStartup.ToString();
	}	
}
