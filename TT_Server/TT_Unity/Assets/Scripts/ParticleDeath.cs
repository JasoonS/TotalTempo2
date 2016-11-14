using UnityEngine;

public class ParticleDeath : MonoBehaviour
{
    private ParticleSystem _particleSystem;

	// Use this for initialization

	void Start ()
    {
        _particleSystem = GetComponent<ParticleSystem>();
	}
	
	// Update is called once per frame

	void Update ()
	{
	    if (_particleSystem.isStopped)
	    {
	        Destroy(gameObject);
	    }
	}
}