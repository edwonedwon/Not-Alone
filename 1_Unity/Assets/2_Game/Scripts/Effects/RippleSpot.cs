using UnityEngine;
using System.Collections;

public class RippleSpot : MonoBehaviour
{
	
	public tk2dAnimatedSprite sprite = null;
	
	public float rampUpTime = 0.5f;
	public float holdAmount = 1.0f;
	public float degradeTime = 0.25f;
	public float goalBrightness = 6.0f;
	private float curTimer = 0.0f;
	
	void Start ()
	{
		sprite.animationCompleteDelegate += AnimationComplete;
	}
	
	
	public void AnimationComplete (tk2dAnimatedSprite touchAnim, int clipId)
	{
		Destroy(gameObject);
	}
	
	void FixedUpdate ()
	{
		float fixedDt = Time.fixedDeltaTime;
		curTimer += fixedDt;
	}
}
