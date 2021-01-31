// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

namespace Pon
{
  /// <summary>
  /// The game core.
  /// </summary>
  public class GridScript : MonoBehaviour
  {
    #region Constants

    private const int BASE_COMBO_PER_LEVEL = 12;

    private const int MAX_LEVEL = 50;

    // Score for each combo (size doesn't matter)
    private const int BASE_SCORE_PER_COMBO = 100;

    // Bonus score for each block > 3
    private const int BONUS_SCORE = 50;

    // Bonus score for chains
    private const int BONUS_SCORE_CHAINS = 500;

    // % of power won with a 3-combo
    private const float POWER_ADD_LOW = 0.04f;

    // % of power won with a 4-combo
    private const float POWER_ADD_MED = 0.0625f;

    // base % of power won with a 5+-combo
    private const float POWER_ADD_HIGH = 0.125f;
    private const float POWER_ADD_HIGH_BONUS = 0.125f;

    // Base last chance duration in seconds
    private const float LAST_CHANCE_DURATION = 1.5f;

    private const float GARBAGE_COOLDOWN = 1.5f;
    private const float GARBAGE_MAX_COOLDOWN = 3f;
    private const float GARBAGE_RETRY_COOLDOWN = 0.5f;

    #endregion

    #region Members

    public Camera gridCam;
    public Canvas ui;

    [HideInInspector]
    public GridSettings settings;

    [HideInInspector]
    public Rect viewportRect = new Rect(0, 0, 1, 1);

    [HideInInspector]
    public float angle = 0;

    [HideInInspector]
    public Player player;

    [HideInInspector]
    public PlayerScript playerScript;

    [HideInInspector]
    public int targetHeight;

    public GameObject targetHeightBar;

    [System.NonSerialized]
    public bool enablePower;

    public event System.Action<GridScript> OnGameOver;
    public event System.Action<GridScript, ComboData> OnCombo;
    public event System.Action<GridScript, Vector2, int> OnMove;
    public event System.Action<GridScript, long> OnScoreChanged;
    public event System.Action<GridScript, PowerType, float, int> OnPowerChargeChanged;
    public event System.Action<GridScript, int> OnLevelChanged;
    public event System.Action<GridScript, float> OnSpeedUp;
    public event System.Func<GridScript, BlockScript, bool> OnCanMoveBlock;
    public event System.Action<GarbageStored> OnGarbageStored;
    public event System.Action<GarbageStored> OnGarbageAdded;
    public event System.Action<GridScript, int> OnMultiplierChange;

    // Core
    private Grid grid;
    private Dictionary<Block, BlockScript> blocks;
    private Grid.MoveResult lastMove;

    // Garbages
    private List<GarbageStored> garbages = new List<GarbageStored>();
    private float garbageCooldown, garbageBaseCooldown;

    // Draw settings
    private SpriteRenderer backgroundSprite;
    private SpriteRenderer[] columnSprites;
    private Color[] columnSpritesColors;

    private float speedUpCooldown;
    private float currentSpeed;
    private float currentSpeedBonus = 1f;
    private bool bypassFrozen;
    private bool isSpeeding;

    // Game progression
    private int level = 1;
    private long score;
    private int comboMultiplier;
    private int multiplierFrame = -1;
    public int totalChains;
    private int chainCount;
    private int moves;
    private bool isPaused, isGameOver;

    private int totalCombos, total4Combos, total5Combos, total6Combos;

    private float timeBeforeGameOverMax = LAST_CHANCE_DURATION;
    private float timeBeforeGameOverCurrent = LAST_CHANCE_DURATION;

    // Effects
    private GameObject background;

    #endregion

    #region Timeline

