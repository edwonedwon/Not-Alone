using UnityEngine;
using System.Collections;

public class LineConnectorScript : MonoBehaviour
{
	
	public LineConnectorScript connecTo = null;
	public FluidFieldGenerator fluidField = null;
	
	public bool LineLinkEnabled = false;
	
	void Start ()
	{
	
		
	}
	
	
	
	void Update ()
	{
		if(fluidField == null)
			return;
		
		if(connecTo != null)
		{
			if(LineLinkEnabled && connecTo.LineLinkEnabled)
			{
				Vector2 v1 = Camera.main.WorldToViewportPoint(this.transform.position);
				Vector2 v2 = Camera.main.WorldToViewportPoint(connecTo.transform.position);
				
				float pixelx = 1.0f / Camera.main.GetScreenWidth() * 2;
				float pixely = 1.0f / Camera.main.GetScreenHeight() * 2;
				
				v1.x += UnityEngine.Random.Range(-pixelx, pixelx);
				v1.y += UnityEngine.Random.Range(-pixely, pixely);
				
				v2.x += UnityEngine.Random.Range(-pixelx, pixelx);
				v2.y += UnityEngine.Random.Range(-pixely, pixely);
				
				fluidField.InkAlongLine(v1.x, v1.y, v2.x, v2.y);
			}
		}
	}
}
