using UnityEngine;

public class MapLvlButton : MonoBehaviour
{
  [SerializeField] int level;
  [SerializeField] bool isUnlocked = true;

  SpriteRenderer spriteRenderer;

  private void Awake()
  {
    spriteRenderer = this.GetComponent<SpriteRenderer>();
    ShowLockStatus();
  }

  public int GetLevel()
  {
    return level;
  }
  public bool GetUnlocked()
  {
    return isUnlocked;
  }

  public void SetUnlocked(bool desValue)
  {
    isUnlocked = desValue;
    ShowLockStatus();
  }

  public void ShowLockStatus()
  {
    if(spriteRenderer == null)
    {
      spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    if (!isUnlocked)
    {
      spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f);
    }
    else
    {
      spriteRenderer.color = new Color(1, 1, 1);
    }
  }
}
