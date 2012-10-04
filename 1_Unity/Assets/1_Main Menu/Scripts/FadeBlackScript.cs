using UnityEngine;
using System.Collections;

public class FadeBlackScript : MonoBehaviour {
	
	private tk2dSprite blackSprite;
	
	private float blackAlpha;
	
	public float fadeUpSpeed;

	void Start () {
		
		blackAlpha = 1;
		
		blackSprite = gameObject.GetComponent<tk2dSprite>();
	
	}
	
	void Update () {
		
		
		
		blackSprite.color = new  Color(blackSprite.color.r,blackSprite.color.g,blackSprite.color.b,blackAlpha);
	}
	
	void FixedUpdate () {
		blackAlpha -= fadeUpSpeed/1000;
	}
}
