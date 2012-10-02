using UnityEngine;
using System.Collections;

public class MovementScript : MonoBehaviour {
	
	Vector3 moveToPos;
	
	public float speed;
	
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
	
	#region Finger Gestures
	
	void OnFingerDown (int finger, Vector2 pos) {
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(pos.x, pos.y, 0));
		Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit)){
			moveToPos = hit.point;	
//			print(moveToPos);
			if (networkView.isMine) { // super important, makes sure this is infact "our" player, not some other player
				iTween.MoveTo(gameObject, new Vector3(moveToPos.x,transform.position.y,moveToPos.z), 100);
			}
		}
	}
	
	#endregion
	
}
