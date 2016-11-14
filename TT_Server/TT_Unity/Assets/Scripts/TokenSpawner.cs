using System.Collections.Generic;

using UnityEngine;

public class TokenSpawner : MonoBehaviour
{
    public  GameObject TokenToSpawn;

    public List<Token> Tokens;
    public Material Green;
    public Material Blue;
    public Material Yellow;
    public Material Red;

    public float SpawnRadius;

    private int _numTokensSpawned = 0;

    private Colours[] _tokenColours = {
        Colours.GREEN,
        Colours.BLUE,
        Colours.YELLOW,
        Colours.RED
    };

    public enum Colours { RED, GREEN, BLUE, YELLOW, BLACK, WHITE };

    public int NumberOfCollectables = 10;

    Colours[] ColourList =
    {
        Colours.GREEN,
        Colours.BLUE,
        Colours.YELLOW,
        Colours.RED
    };

    private static TokenSpawner _tokenSpawner;

    // Use the static object pattern to guarantee that this object is correctly assigned and pressent in the scene.
    public static TokenSpawner Instance
    {
        get
        {
            if (!_tokenSpawner)
            {
                _tokenSpawner = FindObjectOfType(typeof(TokenSpawner)) as TokenSpawner;

                if (!_tokenSpawner)
                {
                    Debug.LogError("You need to have at least one active Event Manager script in your scene.");
                }
            }
            return _tokenSpawner;
        }
    }

    void Start()
    {
        float sr = SpawnRadius;

        Tokens = new List<Token>((int)(PathFinder.Instance.TotalLength / (sr * 2)) * NumberOfCollectables);

        _numTokensSpawned = 0;

        //TODO:: hardcoded for now. Fix script execution order to allow the read of spawnRadius on TokenSpawner.

        for (float dist = 0; dist < PathFinder.Instance.TotalLength; dist += SpawnRadius * 2)
        {
            spawnTokensArroundCentre(PathFinder.Instance.GetRoutePosition(dist + 1));
        }

    }

    public void spawnTokensArroundCentre(Vector3 centre)
    {
        for (int i = 0; i < NumberOfCollectables; i++)
        {
            float rotationVariance = Random.Range(-Mathf.PI / NumberOfCollectables, Mathf.PI / NumberOfCollectables);

            Vector3 spawnCentre = centre + new Vector3(
                Mathf.Sin((Mathf.PI * 2 / NumberOfCollectables) * i + rotationVariance),
                0.0f,
                Mathf.Cos((Mathf.PI * 2 / NumberOfCollectables) * i + rotationVariance)
            ) * SpawnRadius * Random.Range(0.3f, 1)
            + new Vector3(0, 2.0f, 0);

            Tokens.Add(new Token(spawnCentre, ColourList[_numTokensSpawned % ColourList.Length], _numTokensSpawned++));
        }
    }

    public static void RemoveWithId(int id)
    {
        Instance.Tokens.RemoveAt(id);

        // update the rest of the token ids.
        for (int x= id; x < Instance.Tokens.Count; x++)
        {
            Instance.Tokens[x].TokenScript.setid(x);
        }
    }

    public class Token
    {
        private float _Xpos;
        private float _Ypos;

        private Colours _tokenColour;

        public GameObject TokenObject;
        public TokenScript TokenScript;

        void SetColour(Colours type, Renderer rend)
        {
            switch (type)
            {
                case Colours.RED:
                    rend.sharedMaterial = Instance.Red;
                    break;

                case Colours.GREEN:
                    rend.sharedMaterial = Instance.Green;
                    break;

                case Colours.YELLOW:
                    rend.sharedMaterial = Instance.Yellow;
                    break;

                case Colours.BLUE:
                    rend.sharedMaterial = Instance.Blue;
                    break;

                default:
                    break;
            }
        }

        public Token(Vector3 position, Colours tokenColour, int index)
        {
            createToken(position, tokenColour, index);
        }

        void createToken(Vector3 position, Colours tokenColour, int index)
        {
            TokenObject = Instantiate(Instance.TokenToSpawn);
            TokenObject.transform.SetParent(Instance.transform, false);

            TokenObject.transform.position = position;

            TokenScript = TokenObject.GetComponent<TokenScript>();

            Renderer rend = TokenObject.GetComponent<Renderer>();

            SetColour(tokenColour, rend);

            TokenScript.setid(index);
            TokenScript.setColour(tokenColour);
        }
    }
}