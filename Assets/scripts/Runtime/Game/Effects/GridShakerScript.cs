// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using UnityEngine;

namespace Pon
{
  public class GridShakerScript : MonoBehaviour
  {
    #region Members

    public float duration = 1f;
    public float force = 0.25f;

    private Vector3 previousMouvement = Vector3.zero;
    private Vector3 originalPosition;

    #endregion

    #region Timeline

    void Awake()
    {
      originalPosition = gameObject.transform.position;
      SetShake(duration, force);
    }

    private void SetShake(float d, float f)
    {
      // Reset before adding new shake
      gameObject.transform.position = originalPosition;

      duration = d;
      force = f;

      originalPosition = gameObject.transform.position;
    }

    void Update()
    {
      duration -= Time.deltaTime;
      if (duration < 0)
      {
        // Reset position
        gameObject.transform.position = originalPosition;

        // Destroy script
        Destroy(this);
        return;
      }

      // Cancel previous move
      gameObject.transform.position += -previousMouvement;

      // Get new move
      Vector3 movement = Random.insideUnitSphere * force;
      movement.z = 0;

      // Apply
      gameObject.transform.position += movement;

      // Save values
      previousMouvement = movement;
    }

    #endregion

    #region Public methods

    public static void Shake(GridScript grid, float duration, float force)
    {
      GridShakerScript shaker = grid.gameObject.AddOrGetComponent<GridShakerScript>();
      shaker.SetShake(duration, force);
    }

    #endregion
  }
}