using UnityEngine;
using System.Collections;

public class GameLogicController : MonoBehaviour
{
	bool isPlayer1 = false;
	
	
	public GameObject player1Prefab;
	public GameObject player2Prefab;
	
	private GameObject player1 = null;
	private GameObject player2 = null;	
	
	void Start()
	{		
	
	}
	
	void OnEnable()
	{
		networkView.observed = this;
	}
	
	void OnDisable()
	{

	}
	
	void Update()
	{
		if(Network.isServer)
		{
			if(player1 == null)
			{
				DebugStreamer.message = "created player 1";
				player1 = (GameObject)Network.Instantiate(player1Prefab, new Vector3(0,0,0), Quaternion.identity, 0);				
				isPlayer1 = true;
			}
		}
		else if(Network.isClient)
		{		
			if(player2 == null)
			{
				DebugStreamer.message = "created player 2";
				player2 = (GameObject)Network.Instantiate(player2Prefab, new Vector3(0,0,0), Quaternion.identity, 0);
				isPlayer1 = false;
			}
		}
		
		
		//DebugStreamer.message = "in game logic..." + Time.realtimeSinceStartup.ToString();
	}	
}
