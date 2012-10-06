using UnityEngine;
using System.Collections;


public class GameLogicController : MonoBehaviour
{
	bool isPlayer1 = false;
	
	public static GameLogicController instance = null;
	
	public GameObject player1Prefab;
	public GameObject player2Prefab;
	
	public GameObject gestureInitializer;
	public GameObject gestureManager;
	public GameObject networkManager;
	
	private GameObject player1 = null;
	private GameObject player2 = null;	
		

	private int currentLevelIdx = 0;
	public string[] levelProgression;
	
	void Start()
	{
		Camera camcam = Camera.main;
		float cameraAspect = camcam.aspect;
		float screenWidth = camcam.GetScreenWidth()-1;
		float screenHeight = camcam.GetScreenHeight()-1;		
		camcam.orthographicSize = screenHeight / 2.0f;
		
		if (instance == null)
		{ 
            instance = this;
            DontDestroyOnLoad (instance);
			
			Instantiate(gestureInitializer);
			Instantiate(gestureManager);
			Instantiate(networkManager);
			
			levelProgression = new string[] {"NotAlone", "Level2"};
        }
		else
		{
        	DestroyImmediate (this);
		}
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
		MoveToNextLevel();
	}
	
	public void MoveToNextLevel()
	{
		++currentLevelIdx;
		if(currentLevelIdx > 1)
			currentLevelIdx = 0;
		
		networkView.RPC ("SetLevel", RPCMode.AllBuffered, currentLevelIdx);
	}
	
	int curLevelPrefix = 0;
	
	[RPC]
	private void SetLevel(int idx)
	{
		//turn message sending off...
		Network.SetSendingEnabled(curLevelPrefix, false);
		Network.isMessageQueueRunning = false;
		
		Application.LoadLevel(levelProgression[idx]);
		
		//Okay turn us back on!
		Network.isMessageQueueRunning = true;
		Network.SetSendingEnabled(curLevelPrefix, true);
	}

	void CheckCurrentGameStateForNextLevel()
	{
		
	}
	
	void Update()
	{
		if(Network.isServer)
		{
			if(player1 == null)
			{
				if (!Application.loadedLevelName.Contains("Main Menu")) {
					DebugStreamer.message = "created player 1";
					player1 = (GameObject)Network.Instantiate(player1Prefab, new Vector3(0,0,0), Quaternion.identity, 0);				
					DontDestroyOnLoad(player1);
					isPlayer1 = true;
				}
			}	
		}
		else if(Network.isClient)
		{		
			if(player2 == null)
			{
				DebugStreamer.message = "created player 2"; 
				player2 = (GameObject)Network.Instantiate(player2Prefab, Vector3.zero, Quaternion.identity, 0);
				DontDestroyOnLoad(player2);
				isPlayer1 = false;
			}
		}
		
		//DebugStreamer.message = "in game logic..." + Time.realtimeSinceStartup.ToString();
	}	
}
