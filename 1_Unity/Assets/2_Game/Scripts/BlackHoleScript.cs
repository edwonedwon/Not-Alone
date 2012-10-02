using UnityEngine;
using System.Collections;

public class BlackHoleScript : MonoBehaviour
{
	
	private FluidFieldGenerator fluidField = null;
	
	
	public int radius = 30;
	public float velocityPower = 190.0f;
	public float holePower = 100.0f;
	public float inkSpit = 0.0f;
	
	
	
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
		}
	}
	

	void OnEnable()
	{
		
	}
	
	void OnDisable()
	{

	}
	

}
