using UnityEngine;
using UnityEngine.UI;

public class BackgroundReveal : MonoBehaviour
{
	public static void RevealMap(Image mask, float progression)
	{
		mask.fillAmount = progression;
	}
}

