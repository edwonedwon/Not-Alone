using UnityEngine;
using System.Collections;

public class DefaultCameraScript : MonoBehaviour
{
	public FluidFieldGenerator field = null;
	
	void Start ()
	{
	
	}
	
	void Update ()
	{
	
	}
	
	void OnPostRender()
	{		
		field.PostRenderParticles();
	}
}
