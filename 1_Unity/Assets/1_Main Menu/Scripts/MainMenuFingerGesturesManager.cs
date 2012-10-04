using UnityEngine;
using System.Collections;

public class MainMenuFingerGesturesManager : MonoBehaviour {
	
	public GameObject playButton;

	void Start () {
	
	}
	
	void Update () {
	
	}
	
		
	#region Events
	
	void OnEnable () {
		FingerGestures.OnFingerDown += OnFingerDown; 
	}
	
	void OnDisable () {
		FingerGestures.OnFingerDown -= OnFingerDown;
	}
	
	#endregion
		
	void OnFingerDown (int finger, Vector2 pos) {
		
		RaycastHit hit;
		
		Physics.Raycast(Camera.main.ScreenPointToRay(pos), out hit);
		if (hit.collider != null) {
			if (hit.collider.gameObject == playButton) {
				print ("play button");
			}
		}
		
	}
}