    private void Start()
    {
      // Load settings
      if (settings == null)
      {
        settings = new GridSettings();
        Log.Warning("No settings provided. Using default.");
      }

      ScaleTime = 1f;

      // Scroll
      level = settings.startLevel;
      SetSpeedForLevel();
      currentSpeed = ScrollingSpeed * currentSpeedBonus;

      // Create grid
      grid = new Grid(transform.position, settings.width, settings.height, settings.previewLines,
        BlockDefinitionBank.Instance);
      grid.OnComboDetected += ComboDetected;
      grid.OnFallEnd += FallEnd;
      grid.OnLineUp += OnLineUp;


      grid.Start(level, settings.startLines);
      ApplyGridSettings();

      blocks = new Dictionary<Block, BlockScript>();

      // Create block scripts
      var blocksGo = new GameObject("Blocks");
      blocksGo.transform.parent = transform;
      blocksGo.transform.position = transform.position;

      foreach (var b in grid.Map.Values)
      {
        var bgo = new GameObject("Block");
        bgo.transform.parent = blocksGo.transform;
        bgo.transform.localPosition = b.position;

        var bs = bgo.AddComponent<BlockScript>();
        bs.SetBlock(b, grid.height, !settings.noScrolling);

        blocks.Add(b, bs);
      }

      // Set in a proper rect
      SetViewport(viewportRect, angle);

      CreateBackground();

      gameObject.SetActive(true);
    }

    private void ApplyGridSettings()
    {
      currentSpeed = ScrollingSpeed * currentSpeedBonus;
    }

    private void CreateBackground()
    {
      if (background != null)
      {
        Destroy(background);
      }

      // Camera dimension
      var camCenter = gridCam.transform.position;
      camCenter.z = 0;

      // Create background sprite
      background = new GameObject("Background");
      background.transform.parent = transform.parent;
      background.name = "Background";
      background.transform.localPosition = camCenter;

      backgroundSprite = background.AddComponent<SpriteRenderer>();
      backgroundSprite.sortingLayerName = "Background";
      backgroundSprite.sprite = player.background;

      // Adapt size to camera
      float missingHeight = (gridCam.orthographicSize * 2) - backgroundSprite.size.y;
      if (missingHeight > 0)
      {
        var scale = (gridCam.orthographicSize * 2) / backgroundSprite.size.y;
        background.transform.localScale = Vector3.one * scale;
      }

      // Create columns
      List<SpriteRenderer> columns = new List<SpriteRenderer>();
      int ci = 0;
      for (int x = -1; x <= grid.width; x++)
      {
        var col = new GameObject("Column " + x);
        col.transform.parent = transform;
        col.transform.localPosition = new Vector3(x + 0.5f, 0, -5);
        col.transform.localScale = Vector3.zero;

        var s = col.AddComponent<SpriteRenderer>();
        s.sprite = TextureEx.Blank(1, 1).ToSprite(1);
        s.sortingLayerName = "Background";
        s.sortingOrder = 10;

        var alphas = new[] {0.065f, 0.045f, 0.03f, 0.05f, 0.07f, 0.05f, 0.03f, 0.05f, 0.03f, 0.04f, 0.065f};
        var a = alphas[ci % alphas.Length] * 4f;
        if (x == -1 || x == grid.width)
        {
          a = 0f;
        }

        s.color = new Color(1f, 1f, 1f, a);

        var col2D = col.AddComponent<BoxCollider2D>();
        col2D.isTrigger = true;
        col2D.size = Vector2.one;

        columns.Add(s);
        ci++;
      }

      columnSprites = columns.ToArray();
      columnSpritesColors = columns.Select(c => c.color).ToArray();
    }

    private void Update()
    {
      UpdateBackgroundColor();

      if (isGameOver) return;
      if (IsStarted == false) return;
      if (isPaused) return;

      UpdateGrid();
      UpdateGarbageGeneration();

      if (grid.IsFrozen == false)
      {
        // Reset everything
        if (comboMultiplier > 0)
        {
          comboMultiplier = 0;
          OnMultiplierChange?.Invoke(this, comboMultiplier);
        }

        chainCount = 0;
        bypassFrozen = false;
      }
    }

    public void ResetCounters()
    {
      comboMultiplier = 0;
      chainCount = 0;
    }

