using UnityEngine;
using System.Collections;

public class caveLogic : MonoBehaviour
{
	public float currentCaveAlpha = 0.0f;
	public Terrain terrainSystem = null;
	public Camera caveCam = null;
	public Light light = null;
	
	// Use this for initialization
	void Start ()
	{
	
	}
	
	
	public bool FadeIn = false;
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		if(FadeIn)
		{
			currentCaveAlpha = Mathf.Lerp(currentCaveAlpha, 1.0f, Time.fixedDeltaTime*0.05f);
		}
		else
		{
			currentCaveAlpha = Mathf.Lerp(currentCaveAlpha, 0.0f, Time.fixedDeltaTime*0.05f);
		}
		
		
		caveCam.farClipPlane = Mathf.Lerp(300, 1500, currentCaveAlpha*currentCaveAlpha);
		
		
		float x = light.transform.position.x;
		float y = light.transform.position.y;
		float z = light.transform.position.z;
		x += Mathf.Cos(Time.timeSinceLevelLoad*0.3473f) * 15.0f;
		z += Mathf.Cos(Time.timeSinceLevelLoad*0.4374f) * 15.0f;
		light.transform.position = new Vector3(x,y,z);
	}
}
