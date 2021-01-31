// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pon
{
  public class PowerUseFX : MonoBehaviour
  {
    const int STARS_COUNT = 9;
    const int STARS_BATCHES = 3;

    public Sprite star;

    private List<GameObject> stars = new List<GameObject>();

    public void Activate()
    {
      // Create stars
      for (int i = 0; i < STARS_COUNT * STARS_BATCHES; i++)
      {
        GameObject s = CreateStar(i);

        stars.Add(s);
      }

      StartCoroutine(AnimateStars());

      // Clean
      Invoke("AutoDestruction", 10f);
    }

    private GameObject CreateStar(int i)
    {
      GameObject s = new GameObject("Star#" + i);
      s.transform.position = transform.position;
      s.transform.localScale = Vector3.one * 0.25f;

      var r = s.AddComponent<SpriteRenderer>();
      r.sortingLayerName = "UI";
      r.sprite = star;
      return s;
    }

    private IEnumerator AnimateStars()
    {
      int n = 0;
      float delta = 0;
      foreach (var s in stars)
      {
        float rotation = delta + ((n + 1) * (360f / STARS_COUNT));

        StartCoroutine(AnimateStar(s, rotation));

        n++;

        if (n % STARS_COUNT == 0)
        {
          yield return new WaitForSeconds(0.15f);
          delta += 15;
        }
      }

      yield return null;
    }

    private IEnumerator AnimateStar(GameObject s, float angle)
    {
      var rotater = s.AddComponent<RotaterScript>();
      rotater.speed = 1f;

      const float MOVE_FROM_CENTER_DURATION = 0.45f;
      const float DISTANCE = 3.75f;
      float t = 0f;

      Vector3 start = s.transform.position;
      Quaternion angleQ = Quaternion.Euler(0, 0, angle);
      Vector3 end = start + (angleQ * new Vector3(Random.Range(DISTANCE * 0.975f, DISTANCE * 1.025f), 0));

      while (t < MOVE_FROM_CENTER_DURATION)
      {
        yield return new WaitForEndOfFrame();

        t += Time.deltaTime;
        float p = Interpolators.EaseOutCurve.Evaluate(t / MOVE_FROM_CENTER_DURATION);

        s.transform.position = Vector3.Lerp(start, end, p);
        rotater.speed = Mathf.Lerp(0f, 10f, p);
      }

      yield return new WaitForSeconds(0.42f);

      const float FLY_DURATION = 0.45f;
      t = 0;
      start = s.transform.position;
      end = start + (angleQ * new Vector3(10, 0));
      Vector3 startScale = s.transform.localScale;

      while (t < FLY_DURATION)
      {
        yield return new WaitForEndOfFrame();

        t += Time.deltaTime;
        float p = Interpolators.EaseInCurve.Evaluate(t / FLY_DURATION);

        s.transform.position = Vector3.Lerp(start, end, p);
        s.transform.localScale = Vector3.Lerp(startScale, Vector3.one, p * 3f);
      }

      Destroy(s);
    }

    // Enemy effect

    public void SendTo(GridScript gridScript, Player sender)
    {
      Loom.RunCoroutine(AnimateSend(gridScript, sender));
    }

    private IEnumerator AnimateSend(GridScript gridScript, Player sender)
    {
      for (int i = 0; i < 10; i++)
      {
        var s = CreateStar(100 + i);
        s.transform.localScale = Vector3.one * Random.Range(0.25f, 0.69f);

        var rotater = s.AddComponent<RotaterScript>();
        rotater.speed = 5f;

        // s.GetComponent<SpriteRenderer>().color = color;

        Loom.RunCoroutine(AnimateSendSingle(gridScript, s));
        yield return new WaitForSeconds(Random.Range(0.015f, 0.135f));
      }

      yield return null;
    }

    private static IEnumerator AnimateSendSingle(GridScript gridScript, GameObject s)
    {
      const float DURATION = 1.35f;
      float t = 0f;

      Vector3 start = gridScript.transform.position
                      + new Vector3(gridScript.settings.width, gridScript.settings.height, 0)
                      + RandomEx.GetVector3(0f, 4f, 0f, 4f, 0, 0);
      Vector3 end = start + new Vector3(-12, -13, 0);

      while (t < DURATION)
      {
        t += Time.deltaTime;
        float p = Interpolators.EaseOutCurve.Evaluate(t / DURATION);

        s.transform.position = Vector3.Lerp(start, end, p);

        yield return new WaitForEndOfFrame();
      }

      Destroy(s);
    }

    public void AutoDestruction()
    {
      foreach (var s in stars)
      {
        if (s != null) Destroy(s);
      }
    }
  }
}