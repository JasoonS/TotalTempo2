using UnityEngine;
using UnityEngine.UI;

public class SliderDisplay : MonoBehaviour
{
	public int _counter;
	public int _currentCount;

	public Slider CountSliderOne;
	public Slider CountSliderTwo;

	void Start()
	{
		CountSliderOne.value = 10;
	}

	void Update()
	{
		CountSliderOne.value = _currentCount;
		CountSliderOne.maxValue = _counter;
	}	     
}