    private void UpdateGrid()
    {
      if (speedUpCooldown > 0)
      {
        speedUpCooldown -= DeltaTime;

        // Disable speed up
        if (speedUpCooldown <= 0)
        {
          speedUpCooldown = 0f;
          currentSpeed = ScrollingSpeed;
        }
      }

      float scrolling = (currentSpeed * currentSpeedBonus) * DeltaTime;
      scrolling = Mathf.Min(scrolling, 0.75f); // Scrolling hard limit

      if ((grid.IsFrozen && !bypassFrozen && speedUpCooldown <= 0f) || grid.IsTopFull)
      {
        scrolling = 0;
      }

      bool wasFull = grid.IsTopFull;

      grid.Update(scrolling, DeltaTime, !FreezeCombos, settings.noScrolling);

      if (targetHeightBar != null)
      {
        targetHeightBar.transform.position = new Vector3(targetHeightBar.transform.position.x,
          targetHeight + grid.ScrollSinceLastDing);
      }

      if (grid.IsTopFull)
      {
        UpdateGridFull(wasFull, grid.IsTopFull);

        var p = (timeBeforeGameOverCurrent / timeBeforeGameOverMax);
        int dangerHeight = (int) (grid.height * p);

        foreach (var b in blocks.Values)
        {
          b.DangerHeight = dangerHeight;
        }
      }
      else
      {
        foreach (var b in blocks.Values)
        {
          b.DangerHeight = grid.height;
        }
      }
    }

    private void UpdateGridFull(bool wasFull, bool isFull)
    {
      if (settings.noScrolling == false)
      {
        // Timer stuff
        if (wasFull == false && isFull)
        {
          timeBeforeGameOverMax = LAST_CHANCE_DURATION;
          timeBeforeGameOverCurrent = timeBeforeGameOverMax;
        }

        else if (isFull && !grid.IsFrozen)
        {
          float factor = isSpeeding ? 2 : 1;
          timeBeforeGameOverCurrent -= (DeltaTime * factor);

          if (timeBeforeGameOverCurrent <= 0)
          {
            // :(
            Log.Warning(" GAME OVER for " + player);
            SetGameOver();
            OnGameOver.Raise(this);
          }
        }
      }
    }

    private float previousGameOverPercent;

    private void UpdateBackgroundColor()
    {
      var percent = 1f - (timeBeforeGameOverCurrent / timeBeforeGameOverMax);
      if (isGameOver)
      {
        percent = 1f;
      }

      if (grid.IsTopFull && (percent > previousGameOverPercent || percent >= 1f))
      {
        backgroundSprite.color = Color.Lerp(Color.white, Color.black, Interpolators.EaseOutCurve.Evaluate(percent));
      }
      else
      {
        backgroundSprite.color = Color.Lerp(backgroundSprite.color, Color.white, Time.deltaTime);
      }

      for (int f = 0; f < columnSpritesColors.Length; f++)
      {
        columnSprites[f].color = Color.Lerp(columnSprites[f].color, columnSpritesColors[f], Time.deltaTime * 10f);
      }

      previousGameOverPercent = percent;
    }

    public bool IsOnScreen(Vector2 p)
    {
      // Convert position to global viewport
      if (Camera.main != null)
      {
        var v = Camera.main.ScreenToViewportPoint(p);

        return gridCam.rect.Contains(v);
      }

      return false;
    }

    #endregion

    #region Score

