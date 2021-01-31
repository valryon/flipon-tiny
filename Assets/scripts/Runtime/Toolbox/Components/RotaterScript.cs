// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  /// <summary>
  /// Simply rotate the current game object
  /// </summary>
  public class RotaterScript : MonoBehaviour
  {
    public float speed = 0.1f;

    public bool randomDirection = true;

    public Vector3 axis = new Vector3(0, 0, 1);

    public virtual void Start()
    {
      if (randomDirection)
      {
        if (Random.Range(0, 2) == 0)
        {
          speed = -speed;
        }
      }
    }

    public virtual void Update()
    {
      this.transform.Rotate(axis, speed, Space.Self);
    }
  }
}