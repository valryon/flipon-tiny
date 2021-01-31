// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Collections.Generic;
using UnityEngine;

namespace Pon
{
  public enum InputType
  {
    None = 0,
    GamepadKeyboard = 1,
    TactileMouse = 2
  }

  public struct InputState
  {
    public Vector2 move;
    public bool action;
    public bool speed;
    public bool power;

    public bool AnyWasPressed => move != Vector2.zero || action || speed || power;

    /// <summary>
    /// ⚠ This is not code from the real game!
    /// The game use InControl, a premium plugin.
    /// This is just a simple 1P keyboard mapping.
    /// </summary>
    /// <returns></returns>
    public static InputState GetKeyboardState()
    {
      return new InputState()
      {
        action = Input.GetKeyDown(KeyCode.Space),
        speed = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl),
        power = Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt),
        move = new Vector2(
          (Input.GetKey(KeyCode.LeftArrow) ? -1 : 0) + (Input.GetKey(KeyCode.RightArrow) ? 1 : 0),
          (Input.GetKey(KeyCode.DownArrow) ? -1 : 0) + (Input.GetKey(KeyCode.UpArrow) ? 1 : 0)
        )
      };
    }
  }

  public class PlayerScript : MonoBehaviour
  {
    #region Members

    [Header("Data")]
    public Camera cam;

    public Player player;
    public Power power;
    public int lastPlayerTarget = -1;

    [Header("Bindings")]
    public GridScript grid;

    [Header("Prefabs")]
    public CursorScript[] cursorPrefabs;

    // Input data
    protected Dictionary<int, CursorScript> cursors = new Dictionary<int, CursorScript>();
    private Dictionary<int, Vector2> touchStarted = new Dictionary<int, Vector2>();
    private Dictionary<int, Vector2> touchHistory = new Dictionary<int, Vector2>();

    private InputType input;
    private bool canChangeInputMode;
    private float scrolled;

    private const float TACTILEMOUSE_FIRST_MOVE =
#if UNITY_ANDROID
0.53f;
#elif UNITY_IOS || UNITY_SWITCH
      // iOS devices are more sensitive. Threshold must be higher.
      0.75f;
#else
      0.6f;
#endif
    private const float TACTILEMOUSE_NEXT_MOUSE =
#if UNITY_ANDROID
0.48f;
#elif UNITY_IOS || UNITY_SWITCH
      0.65f;
#else
      0.6f;
#endif

    #endregion

    #region Timeline

    protected virtual void Start()
    {
      Log.Info("Player " + player);

      if (grid == null)
      {
        Log.Error(player.name = ": MISSING GRID§§§");
      }

      Log.Info("Player " + player + " initialized.");

      grid.OnGameOver += GameOver;

      Input.simulateMouseWithTouches = false;

      if (player.type == PlayerType.AI)
      {
        input = InputType.None;
        canChangeInputMode = false;
      }
      else
      {
#if UNITY_IOS || UNITY_ANDROID
        input = InputType.TactileMouse;
#else
        input = InputType.GamepadKeyboard;
#endif
        canChangeInputMode = true;
      }

      padCursorLoc = new Vector2(
        Mathf.Clamp(Mathf.Ceil((grid.settings.width / 2f) - 1), 0, grid.settings.width - 2),
        Mathf.Clamp(Mathf.Ceil((grid.settings.startLines / 2f) - 1), 0, grid.settings.height - 1)
      );
    }

    protected virtual void Update()
    {
      if (player.GameOver || grid.IsGameOver)
      {
        // Clear cursors
        if (cursors.Count > 0)
        {
          CleanCursors();
        }

        return;
      }

      bool inputHasChanged = DetectInputMode();

      if (IsLocked)
      {
        CleanCursors();
      }
      else
      {
        if (grid.IsPaused == false && inputHasChanged == false)
        {
          switch (input)
          {
            case InputType.GamepadKeyboard:
              UpdatePad();
              break;
            case InputType.TactileMouse:
              UpdateTactile();
              break;
          }

          UpdateCursors();
        }
      }
    }

    protected virtual void LateUpdate()
    {
      player.Score = grid.Score;
    }

    void OnDestroy()
    {
      if (grid.TheGrid != null)
      {
        grid.TheGrid.OnLineUp -= LineUp;
      }
    }

    #endregion

    #region Events

    private void GameOver(GridScript g)
    {
      CleanCursors();
    }

    #endregion

    #region Inputs

    protected bool DetectInputMode()
    {
      if (canChangeInputMode == false) return false;
      if (input == InputType.None) return false;
      if (player.GameOver) return false;

      bool change = false;

      if (input == InputType.GamepadKeyboard)
      {
        if (Input.touchCount > 0 || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
          // Touching the grid?
          if (grid.IsOnScreen(Input.mousePosition))
          {
            input = InputType.TactileMouse;
            change = true;
          }
        }
      }
      else if (input == InputType.TactileMouse)
      {
        if (InputState.GetKeyboardState().AnyWasPressed || Input.GetKeyDown(KeyCode.Escape) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.AltGr)
            || Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt)
            || Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl)
        )
        {
          input = InputType.GamepadKeyboard;
          change = true;
        }
      }

      if (change)
      {
        CleanCursors();

        Log.Debug("INPUT - " + player + " switched to " + input);
      }

      return change;
    }

    private void CleanCursors()
    {
      foreach (var c in cursors)
      {
        c.Value.Clean();
        c.Value.SetActive(false, false);
        Destroy(c.Value);
      }

      grid.TheGrid.OnLineUp -= LineUp;

      cursors.Clear();
    }

    #region Tactile

    protected void UpdateTactile()
    {
      if (grid.IsGameOver == false && grid.IsPaused == false && grid.IsStarted)
      {
        const int MOUSE_ID = 999;

        // Mouse
        if (Input.GetMouseButton(0))
        {
          if (touchStarted.ContainsKey(MOUSE_ID))
          {
            ProcessTactileInput(MOUSE_ID, Input.mousePosition);
          }
          else
          {
            var p = cam.ScreenToWorldPoint(Input.mousePosition);
            touchStarted.Add(MOUSE_ID, p);
            touchHistory.Add(MOUSE_ID, p);
          }
        }
        else
        {
          touchStarted.Remove(MOUSE_ID);
          touchHistory.Remove(MOUSE_ID);
        }

        if (Input.GetMouseButton(1))
        {
          scrolled = 1f;
          grid.SpeedScrollingUp();
          GameUIScript.GetSpeedButton(player.index).SetSpeedActivated(true);
        }
        else if (scrolled > 0)
        {
          scrolled = 0f;
          grid.StopSpeedScrollingUp();
          GameUIScript.GetSpeedButton(player.index).SetSpeedActivated(false);
        }

        if (Input.GetMouseButton(2))
        {
          var b = GameUIScript.GetPowerBar(player.index);
          if (b != null)
          {
            b.button.onClick.Invoke();
          }
        }

        // Tactile
        for (int i = 0; i < Input.touchCount; i++)
        {
          var t = Input.touches[i];

          if (t.phase == TouchPhase.Began)
          {
            touchStarted.Add(t.fingerId, cam.ScreenToWorldPoint(t.position));
            touchHistory.Add(t.fingerId, cam.ScreenToWorldPoint(t.position));
          }
          else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
          {
            touchStarted.Remove(t.fingerId);
            touchHistory.Remove(t.fingerId);
          }
          else
          {
            if (touchStarted.ContainsKey(t.fingerId))
            {
              ProcessTactileInput(t.fingerId, t.position);
            }
          }
        }
      }
    }

    private void ProcessTactileInput(int id, Vector2 position)
    {
      var startPosition = touchStarted[id];
      Vector2 currentPosition = cam.ScreenToWorldPoint(position);

      // Get the block. Either we have a cursor or we need to create one.
      BlockScript target = null;
      CursorScript c = null;

      // Input is in the player grid?
      if (grid.IsOnScreen(position) == false) return;

      // -- Existing cursor (and target)
      if (cursors.ContainsKey(id))
      {
        c = cursors[id];
        target = c.Target;

        if (target != null)
        {
          SetCursorPosition(c, target);
        }
      }

      // New cursor and new target
      if (target == null)
      {
        var b = GetTargetFromPosition(currentPosition);
        if (b != null && b.block.IsEmpty == false && grid.CanMove(b))
        {
          target = b;

          if (target != null)
          {
            c = CreateCursor(id, target);
          }
        }
      }

      // Apply movement
      if (target != null)
      {
        const float t1 = 0.45f; // Threshold first move
        const float t2 = 1f; // Threshold next moves

        bool moved = false;

        var previousPosition = touchHistory[id];

        // Get movement
        var prevMovement = GetDirection(currentPosition, previousPosition);
        var movement = (currentPosition - startPosition).x;

        // Get direction
        int GetDirection(Vector2 current, Vector2 start)
        {
          var m = (current - start).x;
          var d = 0;
          if (m < -0.001f) d = -1;
          if (m > 0.001f) d = 1;

          return d;
        }

        int direction = 0;
        if (movement < -0.001f) direction = -1;
        if (movement > 0.001f) direction = 1;

        int prevDirection = 0;
        if (prevMovement < -0.001f) prevDirection = -1;
        if (prevMovement > 0.001f) prevDirection = 1;

        // Debug.Log("movement=" + movement + " c.mc=" + c.movesCount + " dir=" + direction);

        // Changing direction = start as a new move
        if (direction != 0 && prevDirection != 0 && prevDirection != direction)
        {
          c.movesCount = 0;
          touchStarted[id] = currentPosition;
          touchHistory[id] = currentPosition;
          return;
        }


        // First move
        if (direction != 0)
        {
          if (c.movesCount == 0)
          {
            moved = Mathf.Abs(movement / t1) >= 1f;
          }
          else
          {
            // Other moves
            float width = direction > 0 ? 1f : 0;
            int count = Mathf.FloorToInt((Mathf.Abs(movement - t1 + width) / t2));
            int diff = count - c.movesCount;
            moved |= diff > 0;
          }
        }

        if (c != null && moved)
        {
          var dir = grid.transform.rotation * new Vector3(direction, 0, 0);

          int moveType = MoveAtPosition(target, dir);

          // Horizontal
          if (moveType == 1)
          {
            c.RegisterMove();
          }
        }

        touchHistory[id] = currentPosition;
      }
    }

    #endregion

    #region InControl

    const int FAKE_ID = 0;

    private Vector2 previousPadDirection;
    private Vector2 padCursorLoc;
    private BlockScript padBlock;
    private Vector2 padTimings, padCooldowns;

    // Update gamepads, real devices or virtual controllers (like ML agen or networkt)
    protected void UpdatePad()
    {
      if (grid.IsStarted)
      {
        // Always have a cursor
        if (cursors.Count == 0)
        {
          var block = grid.Get((int) padCursorLoc.x, (int) padCursorLoc.y);

          CreateCursor(FAKE_ID, block, true);

          grid.TheGrid.OnLineUp += LineUp;
        }

        var state = GetPadState();
        UpdatePlayer(state);

        UpdateCursorPadLoc();

        padBlock = grid.Get((int) padCursorLoc.x, (int) padCursorLoc.y);
      }
    }

    private void UpdatePlayer(InputState state)
    {
      // Move block
      var blockToMove = grid.Get((int) padCursorLoc.x, (int) padCursorLoc.y);

      if (state.action)
      {
        bool move = blockToMove != null;
        if (move)
        {
          int r = MoveAtPosition(blockToMove, new Vector3(1, 0));
          if (r == 0)
          {
            // Try the opposite way
            var b = grid.Get((int) padCursorLoc.x + 1, (int) padCursorLoc.y);
            if (b != null)
            {
              r = MoveAtPosition(b, new Vector3(-1, 0));

              if (r == 2)
              {
                padCursorLoc += new Vector2(-1, 0);
              }
            }
          }
        }
      }

      if (state.speed)
      {
        grid.SpeedScrollingUp();
        GameUIScript.GetSpeedButton(player.index).SetSpeedActivated(true);
      }
      else
      {
        grid.StopSpeedScrollingUp();
        GameUIScript.GetSpeedButton(player.index).SetSpeedActivated(false);
      }

      if (state.power)
      {
        grid.UsePower();
      }

      if (state.move != Vector2.zero)
      {
        // Limits
        if (padCursorLoc.x <= 0 && state.move.x < 0 || padCursorLoc.x >= grid.settings.width - 2 && state.move.x > 0)
          state.move.x = 0;
        if (padCursorLoc.y <= 0 && state.move.y < 0 || padCursorLoc.y >= grid.settings.height - 1 && state.move.y > 0)
          state.move.y = 0;

        // Move cursor
        var newCursorLoc = padCursorLoc + state.move;

        var newBlock = grid.Get((int) padCursorLoc.x, (int) padCursorLoc.y);
        if (newBlock != null)
        {
          padCursorLoc = newCursorLoc;
        }
      }
    }

    const float PAD_THRESHOLD_X = 0.50f;
    const float PAD_THRESHOLD_Y = 0.80f;
    const float PAD_REPEAT_THRESHOLD_X = 0.925f;
    const float PAD_REPEAT_THRESHOLD_Y = 0.925f;
    const float PAD_REPEAT_COOLDOWN_FIRST = 0.19f;
    const float PAD_REPEAT_COOLDOWN_NEXT = 0.075f;
    const float KEYBOARD_THRESHOLD_X = 0.5f;
    const float KEYBOARD_THRESHOLD_Y = 0.5f;
    const float KEYBOARD_REPEAT_THRESHOLD_X = 0f;
    const float KEYBOARD_REPEAT_THRESHOLD_Y = 0f;
    const float KEYBOARD_REPEAT_COOLDOWN_FIRST = 0.19f;
    const float KEYBOARD_REPEAT_COOLDOWN_NEXT = 0.06f;

    protected virtual InputState GetPadState()
    {
      var state = InputState.GetKeyboardState();
      var isKeyboard = true; // Gamepad removed in tiny
      var thresholdX = isKeyboard ? KEYBOARD_THRESHOLD_X : PAD_THRESHOLD_X;
      var thresholdY = isKeyboard ? KEYBOARD_THRESHOLD_Y : PAD_THRESHOLD_Y;
      var repeatCooldownFirst = isKeyboard ? KEYBOARD_REPEAT_COOLDOWN_FIRST : PAD_REPEAT_COOLDOWN_FIRST;
      var repeatCooldownNext = isKeyboard ? KEYBOARD_REPEAT_COOLDOWN_NEXT : PAD_REPEAT_COOLDOWN_NEXT;
      var repeatThresholdX = isKeyboard ? KEYBOARD_REPEAT_THRESHOLD_X : PAD_REPEAT_THRESHOLD_X;
      var repeatThresholdY = isKeyboard ? KEYBOARD_REPEAT_THRESHOLD_Y : PAD_REPEAT_THRESHOLD_Y;

      #region Movement

      Vector2 move = Vector3.zero;
      float rawValue = 0f;
      float repeatThreshold = 0f;

      // ------ X
      if (Mathf.Abs(state.move.x) > thresholdX)
      {
        rawValue = state.move.x;
        repeatThreshold = repeatThresholdX;
        if (state.move.x < 0)
        {
          move.x = -1;
        }
        else if (state.move.x > 0)
        {
          move.x = 1;
        }
      }
      else
      {
        padTimings.x = 0;
        padCooldowns.x = 0;
      }

      // ------ Y
      if (Mathf.Abs(state.move.y) > thresholdY)
      {
        rawValue = state.move.y;
        repeatThreshold = repeatThresholdY;
        if (state.move.y > 0)
        {
          move.y = 1;
        }
        else if (state.move.y < 0)
        {
          move.y = -1;
        }
      }
      else
      {
        padTimings.y = 0;
        padCooldowns.y = 0;
      }

      var previousMove = previousPadDirection;
      previousPadDirection = move;

      if (move.x != 0)
      {
        if (previousMove.x == move.x)
        {
          if (Mathf.Abs(rawValue) < repeatThreshold || padTimings.x < padCooldowns.x)
          {
            move.x = 0;
          }
          else
          {
            padCooldowns.x += repeatCooldownNext;
          }
        }
        else
        {
          padCooldowns.x = repeatCooldownFirst;
        }

        padTimings.x += Time.deltaTime;
      }

      if (move.y != 0)
      {
        if (previousMove.y == move.y)
        {
          if (padTimings.y < padCooldowns.y)
          {
            move.y = 0;
          }
          else
          {
            padCooldowns.y += repeatCooldownNext;
          }
        }
        else
        {
          padCooldowns.y = repeatCooldownFirst;
        }

        padTimings.y += Time.deltaTime;
      }

      // Grid rotation!
      var a = player.gridAngle;
      var m = move;
      if (a >= 0 && a < 90)
      {
        m = move;
      }
      else if (a >= 90 && a < 180)
      {
        m.x = -move.y;
        m.y = move.x;
      }
      else if (a >= 180 && a < 270)
      {
        m.x = -move.x;
        m.y = -move.y;
      }
      else if (a >= 270 && a < 360)
      {
        m.x = move.y;
        m.y = -move.x;
      }

      move = m;

      state.move = move;

      #endregion

      return state;
    }

    private void LineUp()
    {
      if (input == InputType.GamepadKeyboard)
      {
        UpdateCursorPadLoc();
      }
    }

    private void UpdateCursorPadLoc()
    {
      if (IsLocked) return;

      var cursor = cursors[FAKE_ID];

      padCursorLoc.x = Mathf.Clamp(padCursorLoc.x, 0, grid.settings.width);
      padCursorLoc.y = Mathf.Clamp(padCursorLoc.y, 0, grid.settings.height - 1);

      // Cursor may be out of grid because of line up
      for (int y = (int) padCursorLoc.y; y >= 0; y--)
      {
        padCursorLoc.y = y;
        var newBlock = grid.Get((int) padCursorLoc.x, y); // Grid map is not updated yet on line up...
        if (newBlock != null)
        {
          padCursorLoc.y = newBlock.block.y; // ...but blocks coordinates are!
          cursor.stickToTarget = false;
          SetCursorPosition(cursor, newBlock);
          break;
        }
      }
    }

    #endregion

    private int MoveAtPosition(BlockScript target, Vector3 dir)
    {
      int moveType = 0;

      if (target != null && target.block.IsEmpty == false && grid.CanMove(target))
      {
        if (Mathf.Abs(dir.x) > 0.25f)
        {
          int dirInt = (int) Mathf.Sign(dir.x);
          grid.Move(target.block, dirInt);
          moveType = 1;
        }
        else if (dir.y < -0.25f)
        {
          grid.ForceFall(target.block);
          moveType = 2;
        }
      }

      return moveType;
    }

    protected CursorScript CreateCursor(int id, BlockScript b, bool force = false)
    {
      CursorScript c;
      bool justCreated = false;
      if (cursors.TryGetValue(id, out c) == false)
      {
        var cursorPrefab = cursorPrefabs[0];
        foreach (var p in cursorPrefabs)
        {
          if (p.inputType == input)
          {
            cursorPrefab = p;
            break;
          }
        }

        // Create missing cursors
        var cGo = Instantiate(cursorPrefab.gameObject, grid.transform, true);
        cGo.transform.localRotation = Quaternion.identity;

        c = cGo.GetComponent<CursorScript>();
        c.stickToTarget = true;
        c.grid = grid;

        cursors.Add(id, c);
        justCreated = true;
      }

      if (c.Target == null || c.Target == b || force)
      {
        SetCursorPosition(c, b);
      }
      else
      {
        c.SetActive(false, !justCreated);
      }

      return c;
    }

    protected virtual void UpdateCursors()
    {
      foreach (var c in cursors)
      {
        if (input == InputType.TactileMouse)
        {
          // Disable useless cursors
          // (avoid delete, pooling is nice)
          bool activate = false;

          foreach (var m in touchStarted)
          {
            activate |= (m.Key == c.Key);
          }

          if (activate)
          {
            // Check block
            if (c.Value.Target == null || c.Value.Target.block.IsBeingRemoved ||
                c.Value.Target.block.FallDuration > 0.1f)
            {
              activate = false;
            }
          }

          if (activate == false)
          {
            c.Value.Clean();
            c.Value.SetActive(false);
          }
        }
        else if (input == InputType.GamepadKeyboard)
        {
          // Always on!
          c.Value.SetActive(true);
        }
      }
    }

    /// <summary>
    /// Get the current targeted tile from world position
    /// </summary>
    private BlockScript GetTargetFromPosition(Vector2 worldPosition)
    {
      // Raycast
      var collisions = Physics2D.OverlapPointAll(worldPosition);

      foreach (var col in collisions)
      {
        if (col != null)
        {
          var t = col.gameObject.GetComponent<BlockScript>();
          if (t != null)
          {
            return t;
          }
        }
      }

      return null;
    }

    private void SetCursorPosition(CursorScript c, BlockScript b)
    {
      c.Target = b;

      if (b != null && b.block != null)
      {
        c.SetActive(true);
      }
    }

    public void SetCursorPosition(BlockScript b)
    {
      padCursorLoc = new Vector2(
        Mathf.Max(0, Mathf.Min(b.block.x, grid.settings.width - 2)),
        Mathf.Max(0, Mathf.Min(b.block.y, grid.settings.height - 1))
      );
    }

    #endregion

    #region Properties

    public bool IsLocked { get; set; }

    #endregion
  }
}