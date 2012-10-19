using UnityEngine;
using System.Collections;

public class LenseFlareBurst : MonoBehaviour
{
	public LensFlare TheFlare = null;
	
	public float rampUpTime = 0.5f;
	public float holdAmount = 1.0f;
	public float degradeTime = 0.25f;
	public float goalBrightness = 6.0f;
	private float curTimer = 0.0f;
	
	void Start ()
	{
	
	}
	
	
	void FixedUpdate ()
	{
		float fixedDt = Time.fixedDeltaTime;
		curTimer += fixedDt;
		
		float delta = 0.0f;
		if(curTimer < rampUpTime)
		{
			delta = curTimer / rampUpTime;
			TheFlare.brightness = Mathf.Lerp(0, goalBrightness, delta);
		}
		if(curTimer > rampUpTime)
		{
			delta = (curTimer - rampUpTime) / holdAmount;			
		}
		if(curTimer > rampUpTime+holdAmount)
		{
			delta = (curTimer-(rampUpTime+holdAmount)) / degradeTime;
			TheFlare.brightness = Mathf.Lerp(goalBrightness, 0, delta);
			if(delta > 1.0f)
				Destroy(gameObject);			
		}	
	}
}
