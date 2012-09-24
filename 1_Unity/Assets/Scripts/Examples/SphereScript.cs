using UnityEngine;
using System.Collections;

public class SphereScript : MonoBehaviour {
	
	private Vector3 redColor;
	private Vector3 greenColor;
	
	void Start () {
		redColor = new Vector3(1,0,0);
		greenColor = new Vector3(0,1,0);
	}
	
	void Update () {
	
	}
	
	void OnTriggerEnter ( ) {
		networkView.RPC("SetColorRed", RPCMode.AllBuffered, redColor);
	}
	
	void OnTriggerExit ( ) {
		networkView.RPC("SetColorGreen", RPCMode.AllBuffered, greenColor);
	}
	
	[RPC] void SetColorRed (Vector3 redColor) {
		renderer.material.color = new Color(redColor.x, redColor.y, redColor.z,1);
	}
	
	[RPC] void SetColorGreen (Vector3 greenColor) {
		renderer.material.color = new Color(greenColor.x, greenColor.y, greenColor.z,1);
	}
}
