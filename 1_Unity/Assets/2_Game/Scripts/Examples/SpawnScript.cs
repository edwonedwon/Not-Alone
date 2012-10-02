using UnityEngine;
using System.Collections;

public class SpawnScript : MonoBehaviour {
	
	public GameObject playerPrefab;

	void Start () {
	
	}
	
	void Update () {
	
	}
	
	void spawnPlayer() {
		Network.Instantiate(playerPrefab, transform.position, Quaternion.identity, 0);
	}
	
	void OnServerInitialized () {
		spawnPlayer();
	}
	
	void OnConnectedToServer () {
		spawnPlayer();	
	}

}