    private void ComboDetected(List<Block> comboBlocks, int currentComboIndex, int totalCombosCount)
    {
      int blocksCount = comboBlocks.Count;
      if (blocksCount < 3) return;

      bypassFrozen = false;

      // Light columns
      for (int b = 0; b < blocksCount; b++)
      {
        int x = comboBlocks[b].x + 1;
        columnSprites[x].color = new Color(1f, 1f, 1f, 0.75f);
      }

      // Shake shake shake
      GridShakerScript.Shake(this, 0.15f, 0.05f);

      // Reset Game Over timer
      timeBeforeGameOverCurrent = timeBeforeGameOverMax;

      totalCombos++;

      // Update chains
      bool isChain = false;
      if (PonGameScript.instance == null || PonGameScript.instance.Settings.disableChains == false)
      {
        // -- A chain is combo where at least one block is falling and it is not the latest block moved
        bool blocksAreChainable = true;
        foreach (var b in comboBlocks)
        {
          blocksAreChainable &= b.Chainable;

          if (!blocksAreChainable) break;
        }

        // -- Also a chain is never the first combo
        isChain = ComboMultiplier >= 1 && blocksAreChainable;
        if (isChain)
        {
          chainCount++;
          totalChains++;
          Log.Info("CHAIN! chainCount=" + chainCount);
        }
      }

      if (currentComboIndex >= totalCombosCount - 1)
      {
        lastMove = new Grid.MoveResult();
      }

      if (blocksCount == 4)
      {
        total4Combos++;
      }
      else if (blocksCount == 5)
      {
        total5Combos++;
      }
      else if (blocksCount > 5)
      {
        total6Combos++;
      }

      CheckForNextLevel();

      // Update combo
      IncreaseMultiplier(isChain ? 2 : 1);

      // Compute score
      long points = GetScore(blocksCount, isChain);
      AddPoints(points);

      // Add some power
      AddPower(blocksCount);

      // Combo feedback
      Vector3 loc = BlockToWorld(comboBlocks[Mathf.CeilToInt(comboBlocks.Count / 2f)].position, Vector2.zero);
      var locs = comboBlocks.Select(b =>
          BlockToWorld(b.position, Vector2.zero) + new Vector2(0.50f, 0.5f))
        .ToArray();
      OnCombo.Raise(this,
        new ComboData(blocksCount, isChain ? chainCount : comboMultiplier, isChain, loc, comboBlocks[0].Definition));

      // Center
      loc += new Vector3(0.60f, 0.5f);

      GameUIScript.DisplayComboWidget(this, comboMultiplier, blocksCount, points, comboBlocks[0].Definition,
        loc, locs);
      if (isChain)
      {
        GameUIScript.DisplayChainWidget(this, chainCount, points, comboBlocks[0].Definition.color, loc);
      }
    }

    private int GetScore(int blocksCount, bool isChain)
    {
      int points;
      if (isChain)
      {
        points = BONUS_SCORE_CHAINS;
      }
      else
      {
        int bonus = blocksCount - 3;
        points = (BASE_SCORE_PER_COMBO + (bonus * BONUS_SCORE));
      }

      points *= comboMultiplier;

      return points;
    }

    public void IncreaseMultiplier(int multiplierModifier)
    {
      if (multiplierFrame != grid.MultiplierFrame)
      {
        comboMultiplier = 0;
      }

      multiplierFrame = grid.MultiplierFrame;

      comboMultiplier += multiplierModifier;
      HighestMultiplier = Mathf.Max(HighestMultiplier, comboMultiplier);

      OnMultiplierChange?.Invoke(this, comboMultiplier);
    }

    public void AddPoints(long points)
    {
      score += points;

      player.Score = score;

      OnScoreChanged.Raise(this, score);
    }

    public void AddPower(int blocksCount)
    {
      if (enablePower && FreezePowerGain == false)
      {
        float bonus = 0;
        if (blocksCount <= 3)
        {
          bonus += POWER_ADD_LOW;
        }
        else if (blocksCount == 4)
        {
          bonus += POWER_ADD_MED;
        }
        else
        {
          bonus += POWER_ADD_HIGH + ((blocksCount - 5) * POWER_ADD_HIGH_BONUS);
        }

        bonus *= playerScript.power.ChargeMultiplicator;

        PowerCharge += bonus;
      }
    }

    private void CheckForNextLevel()
    {
      if (level >= MAX_LEVEL) return;

      if (totalCombos % BASE_COMBO_PER_LEVEL == 0)
      {
        // Level up
        AddLevel(1);
      }
    }

    public void AddLevel(int levels)
    {
      if (settings.noLevelUp == false &&
          (settings.limitLevel < 0 || (settings.limitLevel >= 0 && settings.limitLevel > level)))
      {
        level += levels;
        if (settings.limitLevel > 0)
        {
          level = Mathf.Clamp(level, 0, settings.limitLevel);
        }

        grid.level = level;

        // New settings
        SetSpeedForLevel();
        ApplyGridSettings();

        Log.Debug("Speed level=" + level);

        // Update UI
        OnLevelChanged.Raise(this, level);
      }
    }

