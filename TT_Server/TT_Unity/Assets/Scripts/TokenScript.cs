using UnityEngine;

public class TokenScript : MonoBehaviour
{
	public Renderer Render;
	public GameObject GameObj;

    private int _id;

    private TokenSpawner.Colours _tokenColour;

    public GameObject TokenParticlePrefab;

    void Start()
    {
        Render = GetComponent<Renderer> ();
		Render.enabled = true;
	}

	void OnTriggerEnter(Collider collision)
    {
        Instantiate(TokenParticlePrefab, GameObj.transform.position, GameObj.transform.rotation);

        TokenSpawner.RemoveWithId(_id);

        Destroy(gameObject);
            
        GridDisplay.HitTokenUpdate(collision.gameObject.GetComponent<HoverMotor>()._id, Metronome.CurTokenCycle, _tokenColour);
	}

    public void setid(int tokenid)
    {
        _id = tokenid;
    }

    public void enableToken()
    {
        Render.enabled = true;
    }

    public void disableToken()
    {
        Render.enabled = false;
    }

    public void setColour(TokenSpawner.Colours tokenCol)
    {
        _tokenColour = tokenCol;
    }
}