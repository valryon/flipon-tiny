using TMPro;
using UnityEngine;

namespace Pon
{
  [RequireComponent(typeof(TextMeshProUGUI))]
  public class TextBlinker : MonoBehaviour
  {
    public bool loop = true;
    public bool fade = true;
    public AnimationCurve fadeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Range(0, 1f)]
    public float alphaMin = 0.25f;

    [Range(0, 1f)]
    public float alphaMax = 1f;

    [Range(-1, 1f)]
    public float signStart = 1f;

    [Range(0, 5f)]
    public float duration = 1f;

    private float time;
    private float alpha;
    private float sign;
    private TextMeshProUGUI text;

    void Awake()
    {
      text = GetComponent<TextMeshProUGUI>();

      sign = Mathf.Sign(signStart);
      alpha = text.color.a;

      text.color = text.color.SetAlpha(alpha);
    }

    void Update()
    {
      time += Time.deltaTime;

      if (time > duration)
      {
        if (loop)
        {
          time = 0f;
          sign = -sign;
        }
        else
        {
          Destroy(this);
        }
      }

      if (fade)
      {
        float p = time / duration;
        if (sign > 0)
        {
          alpha = Mathf.Lerp(alphaMin, alphaMax, fadeCurve.Evaluate(p));
        }
        else
        {
          alpha = Mathf.Lerp(alphaMax, alphaMin, fadeCurve.Evaluate(p));
        }
      }
      else
      {
        alpha = sign > 0 ? alphaMax : alphaMin;
      }

      text.color = text.color.SetAlpha(alpha);
    }
  }
}