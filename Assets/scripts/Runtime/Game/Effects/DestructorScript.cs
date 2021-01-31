// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Collections;
using UnityEngine;

namespace Pon
{
  public class DestructorScript : MonoBehaviour
  {
    #region Members

    public float timeToLive = 3f;

    #endregion

    #region Timeline

    void Start()
    {
      Destroy(this.gameObject, timeToLive);
    }

    #endregion
  }
}