    #endregion

    #region Public methods

    public Grid.MoveResult Move(Block block, int direction)
    {
      lastMove = new Grid.MoveResult();

      if (block == null)
      {
        Log.Error("Cannot move null block.");
        return lastMove;
      }

      if (isGameOver == false && isPaused == false)
      {
        lastMove = grid.Move(block, direction);
        if (lastMove.success)
        {
          moves++;
          OnMove.Raise(this, BlockToWorld(block.position, Vector2.zero), moves);
        }

        return lastMove;
      }

      return lastMove;
    }

    public bool CanMove(int x, int y)
    {
      if (isGameOver || isPaused) return false;

      if (OnCanMoveBlock != null)
      {
        if (OnCanMoveBlock(this, Get(x, y)) == false) return false;
      }

      return grid.CanMove(x, y);
    }

    public bool CanMove(BlockScript b)
    {
      return (b != null) && CanMove(b.block.x, b.block.y);
    }

    public bool ForceFall(Block block)
    {
      if (isGameOver == false && isPaused == false)
      {
        return grid.ForceFall(block);
      }

      return false;
    }

    public BlockScript Get(int x, int y)
    {
      var b = grid.Get(x, y);

      if (b != null)
      {
        return blocks[b];
      }

      return null;
    }

    /// <summary>
    /// Define the "draw" zone
    /// </summary>
    private void SetViewport(Rect r, float angle)
    {
      gridCam.rect = r;
      gridCam.transform.rotation = Quaternion.Euler(0, 0, angle);

      //float preview = Mathf.Abs(settings.previewLines) - (settings.previewLines != 0 ? 0.5f : 0);

      // Display only a bit of the preview
      float preview = (settings.previewLines > 0 ? 0.25f : 0);

      // Center on screen
      gridCam.transform.position = new Vector3(
        transform.position.x + (settings.width / 2f),
        transform.position.y + (settings.height / 2f) + (-preview / 2f),
        gridCam.transform.position.z
      );
      gridCam.orthographicSize = (settings.height + preview) / 2f;

      var gui = GameUIScript.GetUI();
      transform.position += new Vector3(0f, gui.gridRelativePosition.y * gridCam.orthographicSize * 2f);
      transform.localScale = gui.gridScale;
    }

    public void SetSpeedForLevel()
    {
      if (settings.noScrolling)
      {
        ScrollingSpeed = 0f;
      }
      else
      {
        const float SPEED_BASE = 0.2f;
        const float SPEED_BONUS_PER_LEVEL = 0.1f;
        const float SPEED_BONUS_PER_HIGH_LEVEL = 0.1f;

        float s = SPEED_BASE + (level * SPEED_BONUS_PER_LEVEL);
        if (level >= 10)
        {
          s += (level - 10) * SPEED_BONUS_PER_HIGH_LEVEL;
        }

        ScrollingSpeed = s;
      }
    }

    internal void SpeedScrollingUp()
    {
      if (settings.noScrolling == false)
      {
        bypassFrozen = true;

        isSpeeding = true;
        speedUpCooldown = settings.speedUpDuration;
        currentSpeed = Mathf.Max(2, ScrollingSpeed + 2.15f);

        OnSpeedUp.Raise(this, speedUpCooldown);
      }
    }

    internal void StopSpeedScrollingUp()
    {
      if (isSpeeding)
      {
        isSpeeding = false;
        speedUpCooldown = 0.001f; // Not 0 so the regular cooldown code can handle the stop
      }
    }

    /// <summary>
    /// Convert block position to world position
    /// </summary>
    private Vector2 BlockToWorld(Vector2 blockLocalPosition, Vector3 shift)
    {
      return transform.TransformPoint(blockLocalPosition) + shift;
    }

    public void SetPause(bool p)
    {
      isPaused = p;

      if (settings.hideOnPause)
      {
        // Hide or show blocs
        foreach (var blockScript in blocks)
        {
          if (blockScript.Value != null && blockScript.Key.IsEmpty == false
                                        && blockScript.Key.IsBeingRemoved == false
                                        && blockScript.Key.IsConverting == false)
          {
            if (isPaused)
            {
              blockScript.Value.Hide(0, 3);
            }
            else
            {
              blockScript.Value.Reveal(0, 5);
            }
          }
        }
      }
    }

