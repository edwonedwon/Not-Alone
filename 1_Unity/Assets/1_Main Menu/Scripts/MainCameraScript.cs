using UnityEngine;
using System.Collections;

public class MainCameraScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
		AnimateCamera();
	
	}
	
	// Update is called once per frame
	void Update () {
		
		transform.LookAt(GameObject.Find("Not Alone Title").transform.position);
		
//		if (Input.GetButtonDown("Jump") == true) {
//			LookAtLogo();
//		}
	}
	
	void AnimateCamera() {
		iTween.MoveAdd(gameObject,iTween.Hash("y",30,"time",15,"delay",0,
			"easetype", iTween.EaseType.easeInOutQuad, 
			"looptype",iTween.LoopType.pingPong));	
	}
	
//	void LookAtLogo () {
//		print("look at logo");
//		transform.LookAt(GameObject.Find("Not Alone Title").transform.position);
//	}
}
