using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// allows you to see the 'live' effects of editing
[ExecuteInEditMode]
public class PestEffectScript : MonoBehaviour {

	public Material mat;
  public GameObject ToggleButtonObject;
	public RectTransform ParentPanel;

	Button ToggleButton;

	void Start() {
		ToggleButton = ToggleButtonObject.GetComponent<Button> ();
		ToggleButton.onClick.AddListener(() => ChangeEffect());
		ToggleButton.GetComponentInChildren<Text>().text = "ToggleScreenWaveEffect";
	}

	void ChangeEffect() {
		mat.SetInt("_MustWave", 1 - mat.GetInt("_MustWave"));
	}

	void OnRenderImage( RenderTexture src, RenderTexture dest ) {
		Graphics.Blit (src, dest, mat);
	}
}