    public void SetGameOver()
    {
      isPaused = true;
      isGameOver = true;

      if (targetHeightBar != null) targetHeightBar.gameObject.SetActive(false);

      StartCoroutine(GameOverAnimation());
    }

    public void SetVictory()
    {
      isPaused = true;
      isGameOver = true;

      if (targetHeightBar != null) targetHeightBar.gameObject.SetActive(false);

      StartCoroutine(VictoryAnimation());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>1 = grid is filled, 0 = grid is empty, -1 = not enough blocks for a combo</returns>
    public int IsGridEmpty()
    {
      int blocksCount = 0;
      int blocksBeingRemoved = 0; // Let clear animation play to completion

      Dictionary<BlockDefinition, int> blocksLeftPerColor = new Dictionary<BlockDefinition, int>();

      for (int x = 0; x < grid.width; x++)
      {
        for (int y = 0; y < grid.height; y++)
        {
          var b = grid.Get(x, y);
          if (b != null && b.IsEmpty == false)
          {
            if (b.IsBeingRemoved)
            {
              blocksBeingRemoved++;
            }
            else
            {
              blocksCount++;
            }

            if (blocksLeftPerColor.ContainsKey(b.Definition))
            {
              blocksLeftPerColor[b.Definition] += 1;
            }
            else
            {
              blocksLeftPerColor.Add(b.Definition, 1);
            }
          }
        }
      }

      if (blocksCount == 0 && blocksBeingRemoved == 0) return 0;
      if (blocksCount <= 2 && blocksBeingRemoved == 0) return -1;

      // Check if we only have 2 blocks of the same colors only (no more combo possible)
      if (blocksBeingRemoved == 0)
      {
        bool allLost = blocksLeftPerColor.Count > 0;
        foreach (var d in blocksLeftPerColor.Keys)
        {
          allLost &= (blocksLeftPerColor[d] <= 2);
        }

        if (allLost)
        {
          return -2;
        }
      }

      return 1;
    }

    public Block[] GetHighestLine()
    {
      return grid.GetLine((int) grid.HighestBlock.y).Where(b => b != null && b.IsEmpty == false).ToArray();
    }

    #endregion

    #region Power

    public void UsePower()
    {
      // Enough power?
      if (PowerCharge >= 1f && playerScript.power.CanUsePower(this, grid))
      {
        playerScript.power.UsePower(this, grid);

        // Freeze grid n sec to avoid game over
        grid.Freeze(3f);
      }
    }

    #endregion

    #region Garbage blocks

    public struct GarbageStored
    {
      public int width;
      public int color;
      public bool isFromChain;
    }

    private GarbagesWidget garbageWidget;

    public void AddGarbage(int width, bool fromChain, PlayerScript sender, Vector3 comboLocation, Color c)
    {
      if (isGameOver) return;
      if (garbageWidget == null)
      {
        garbageWidget = ui.GetComponentInChildren<GarbagesWidget>();
      }

      AddGarbageInternal(new GarbageStored()
      {
        width = width, isFromChain = fromChain
      });
    }

    private void AddGarbageInternal(GarbageStored garb)
    {
      // Store the garbage and bump cooldown
      garbages.Add(garb);

      float cd = GARBAGE_COOLDOWN / garbages.Count;
      garbageBaseCooldown += cd;
      garbageCooldown += cd;
      garbageCooldown = Mathf.Min(garbageCooldown, GARBAGE_MAX_COOLDOWN);

      // Merge combos when possible
      if (garb.isFromChain == false)
      {
        int sum = garbages.Where(g => g.isFromChain == false).Sum(g => g.width);
        garbages = garbages.Where(g => g.isFromChain).ToList();

        int g5 = sum / 5;
        for (int i = 0; i < g5; i++)
        {
          garbages.Add(new GarbageStored() {width = 5});
        }

        sum -= g5 * 5;
        int g3 = sum / 3;
        for (int i = 0; i < g3; i++)
        {
          garbages.Add(new GarbageStored() {width = 3});
        }

        sum -= g3 * 3;
        for (int i = 0; i < sum; i++)
        {
          garbages.Add(new GarbageStored() {width = 1});
        }
      }

      OnGarbageStored?.Invoke(garb);
    }

    public void UpdateGarbageGeneration()
    {
      if (garbages.Count == 0)
      {
        garbageCooldown = 0;
        return;
      }

      // Garbages are stacked a bit and released in groups
      if (garbageCooldown > 0)
      {
        float factor = isSpeeding ? 10f : 1f;
        garbageCooldown -= (DeltaTime * GarbageCooldownSpeed * factor);
        return;
      }

      garbageBaseCooldown = 0;

      // Release the kraken!
      int countAdded = 0;

      var copy = garbages.ToList();
      for (int i = 0;
        i < copy.Count;
        i++)
      {
        var g = copy[i];
        if (grid.CanAddGarbage(g.width, out var garbageLocation))
        {
          countAdded++;
          garbages.Remove(g);

          OnGarbageAdded?.Invoke(g);

          var block = grid.AddGarbage(g.width, garbageLocation);

          if (block != null && block.Definition != null && block.Definition is GarbageBlockDefinition gd)
          {
            var right = block;
            while (right != null)
            {
              right = right.Right;
            }
          }
        }
        else
        {
          break;
        }
      }

      if (garbages.Count > 0)
      {
        // Keep in stack those that can't be added yet
        garbageCooldown = GARBAGE_RETRY_COOLDOWN;
      }
    }

    #endregion

    #region Events

    private static Vector3 GridToUI(Camera gridCam, Vector3 position)
    {
      var viewport = gridCam.WorldToScreenPoint(position);
      var uiPosition = Camera.main.ScreenToWorldPoint(viewport);

      uiPosition.z = 0;
      // Debug.Log(position + "->" + viewport + "->" + uiPosition);
      return uiPosition;
    }

    private void OnLineUp()
    {
      if (targetHeightBar != null) targetHeight++;
    }

    private void FallEnd(Block block, bool isBottom)
    {
      if (isBottom && block.IsEmpty == false)
      {
        if (block.Definition.isGarbage)
        {
          GridShakerScript.Shake(this, 0.15f, 0.1f);
        }
      }
    }

    #endregion

    #region Effects

    private IEnumerator GameOverAnimation()
    {
      for (int x = 0;
        x < grid.width;
        x++)
      {
        StartCoroutine(GameOverColumnAnimation(x));
      }

      foreach (var columnSprite in columnSprites)
      {
        columnSprite.transform.DOScale(new Vector3(1, 0), Random.Range(1.5f, 2.42f))
          .SetEase(Ease.InCubic)
          .SetDelay(Random.Range(0.1f, 0.25f));
      }

      yield return null;
    }

    private IEnumerator GameOverColumnAnimation(int x)
    {
      yield return new WaitForSeconds(Random.Range(0.01f, 0.05f));
      GridShakerScript.Shake(this, 0.25f, 0.025f * (x + 1));
      for (int y = grid.height - 1;
        y >= -grid.previewLines;
        y--)
      {
        var block = Get(x, y);

        if (block != null && block.block.IsEmpty == false)
        {
          StartCoroutine(GameOverBlockAnimation(block));
          yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }
      }
    }

    private IEnumerator GameOverBlockAnimation(BlockScript b)
    {
      var block = b.gameObject;
      Destroy(b);
      var startScale = block.transform.localScale;
      var position = block.transform.position;
      var def = b.block.Definition;
      float time = 0f;
      float duration = Random.Range(0.2f, 0.35f);

      bool explo = false;
      while (time < duration)
      {
        time += DeltaTime;

        float p = Interpolators.BumpCurve.Evaluate(1f - (time / duration));

        // Shrink
        block.transform.localScale = MathfEx.LerpWithoutClamp(Vector3.zero, startScale, p);

        var shiftToCenter = (startScale - block.transform.localScale);
        shiftToCenter.x = shiftToCenter.x / 2f;
        shiftToCenter.y = 0;

        block.transform.position = position + shiftToCenter;

        if (p < 0.725f && !explo)
        {
          explo = true;
        }

        yield return new WaitForEndOfFrame();
      }

      yield return null;
    }

    private IEnumerator VictoryAnimation()
    {
      yield return new WaitForSeconds(1f);
      for (int y = grid.height - 1;
        y >= -grid.previewLines;
        y--)
      {
        for (int x = 0; x < grid.width; x++)
        {
          var block = Get(x, y);

          if (block != null && block.block.IsEmpty == false && block.block.IsBeingRemoved == false)
          {
            block.block.EmptyWithAnimation(1, 1);
            yield return new WaitForSeconds(0.015f);
          }
        }

        yield return null;
      }
    }

    #endregion

    #region Debug

    void OnDrawGizmos()
    {
      if (gridCam != null)
      {
        var size = gridCam.ViewportToWorldPoint(new Vector3(1, 1)) -
                   gridCam.ViewportToWorldPoint(new Vector3(0, 0));
        Gizmos.DrawWireCube(gridCam.transform.position, size);

        var c = Gizmos.color;

        foreach (var block in blocks.Keys)
        {
          Gizmos.color = Color.gray;
          Gizmos.DrawWireCube(transform.position + new Vector3(block.x + 0.5f, block.y + 0.5f), Vector3.one);
          Gizmos.color = Color.yellow;
          Gizmos.DrawWireCube(transform.position + new Vector3(block.position.x + 0.5f, block.position.y + 0.5f),
            Vector3.one);
        }

        Gizmos.color = c;
      }
    }

    #endregion

    #region Properties

    public float DeltaTime => Time.deltaTime * ScaleTime;

    public float ScaleTime { get; set; } = 1f;

    public bool IsStarted { get; set; }

    public bool IsPaused => isPaused;

    public bool IsGameOver => isGameOver;

    public long Score => score;

    public int ComboMultiplier => comboMultiplier;

    public int HighestMultiplier { get; private set; }

    public int TotalCombos => totalCombos;

    public int Total4Combos => total4Combos;

    public int Total5Combos => total5Combos;

    public int Total6Combos => total6Combos;

    public int TotalChains => totalChains;

    public int Chains => chainCount;

    public int HighestY => grid != null ? (int) grid.HighestBlock.y : 0;

    public int Moves => moves;

    public int SpeedLevel => level;

    public float FrozenCooldownPercent => grid.FrozenCooldown / Grid.COMBO_FREEZE_TIME;

    private bool freezeCombos;

    public bool FreezeCombos
    {
      get => freezeCombos;
      set
      {
        freezeCombos = value;
        foreach (var block in blocks)
        {
          block.Key.Chainable = false;
        }
      }
    }

    public bool FreezePowerGain { get; set; }

    public bool BypassFrozen
    {
      get => bypassFrozen;
      set => bypassFrozen = value;
    }

    private float powerCharge;

    public float PowerCharge
    {
      get => powerCharge;

      set
      {
        float oldValue = powerCharge;
        powerCharge = value;
        OnPowerChargeChanged.Raise(this, player.power, powerCharge, oldValue > powerCharge ? -1 : 1);
      }
    }

    public float GarbageCooldownPercent => garbageBaseCooldown == 0
      ? 0
      : (garbageCooldown / (Mathf.Min(garbageBaseCooldown, GARBAGE_MAX_COOLDOWN)));

    public float GarbageCooldownSpeed { get; set; } = 1f;

    public List<GarbageStored> Garbages => garbages;

    /// <summary>
    /// USE WITH EXTREME CAUTION
    /// </summary>
#if UNITY_EDITOR
    public Grid TheGrid
#else
      internal Grid TheGrid
#endif

    {
      get { return grid; }
    }

    public float ScrollingSpeed { get; private set; }

    public float SpeedBonus
    {
      get => currentSpeedBonus;
      set => currentSpeedBonus = Mathf.Max(0, value);
    }

    #endregion
  }
}