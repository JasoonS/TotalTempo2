using UnityEngine;

public class Rotator : MonoBehaviour
{
	void Update()
	{
		float x = Mathf.Pow(Random.Range(0, 1f), 3) * 200;
		float y = Mathf.Pow(Random.Range(0, 1f), 3) * 200;
		float z = Mathf.Pow(Random.Range(0, 1f), 3) * 200;

		float speed = Random.Range(0.1F, 6);

		transform.Rotate(new Vector3(x,y,z) * Time.deltaTime*speed);
	}
}