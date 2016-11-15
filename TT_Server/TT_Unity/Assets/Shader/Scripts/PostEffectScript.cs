using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// allows you to see the 'live' effects of editing
[ExecuteInEditMode]
public class PostEffectScript : MonoBehaviour {

	public Material mat;
  	public GameObject ToggleButtonObject;
	public RectTransform ParentPanel;

	Button ToggleButton;

	void Start() {
	}

	public void ChangeEffect() {
		mat.SetInt("_MustWave", 1 - mat.GetInt("_MustWave"));
	}

	void OnRenderImage( RenderTexture src, RenderTexture dest ) {
		Graphics.Blit (src, dest, mat);
	}
}
