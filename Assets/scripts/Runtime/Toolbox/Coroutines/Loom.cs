using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class Loom : MonoBehaviour
{
  public struct DelayedQueueItem
  {
    public float time;
    public Action action;
  }

  public static int MAX_THREADS = 8;

  private static Loom _current;
  private static int numThreads;
  private static bool initialized;

  public static Loom Current
  {
    get
    {
      Initialize();
      return _current;
    }
  }

  private List<Action> _actions = new List<Action>();
  private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();
  private List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
  private List<Action> _currentActions = new List<Action>();

  void Awake()
  {
    _current = this;
    initialized = true;
  }

  void OnDisable()
  {
    if (_current == this)
    {
      _current = null;
    }
  }

  void Update()
  {
    lock (_actions)
    {
      _currentActions.Clear();
      _currentActions.AddRange(_actions);
      _actions.Clear();
    }

    foreach (var a in _currentActions)
    {
      a();
    }

    lock (_delayed)
    {
      _currentDelayed.Clear();
      _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));

      foreach (var item in _currentDelayed)
      {
        _delayed.Remove(item);
      }
    }

    foreach (var delayed in _currentDelayed)
    {
      delayed.action();
    }
  }

  public static void Initialize()
  {
    if (initialized == false || _current == null)
    {
      if (Application.isPlaying == false)
      {
        return;
      }

      initialized = true;

      var g = new GameObject("Threads");
      _current = g.AddComponent<Loom>();
      DontDestroyOnLoad(g);
    }
  }

  public static void RunMain(Action action)
  {
    RunMain(action, 0f);
  }

  public static void RunMain(Action action, float time)
  {
    if (time != 0)
    {
      lock (Current._delayed)
      {
        Current._delayed.Add(new DelayedQueueItem {time = Time.time + time, action = action});
      }
    }
    else
    {
      lock (Current._actions)
      {
        Current._actions.Add(action);
      }
    }
  }

  public static Thread RunAsync(Action a)
  {
    Initialize();

    while (numThreads >= MAX_THREADS)
    {
      Thread.Sleep(1);
    }

    Interlocked.Increment(ref numThreads);
    ThreadPool.QueueUserWorkItem(RunAction, a);

    return null;
  }

  public static Coroutine RunCoroutine(IEnumerator coroutine)
  {
    return Current.StartCoroutine(coroutine);
  }

  private static void RunAction(object action)
  {
    try
    {
      ((Action) action)();
    }
    catch
    {
    }
    finally
    {
      Interlocked.Decrement(ref numThreads);
    }
  }

  public static void DelayFrames(int frames, Action callback)
  {
    Loom.RunCoroutine(WaitFramesAndExecute(frames, callback));
  }

  private static IEnumerator WaitFramesAndExecute(int frames, Action callback)
  {
    for (int i = 0; i < frames; i++)
    {
      yield return new WaitForEndOfFrame();
    }

    callback?.Invoke();
  }
}