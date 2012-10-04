using UnityEngine;
using System.Collections;

public class BGParticlesScript : MonoBehaviour {
	
	ParticleSystem particles;
	
	public float particleStartSize = 100f;

	void Start () {
		particles = gameObject.GetComponent<ParticleSystem>();
		
		Hashtable param = new Hashtable();
		param.Add("from", 500f);
		param.Add("to", 45f);
		param.Add("time", 5f);
		param.Add("easyType", iTween.EaseType.easeOutQuad);
		param.Add("onupdate", "TweenedParticleStartSize");
		iTween.ValueTo(gameObject, param);
	}
	
	void Update () {
		particles.startSize = particleStartSize;
	}
	
	void FixedUpdate () {
//		particles.startSize += 1f;
	}
	
	public void TweenedParticleStartSize (float val){
		particleStartSize = val;
	}
	
}
