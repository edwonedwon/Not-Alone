using UnityEngine;
using System.Collections;

public class BlackHoleScript : MonoBehaviour
{	
	private FluidFieldGenerator fluidField = null;
	
	public int radius = 30;
	public float velocityPower = 190.0f;
	public float holePower = 100.0f;
	public float inkSpit = 0.0f;
	public float RotationSpeed = 100;
	
	void Start()
	{		
		
	}	
	
	void Update()
	{
		if(fluidField == null)
			fluidField = GameObject.Find("heightfield mesh").GetComponent<FluidFieldGenerator>();
		else
		{
			fluidField.UpdateBasedOnBlackHole(this);			
			
			//RotationSpeed -= 25.0f * Time.deltaTime;
			//DebugStreamer.message = "RotationSpeed: " + RotationSpeed.ToString();
			if(RotationSpeed < 0.0f)
				Destroy (gameObject);
		}
	}

	void OnEnable()
	{
		
	}
	
	void OnDisable()
	{

	}
	

}
