// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  /// <summary>
  /// 
  /// </summary>
  /// <remarks>Source: http://answers.unity3d.com/questions/23291/same-visible-area-regardless-of-aspect-ratio.html </remarks>
  [RequireComponent(typeof(Camera))]
  public class LetterBoxing : MonoBehaviour
  {
    public float ratioToKeep = 4 / 3f;

    private Camera cam;

    void Start()
    {
      float aspectRatio = Screen.width / ((float) Screen.height);
      float percentage = 1 - (aspectRatio / ratioToKeep);

      cam = GetComponent<Camera>();

      cam.rect = new Rect(0f, (percentage / 2), 1f, (1 - percentage));
    }
  }
}