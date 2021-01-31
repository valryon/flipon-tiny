// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Pon
{
  /// <summary>
  /// The game core.
  /// </summary>
  public sealed class Grid
  {
    // ReSharper disable once InconsistentNaming
    private static Vector2[] NEIGHBORS_AXIS_4 =
    {
      new Vector2(-1, 0),
      new Vector2(1, 0),
      new Vector2(0, -1),
      new Vector2(0, 1)
    };

    public struct MoveResult
    {
      public readonly bool success;
      public readonly Vector2 position1;
      public readonly Vector2 position2;
      public readonly Block block1;
      public readonly Block block2;

      public MoveResult(Vector2 p1, Block b1, Vector2 p2, Block b2)
      {
        success = true;
        position1 = p1;
        block1 = b1;
        position2 = p2;
        block2 = b2;
      }
    }

    #region Constants

    public const float COMBO_FREEZE_TIME = 2.42f;
    public const float ANIM_BLOCK_FALL_MOMENTUM = 0.15f;
    public const float ANIM_BLOCK_FALL_MOMENTUM_MOVE = 0.125f;

    #endregion

    #region Members

    public readonly int width;
    public readonly int height;
    public int level = 1;
    public readonly int previewLines;
    public bool generateNewBlocks = true;
    public bool seedGarbageDestruction = false;

    public event System.Action<List<Block>, int, int> OnComboDetected;
    public event System.Action OnNewLineGenerated;
    public event System.Action<Block, bool> OnFallEnd;

    // ReSharper disable once InconsistentNaming
    public event System.Func<BlockDefinition> GetGarbageNewDefinition;
    public event System.Action OnLineUp;
    public event System.Action<Block, GarbageBlockDefinition, BlockDefinition> OnGarbageBreak;

    // Blocks data
    private IBlockFactory blockFactory;

    // Blocks position
    private Dictionary<Vector2, Block> map;
    private List<Block> blocks;

    // Grid data
    private GameRandom gridRandom;
    private float scrollSinceLastDing;
    private float scrollValue, previousScrollValue;
    private int topLine;
    private float frozenCooldown;
    private bool topReached;
    private bool topReachedByBlock;
    private bool topTempScrollLock;

    // Combos data
    private List<List<Block>> combos;
    private List<Block> currentCombo = new List<Block>();

    private Vector2 realWorldPosition;
    private bool isStarted = false;

    #endregion

    #region Constructor

    /// <summary>
    /// Create a new grid, empty or pref-filled with blocks.
    /// </summary>
    public Grid(Vector2 realWorldPosition, int width, int height, int previewLines, IBlockFactory blockFactory,
      Dictionary<Vector2, BlockDefinition> blocks = null)
    {
      this.realWorldPosition = realWorldPosition;
      this.width = width;
      this.height = height;
      this.previewLines = previewLines;

      this.blockFactory = blockFactory;

      var seed = PonGameScript.instance
                 && PonGameScript.instance.Settings != null
                 && PonGameScript.instance.Settings.seed > 0
        ? PonGameScript.instance.Settings.seed
        : Time.frameCount;
      gridRandom = new GameRandom(seed);

      map = new Dictionary<Vector2, Block>();
      this.blocks = new List<Block>(width * height);

      // Fill grid
      for (int y = -previewLines; y < height; y++)
      {
        for (int x = 0; x < width; x++)
        {
          var v = new Vector2(x, y);
          if (map.ContainsKey(v) == false)
          {
            var block = new Block();
            RegisterBlock(v, block);

            block.Empty();
          }
        }

        // Top line is the last one as we go from bottom to top
        topLine = y;
      }

      if (blocks != null)
      {
        foreach (var preload in blocks)
        {
          var b = Get(preload.Key);
          if (b != null)
          {
            b.SetDefinition(preload.Value, preload.Key.y >= 0);

            if (b.y > HighestBlock.y)
            {
              HighestBlock = new Vector2(b.x, b.y);
            }

            // Link garbages!
            if (b.IsEmpty == false && b.Definition.isGarbage)
            {
              // Something at block's left?
              var leftPostion = preload.Key + new Vector2(-1, 0);
              var bleft = Get(leftPostion);
              if (bleft != null && bleft.IsEmpty == false && bleft.Definition.isGarbage)
              {
                bleft.Right = b; // Link garbages from left to right
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Start the grid. All blocks will fall.
    /// </summary>
    public void Start(int startLevel, int fillLinesAtStart = -1)
    {
      level = startLevel;
      fillLinesAtStart = Mathf.Min(fillLinesAtStart, height);

      if (fillLinesAtStart > 0)
      {
        for (int y = -previewLines; y < height; y++)
        {
          // Fill grid if procedural
          foreach (var block in map.Values)
          {
            if (block.y <= (fillLinesAtStart - 1))
            {
              SetRandomBlock(block, block.y >= 0);
              if (block.y > HighestBlock.y)
              {
                HighestBlock = new Vector2(block.x, block.y);
              }
            }
          }
        }
      }

      isStarted = true;
    }

    #endregion

    #region Update

    public void Update(float scroll, float elapsedTime, bool checkCombos = true, bool noScroll = false)
    {
      if (checkCombos)
      {
        // Combo
        UpdateCombos();
      }

      // Freeze
      if (frozenCooldown > 0)
      {
        frozenCooldown -= elapsedTime;

        if (frozenCooldown < 0)
        {
          MultiplierFrame++;
        }
      }

      // Recycle empty blocks and spawn new ones
      UpdateTopLine(noScroll);

      // Falling down
      UpdateBlocks(elapsedTime);

      // Update blocks position
      Scroll(scroll);

      // Make sure the map is correct
      Remap(true);
    }

    /// <summary>
    /// Scroll the whole grid
    /// </summary>
    private void Scroll(float s)
    {
      if (isStarted == false)
      {
        Log.Error("Grid must be started!");
        return;
      }

      // Rocket grid protection
      if (topTempScrollLock)
      {
        s = 0;
      }

      scrollValue += s;
      scrollSinceLastDing += s;

      int scrollGap = ((int) scrollValue - (int) previousScrollValue);
      if (scrollGap > 0)
      {
        if (scrollGap > 1)
        {
          Log.Error("Scrolling more than 1 per frame?!");
          scrollGap = 1;
        }

        scrollSinceLastDing -= scrollGap;
      }

      bool ding = (scrollGap > 0);

      if (!ding) HighestBlock = Vector2.zero;
      else HighestBlock += new Vector2(0, 1);

      foreach (var block in map.Values)
      {
        block.position += new Vector2(0, s);
        if (!ding && block.IsEmpty == false && block.y > HighestBlock.y)
        {
          HighestBlock = new Vector2(block.x, block.y);
        }

        if (ding)
        {
          block.LineUp();

          // Activate block?
          if (block.y >= 0)
          {
            block.Activate(block.IsActive == false);
          }
        }
      }

      if (ding)
      {
        OnLineUp.Raise(); // raised AFTER blocks are moved§§§
      }

      previousScrollValue = scrollValue;
    }

    private void UpdateBlocks(float elapsedTime)
    {
      NonEmptyBlocksCount = 0;
      List<Block> fallEndedBlocks = new List<Block>();

      for (int pY = -previewLines; pY < height; pY++)
      {
        for (int pX = 0; pX < width; pX++)
        {
          var block = Get(pX, pY);

          if (block == null) continue;

          block.FrameCount++;

          if (block.IsBeingRemoved) continue;

          if (block.IsEmpty == false && block.IsActive && block.CanMove) NonEmptyBlocksCount++;

          float x = block.x;
          float y = block.y + scrollSinceLastDing;

          bool stopAndAlign = !(block.Definition != null
                                && block.Definition.isGarbage
                                && block.Left != null);
          var movementAfterAlign = MovementType.None;

          // Horizontal (blocks swapped by player)
          // ==================================================================
          if (block.Movement == MovementType.Horizontal)
          {
            block.FallMomentum = ANIM_BLOCK_FALL_MOMENTUM_MOVE;
            block.Chainable = false;

            stopAndAlign = false;
            x = block.x;

            float remainingWidth;

            if (block.DirectionX > 0)
            {
              remainingWidth = block.x - block.position.x;
            }
            else
            {
              remainingWidth = block.position.x - block.x;
            }

            if (remainingWidth > 0f)
            {
              float horizontalMovement = block.movingSpeed.x * elapsedTime;

              // Try not to go in the deeper block
              horizontalMovement = Mathf.Min(horizontalMovement, Mathf.Abs(remainingWidth));

              block.position = new Vector2(block.position.x + (block.DirectionX * horizontalMovement), y);

              if (horizontalMovement >= remainingWidth)
              {
                stopAndAlign = true;
                movementAfterAlign = MovementType.Wait;
              }
            }
            else
            {
              stopAndAlign = true;
              movementAfterAlign = MovementType.Wait;
            }
          }
          else
          {
            block.Movement = MovementType.None;

            if (block.IsEmpty == false && block.IsActive)
            {
              if (block.Left == null)
              {
                // Fall?
                // ==================================================================              
                var fall = GetFallDepth(block);

                if (fall > 0.01f)
                {
                  if (block.FallMomentum > 0)
                  {
                    block.FallMomentum -= elapsedTime;
                    block.Movement = MovementType.Wait;
                  }
                  else
                  {
                    stopAndAlign = false;
                    block.Movement = MovementType.Fall;

                    y = block.y + scrollSinceLastDing;

                    float expectedY = (block.position.y - fall);
                    float remainingHeight = block.position.y - expectedY;

                    float fallMovement = block.movingSpeed.y * elapsedTime;

                    // Acceleration = small % of total fall duration
                    fallMovement += (block.movingSpeed.y * 0.09f * block.FallDuration);

                    // Try not to go in the deeper block
                    fallMovement = Mathf.Min(fallMovement, remainingHeight);

                    // Swap with empty?
                    block.position += new Vector2(0, -fallMovement);

                    int currentY = Mathf.FloorToInt(block.position.y - scrollSinceLastDing);

                    // /❗\ floats are not precise enough to rely on them.
                    // A slight delta of 0.0001 can be enough to trigger a line change without the proper checks 
                    var deltaCheck = block.position.y - (block.y + scrollSinceLastDing);

                    if (block.y != currentY && Mathf.Abs(deltaCheck) > 0.01f)
                    {
                      var below = Get(block.x, currentY);
                      if (below != null && below.IsEmpty)
                      {
                        Swap(block.x, block.y, block.x, currentY, false, true);

                        below.SetPosition(below.x, below.y);
                      }
                    }

                    // If the movement perfectly fits (mostly in tests)
                    if (remainingHeight <= fallMovement)
                    {
                      stopAndAlign = true;
                    }

                    // Acceleration for further frames
                    block.FallDuration += elapsedTime;
                  }
                }
                else
                {
                  stopAndAlign = true;
                }
              }
            }
          }

          if (stopAndAlign)
          {
            var topBlock = Get(block.x, block.y + 1);
            var botBlock = Get(block.x, block.y - 1);

            if (block.Movement == MovementType.Fall && block.FallDuration > Time.deltaTime)
            {
              fallEndedBlocks
                .Add(block); // We need to keep track of previously fallen blocks, as we reset their state here.

              bool isBottom = (topBlock == null || topBlock.IsEmpty ||
                               (topBlock.IsEmpty == false && topBlock.Movement == MovementType.Fall))
                              && (botBlock == null || fallEndedBlocks.Contains(botBlock) == false);

              OnFallEnd.Raise(block, isBottom);

              // Propagate chains
              for (int iy = block.y - 1; iy >= 0; iy--)
              {
                var b = Get(block.x, iy);
                if (b != null && b.IsEmpty == false && b.IsBeingRemoved == false && b.Definition.isGarbage == false)
                {
                  b.Chainable = block.Chainable;
                }
                else
                {
                  break;
                }
              }
            }

            // Interruptable falls and chains
            if (topBlock != null
                && topBlock.Movement == MovementType.Fall
                && block.Chainable == false)
            {
              if (block.Movement == MovementType.Horizontal)
              {
                block.Chainable = true;
              }
            }

            // Align the block
            block.position = new Vector3(x, y, 0);
            block.Movement = movementAfterAlign;
            block.FallDuration = 0f;

            if (block.Chainable == false &&
                block.Movement == MovementType.None && block.FrameCount > 6)
            {
              block.Chainable = true;
            }

            if (botBlock != null && (!botBlock.IsEmpty && !botBlock.IsBeingRemoved))
            {
              block.FallMomentum = 0f;
            }
          }

          // Chained blocks
          // ==================================================================    
          if (block.Left != null)
          {
            // Align & copy properties
            var leftest = block.Leftest;

            bool swapRequired = (block.y != leftest.y);
            if (swapRequired)
            {
              if (!Swap(block.x, block.y, block.x, leftest.y, false, true))
              {
                Log.Warning("Garbage bug! " + block);
                // This happens when the block on the left is falling further than it should/can
                // Check fall depth + "below" above
// #if UNITY_EDITOR
//                 Debug.Break();
// #endif
              }
            }

            block.position = new Vector2(block.position.x, leftest.position.y);
            block.Movement = leftest.Movement;
          }
        } // y
      } // x
    }

    #endregion

    #region Block actions

    /// <summary>
    /// Add block to the map.
    /// </summary>
    private void RegisterBlock(Vector2 v, Block block)
    {
      RegisterBlock((int) v.x, (int) v.y, block);
    }

    /// <summary>
    /// Add block to the map.
    /// </summary>
    private void RegisterBlock(int x, int y, Block block)
    {
      block.SetPosition(x, y);
      map.Add(new Vector2(x, y), block);
      blocks.Add(block);
    }

    private BlockDefinition GetRandomBlock(Block block)
    {
      // Get neighborhs
      List<BlockDefinition> excludedDef = new List<BlockDefinition>();
      var t1 = Get(block.x, block.y - 2);

      var t2 = Get(block.x, block.y - 1);
      if (t1 != null && t2 != null && t1.Definition == t2.Definition)
      {
        excludedDef.Add(t1.Definition);
      }

      var b1 = Get(block.x, block.y + 2);

      var b2 = Get(block.x, block.y + 1);
      if (b1 != null && b2 != null && b1.Definition == b2.Definition)
      {
        excludedDef.Add(b1.Definition);
      }

      var l1 = Get(block.x - 2, block.y);

      var l2 = Get(block.x - 1, block.y);
      if (l1 != null && l2 != null && l1.Definition == l2.Definition)
      {
        excludedDef.Add(l1.Definition);
      }

      var r1 = Get(block.x + 2, block.y);

      var r2 = Get(block.x + 1, block.y);
      if (r1 != null && r2 != null && r1.Definition == r2.Definition)
      {
        excludedDef.Add(r1.Definition);
      }

      // Set sprite/color/whatever
      return blockFactory.GetRandomNormal(level, width, gridRandom, excludedDef);
    }

    private void SetRandomBlock(Block block, bool active)
    {
      block.SetDefinition(GetRandomBlock(block), active);
    }

    /// <summary>
    /// Get the furthest block below the given one and the fall depth
    /// </summary>
    /// <param name="fallingBlock"></param>
    private float GetFallDepth(Block fallingBlock)
    {
      float fall = height + 1;

      var b = fallingBlock.Leftest;
      while (b != null)
      {
        int blockFallHeight = 0;
        bool weNeedToGoDeeper = true;
        float depth = Mathf.Max(0, b.position.y - b.y) - scrollSinceLastDing;

        while (weNeedToGoDeeper)
        {
          blockFallHeight++;
          int x = b.x;
          int y = b.y - blockFallHeight;

          var belower = Get(x, y);

          if (belower != null && belower.Left != null)
          {
            belower = belower.Leftest;
          }

          // Stop conditions
          var stop = ((belower == null
                       || belower.IsBeingRemoved
                       || belower.IsEmpty == false && belower.IsBeingRemoved == false &&
                       belower.Movement != MovementType.Fall)
                      || (belower.IsEmpty && belower.Movement == MovementType.Horizontal)
            );
          if (stop)
          {
            blockFallHeight--;
            weNeedToGoDeeper = false;
          }
          else
          {
            depth = b.position.y - (belower.position.y);
          }
        }

        fall = Mathf.Min(fall, depth);
        b = b.Right;
      }

      // Fix Unity float precision shit
      // Basically, with scrolling and position, you can have a frame of delay between two blocks
      // And that frame can change everything...
      // if (fall < 0.15f)
      // {
      //   fall = 0f;
      // }
      return fall;
    }

    private void EmptyBlocks(List<Block> blocksToKill)
    {
      // Reset freeze timer
      Freeze(COMBO_FREEZE_TIME);
      for (int i = 0;
        i < blocksToKill.Count;
        i++)
      {
        var b = blocksToKill[i];

        b.EmptyWithAnimation(i, blocksToKill.Count);

        // Add momentum on upper blocks 
        var upperBlock = Get(b.x, b.y + 1);
        if (upperBlock != null && upperBlock.IsEmpty == false)
        {
          upperBlock.FallMomentum = ANIM_BLOCK_FALL_MOMENTUM;
        }
      }
    }

    #endregion

    #region Combos

    /// <summary>
    /// Look for 3 or more color in a row/line
    /// </summary>
    private void UpdateCombos()
    {
      // Check lines
      if (combos == null)
      {
        combos = new List<List<Block>>();
      }

      ComboCheckHorizontal();
      ComboCheckVertical();
      ComboCombine();

      // Animation for combos
      int totalCombos = combos.Count;
      if (totalCombos > 0)
      {
        for (int i = 0; i < totalCombos; i++)
        {
          var combo = combos[i];

          OnComboDetected.Raise(combo, i, totalCombos);

          for (int j = 0; j < combo.Count; j++)
          {
            var block = combo[j];

            // Check for garbages block touching the combo!
            for (int k = 0; k < NEIGHBORS_AXIS_4.Length; k++)
            {
              var neighbor = Get(block.x + (int) NEIGHBORS_AXIS_4[k].x, block.y + (int) NEIGHBORS_AXIS_4[k].y);
              if (neighbor != null && neighbor.IsEmpty == false && neighbor.Definition.isGarbage)
              {
                // Garbage block is broken, make a new random block instead!
                BreakGarbage(neighbor);
              }
            }

            // Propagate chains
            for (int iy = block.y + 1; iy < height; iy++)
            {
              var topBlock = Get(block.x, iy);
              if (topBlock != null && topBlock.IsEmpty == false && topBlock.Definition.isGarbage == false)
              {
                topBlock.Chainable = true;
              }
              else
              {
                break;
              }
            }
          }

          // Empty blocks
          EmptyBlocks(combo);
        }

        combos.Clear();
      }
    }

    /// <summary>
    /// Check and fill combos list checking horizontally
    /// </summary>
    private void ComboCheckHorizontal()
    {
      ComboClean();

      // Check if we have horizontal combos
      BlockDefinition d = null;
      for (int y = 0;
        y < height;
        y++)
      {
        for (int x = 0; x < width; x++)
        {
          d = ComboCheckBlock(x, y, d);
        } // x

        // Between lines/columns, forget everthing
        ComboClean();
      } // y
    }

    /// <summary>
    /// Check and fill combos list checking vertically
    /// </summary>
    private void ComboCheckVertical()
    {
      ComboClean();

      // Check if we have vertical combos
      BlockDefinition d = null;
      for (int x = 0;
        x < width;
        x++)
      {
        for (int y = 0; y < height; y++)
        {
          d = ComboCheckBlock(x, y, d);
        } // y

        // Between lines/columns, forget everthing
        ComboClean();
      } // x
    }

    /// <summary>
    /// Check if the given block match the current combo data
    /// </summary>
    private BlockDefinition ComboCheckBlock(int x, int y, BlockDefinition currentDef)
    {
      bool restartCombo = true;

      Block block;
      if (map.TryGetValue(new Vector2(x, y), out block))
      {
        var bottom = Get(x, y - 1);
        if (block.CanCombo && (bottom == null || bottom.IsEmpty == false))
        {
          // Block matches?
          if (currentDef != null && currentDef == block.Definition)
          {
            restartCombo = false;
            currentCombo.Add(block);
          }
        }
      }

      if (restartCombo)
      {
        ComboClean();

        // Current block is the new combo reference, if comboable
        if (block != null && block.CanCombo)
        {
          currentDef = block.Definition;
          currentCombo.Add(block);
        }
      }

      return currentDef;
    }

    private void ComboCombine()
    {
      List<List<Block>> newCombos = new List<List<Block>>();

      // Merge combos with the same blocks
      for (int i = combos.Count - 1; i >= 0; i--)
      {
        if (combos.Count == 0 || i >= combos.Count) continue;

        var combo = combos[i];

        // Pick every block, check if we find it in another combo
        bool merged = false;
        foreach (var b in combo)
        {
          if (merged) break;

          // Check in all the other combos if we find the block again
          for (int j = combos.Count - 1; j >= 0; j--)
          {
            if (j > combos.Count) continue;
            if (merged) break;

            var comboToCheck = combos[j];

            if (i != j)
            {
              foreach (var bToCheck in comboToCheck)
              {
                if (b == bToCheck)
                {
                  Log.Info("Found a combo to merge!");

                  // Remove both combos
                  combos.Remove(combo);
                  combos.Remove(comboToCheck);

                  // Create a new one
                  var newCombo = new List<Block>();
                  newCombo.AddRange(combo);
                  newCombo.AddRange(comboToCheck);

                  // Sorte
                  newCombo = newCombo.OrderBy(bl => bl.x).ThenBy(bl => bl.y).ToList();

                  newCombos.Add(newCombo);

                  merged = true;

                  break;
                }
              }
            }
          }
        }
      }

      combos.AddRange(newCombos);
    }

    private void ComboClean()
    {
      if (currentCombo != null)
      {
        if (currentCombo.Count > 2 && combos.Contains(currentCombo) == false)
        {
          //Log.Info("COMBO " + currentCombo.Count);
          combos.Add(currentCombo);
        }
      }

      currentCombo = new List<Block>(); // NEW, not Clear()! We don't want to keep the ref!
    }

    public void BreakGarbage(Block block)
    {
      if (block != null && block.IsEmpty == false && block.Definition.isGarbage && block.IsConverting == false)
      {
        var gd = block.Definition as GarbageBlockDefinition;

        // Empty all blocks
        List<Block> blocksToEmpty = new List<Block>();
        var b = block.Leftest;
        while (b != null)
        {
          blocksToEmpty.Add(b);
          b = b.Right;
        }

        int c = blocksToEmpty.Count;

        var excludedDefs = new List<BlockDefinition>();
        excludedDefs.Add(block.Definition);

        bool isChainGarbage = c == width;
        int surpriseGarbageIndex = Random.Range(0, c);

        float freezeDuration = 0f;

        for (int n = 0; n < c; n++)
        {
          var blockToConvert = blocksToEmpty[n];
          BlockDefinition def = null;
          if (GetGarbageNewDefinition != null)
          {
            def = GetGarbageNewDefinition();
          }

          if (def == null)
          {
            // Spawn a 1x1 garbage bitch in the middle of a chain garbage break
            if (isChainGarbage && n == surpriseGarbageIndex)
            {
              def = blockFactory.GetRandomGarbage();
            }
            else
            {
              def = blockFactory.GetRandomNormal(level, width, seedGarbageDestruction ? gridRandom : null,
                excludedDefs);
            }
          }

          var duration = blockToConvert.ConvertTo(n, c, def, () => { OnGarbageBreak.Raise(blockToConvert, gd, def); });
          duration += 0.25f;
          freezeDuration = Mathf.Max(freezeDuration, duration);

          excludedDefs[0] = def;
        }

        Freeze(freezeDuration);
      }
    }

    #endregion

    #region Grid actions

    /// <summary>
    /// Check what is happening top
    /// </summary>
    private void UpdateTopLine(bool noScroll)
    {
      bool wasTopReachedByBlock = topReachedByBlock;
      topReached = false;
      topReachedByBlock = false;
      topTempScrollLock = false;

      // Find the first line index
      List<Block> firstLine = GetTopLine();
      foreach (var block in firstLine)
      {
        if (block.y >= height - 1)
        {
          topReached = true;

          if (block.IsEmpty == false)
          {
            if (block.Movement == MovementType.Fall)
            {
              topTempScrollLock = true;
            }
            else
            {
              topReachedByBlock = true;
            }
          }
        }
      }

      if (firstLine.Any(b => !b.IsEmpty))
      {
        if (firstLine.Any(b => b.y > height - 1))
        {
          var debug = firstLine.Select(b => b.ToString()).ToArray();
          Log.Error("Rocket grid! Can't recycle top line if it's not empty!\n"
                    + string.Join("\n", debug));
        }

        return;
      }

      // Top is reached
      if (topReached)
      {
        if (topReachedByBlock == false)
        {
          if (noScroll == false)
          {
            // First line is now bottom if empty
            foreach (var block in firstLine)
            {
              var oldPosition = new Vector2(block.x, block.y);
              Teleport(block, block.x, -(previewLines + 1), true);

              block.Empty();

              // Update map
              map.Remove(oldPosition);

              var newPosition = new Vector2(block.x, block.y);
              if (map.ContainsKey(newPosition) == false)
              {
                map.Add(newPosition, block);
              }
              else
              {
                Log.Error("FUCK" + newPosition + block);
              }
            }

            if (generateNewBlocks)
            {
              // Spawn new bottom blocks
              foreach (var block in firstLine)
              {
                SetRandomBlock(block, false);
              }
            }

            // Reset
            topReached = false;

            OnNewLineGenerated.Raise();
          }
        }
      }

      if (wasTopReachedByBlock && topReachedByBlock == false && IsFrozen == false)
      {
        bool allClear = true;
        foreach (var b in GetTopLine())
        {
          allClear &= (b.Movement == MovementType.None);
        }

        if (allClear == false)
        {
          topReachedByBlock = true;
        }
      }
    }

    /// <summary>
    /// Freeze for specified duration.
    /// </summary>
    public void Freeze(float duration)
    {
      frozenCooldown = Mathf.Max(frozenCooldown, duration);
    }

    public void Warm()
    {
      frozenCooldown = 0f;
    }

    private void Teleport(Block block, int x, int y, bool align)
    {
      if (block != null)
      {
        block.x = x;
        block.y = y;

        if (align)
        {
          block.position = new Vector2(x, y);
        }
      }
    }

    /// <summary>
    /// Swap two blocks
    /// </summary>
    private bool Swap(int x1, int y1, int x2, int y2, bool align, bool updateMap)
    {
      var block1 = Get(x1, y1);

      var block2 = Get(x2, y2);
      if (block1 != null && block2 != null)
      {
        //      Log.Info("SWAP " + block1 + " <--> " + block2);

        // Fix garbages causing block below them to move on top of them
        if (block1.IsEmpty == false && block2.IsEmpty == false &&
            (block1.Definition.isGarbage || block2.Definition.isGarbage))
        {
          return false;
        }

        Teleport(block1, x2, y2, align);
        Teleport(block2, x1, y1, align);

        block1.PreviousX = x1;
        block1.PreviousY = y1;
        block2.PreviousX = x2;
        block2.PreviousY = y2;

        if (updateMap)
        {
          var p1 = new Vector2(x1, y1);
          var p2 = new Vector2(x2, y2);

          map.Remove(p1);
          map.Add(p1, block2);

          map.Remove(p2);
          map.Add(p2, block1);
        }

        return true;
      }
      else
      {
        Log.Error("CANNOT SWAP " + block1 + " (" + x1 + "," + y1 + ") <-> " + block2 + " (" + x2 + "," + y2 + ")");
        return false;
      }
    }

    private bool IsEmpty(int x, int y)
    {
      var worldExpectedPosition = realWorldPosition + new Vector2(x, y + scrollSinceLastDing);

      // Center in a tile
      worldExpectedPosition += new Vector2(0.5f, 0.5f);

      var collision = Physics2D.OverlapCircle(worldExpectedPosition, 0.4f);
      return collision != null;
    }

    /// <summary>
    /// Make sure the map is up to date.
    /// </summary>
    private void Remap(bool alignVisually)
    {
      Dictionary<Vector2, Block> newMap = new Dictionary<Vector2, Block>();
      foreach (var block in map.Values)
      {
        // Remap correctly!
        var position = new Vector2(block.x, block.y);
        try
        {
          newMap.Add(position, block);
        }
        catch (System.ArgumentException)
        {
          Log.Error("KEY CONFLICT: at " + position + " " + newMap[position] + " - " + block);
        }

        // Fix empty blocks
        if (alignVisually && block.IsEmpty && block.IsConverting == false)
        {
          block.position = new Vector3(block.x, block.y + scrollSinceLastDing);
        }
      }

      map.Clear();
      map = newMap;
    }

    /// <summary>
    /// Get a block at given coordinates.
    /// </summary>
    public Block Get(Vector2 xy)
    {
      return Get((int) xy.x, (int) xy.y);
    }

    /// <summary>
    /// Get a block at given coordinates.
    /// </summary>
    public Block Get(int x, int y)
    {
      Block block;
      map.TryGetValue(new Vector2(x, y), out block);
      return block;
    }

    /// <summary>
    /// Get all blocks of the line.
    /// </summary>
    public List<Block> GetLine(int y)
    {
      List<Block> line = new List<Block>();
      foreach (var block in map.Values)
      {
        if (block.y == y)
          line.Add(block);
      }

      return line;
    }

    /// <summary>
    /// Get all blocks of the column.
    /// </summary>
    public List<Block> GetColumn(int x)
    {
      List<Block> col = new List<Block>();
      foreach (var block in map.Values)
      {
        if (block.x == x)
          col.Add(block);
      }

      return col;
    }

    public List<Block> GetTopLine()
    {
      // Check all first blocks
      topLine = -10;
      foreach (var block in map.Values)
      {
        topLine = Mathf.Max(block.y, topLine);
      }

      List<Block> firstLine = GetLine(topLine);
      return firstLine;
    }

    public int GetNonEmptyTopLineY()
    {
      for (int pY = height;
        pY >= 0;
        pY--)
      {
        for (int pX = 0; pX < width; pX++)
        {
          var block = Get(pX, pY);
          if (block != null && block.IsEmpty == false)
          {
            return pY;
          }
        }
      }

      return height;
    }

    public bool CanMove(int x, int y)
    {
      var b = Get(x, y);
      if (b == null) return false;
      return b.CanMove;
    }

    /// <summary>
    /// Move a block horizontally.
    /// </summary>
    public MoveResult Move(Vector2 tile, int direction)
    {
      return MoveInternal(Get(tile), direction);
    }

    /// <summary>
    /// Move a block horizontally.
    /// </summary>
    public MoveResult Move(Block block1, int direction)
    {
      return MoveInternal(block1, direction);
    }

    /// <summary>
    /// Move a block horizontally.
    /// </summary>
    private MoveResult MoveInternal(Block block1, int direction)
    {
      if (block1 == null || direction == 0) return new MoveResult();
      if (block1.CanMove && block1.Movement == MovementType.None)
      {
        // Get block
        int x = block1.x + direction;
        int y = block1.y;

        var block2 = Get(x, y);

        //Log.Info("MOVE " + block1 + " <-> " + block2);

        // Do we have a block as target?
        if (block2 == null) return new MoveResult();

        // Can this block be moved (empty, not falling or falling but still high)?
        if (block2.CanMove == false) return new MoveResult();

        // Fall interruption!
        // 1/ Check if we have no collider (a block animated) at block2's position
        if (block2.Movement == MovementType.Fall && block1.InterruptableFall)
        {
          if (IsEmpty(block2.x, block2.y) == false) return new MoveResult();
        }

        // Reassign after all falls stopped
        block2 = Get(x, y);

        // Move state
        block1.Movement = MovementType.Horizontal;
        block1.DirectionX = direction;
        block1.Chainable = false;

        block2.Movement = MovementType.Horizontal;
        block2.DirectionX = -direction;
        block2.Chainable = false;

        // Unchain all tops blocks
        for (int i = y + 1; i < height; i++)
        {
          var topBlock1 = Get(block1.x, i);
          var topBlock2 = Get(block2.x, i);
          if (topBlock1 != null && topBlock1.Movement != MovementType.Fall)
          {
            topBlock1.Chainable = false;
            topBlock1.Movement = MovementType.Wait;
          }

          if (topBlock2 != null && topBlock2.Movement != MovementType.Fall)
          {
            topBlock2.Chainable = false;
            topBlock2.Movement = MovementType.Wait;
          }

          if (topBlock1 == null && topBlock2 == null)
          {
            break;
          }
        }

        var move = new MoveResult(new Vector2(block1.x, block1.y), block1, new Vector2(block2.x, block2.y), block2);

        // Swap positions
        Swap(block1.x, block1.y, block2.x, block2.y, false, true);

        return move;
      }

      return new MoveResult();
    }

    public bool ForceFall(Block block)
    {
      if (block != null && block.FallMomentum > 0)
      {
        block.FallMomentum = 0f;

        float fall = 0.2f * GetFallDepth(block); // Speed up fall!
        fall = Mathf.Clamp(fall, 0f, 0.6f);
        block.FallDuration += fall;

        return true;
      }

      return false;
    }

    #endregion

    #region Garbage

    public bool CanAddGarbage(int requiredWidth, out Vector2 position)
    {
      position = Vector2.zero;

      // Get top line
      // Check available width
      // See if it fits
      int currentWidth = 0;
      var top = GetTopLine();
      int y = 0;
      Dictionary<int, int> slots = new Dictionary<int, int>();

      // Find spaces big enough
      int x = 0;

      bool setX = true;
      foreach (var b in top)
      {
        if (setX)
        {
          x = b.x;
          setX = false;
        }

        y = b.y;

        if (b.IsEmpty)
        {
          currentWidth++;
        }
        else
        {
          if (currentWidth >= requiredWidth)
          {
            slots.Add(x, currentWidth);
          }

          currentWidth = 0;
          setX = true;
        }
      }

      // Last slot
      if (currentWidth >= requiredWidth)
      {
        slots.Add(x, currentWidth);
      }

      if (slots.Count > 0)
      {
        // Pick first or last slot
        bool first = (Random.Range(0f, 1f) > 0.5f);

        int key = -1;

        // ⚠️ weird piece of code...
        // We can't use full LINQ on iOS and you can't access a dictionary value using array index...
        int i = 0;
        foreach (var k in slots.Keys)
        {
          // Last
          if ((i == slots.Keys.Count - 1) && first == false)
          {
            key = k;
          }
          // First
          else if (i == 0 && first)
          {
            key = k;
          }

          i++;
        }

        if (key >= 0)
        {
          int w = slots[key];

          int extraSpace = w - requiredWidth;
          int shift = 0;
          if (extraSpace > 0)
          {
            shift = Random.Range(0, extraSpace + 1);
          }

          position = new Vector2(key + shift, y);
        }
        else
        {
          Log.Error("Weird error in garbage generation!");
        }

        return true;
      }

      return false;
    }

    /// <summary>
    /// Add a garbage block in the grid
    /// </summary>
    public Block AddGarbage(int garbageWidth, Vector2 position)
    {
      Log.Info("GARBAGE " + garbageWidth + " at " + position);
      if (garbageWidth == 0)
      {
        Log.Error("0 width garbage?!");
        return null;
      }

      var garbageDef = blockFactory.GetRandomGarbage();
      if (garbageDef == null)
      {
        Log.Error("Missing Garbage Definition!");
        return null;
      }

      Block first = null;
      Block previousBlock = null;
      int x = (int) position.x;
      int pY = (int) position.y;

      for (int pX = x; pX < x + garbageWidth; pX++)
      {
        var b = Get(pX, pY);

        if (b != null)
        {
          b.SetDefinition(garbageDef, true);

          b.SetFallPosition(7);

          if (first == null)
          {
            first = b;
          }
        }
        else
        {
          Log.Error("Oops. Spawning garbage on another block...");
          break;
        }

        if (previousBlock != null)
        {
          previousBlock.Right = b;
        }

        previousBlock = b;
      }

      return first;
    }

    #endregion

    #region AI

    /// <summary>
    /// Convert the grid to a simple int array, easier for AI
    /// </summary>
    /// <returns></returns>
    public sbyte[,] ToIntArray()
    {
      sbyte[,] g = new sbyte[width, height];
      for (int x = 0; x < width; x++)
      {
        for (int y = 0; y < height; y++)
        {
          g[x, y] = -1;
        }
      }

      for (int i = 0; i < blocks.Count; i++)
      {
        var b = blocks[i];

        if (b.y >= 0)
        {
          if (b.x < width && b.y < height)
          {
            g[b.x, b.y] = b.ToInt();
          }
          else
          {
            g[b.x, b.y] = -1;
          }
        }
      }

      return g;
    }

    public static string ToString(sbyte[,] g, bool clean = false)
    {
      string sGrid = "";
      int width = g.GetUpperBound(0) + 1;

      int height = g.GetUpperBound(1) + 1;
      for (int y = height - 1; y >= 0; y--)
      {
        for (int x = 0; x < width; x++)
        {
          var c = (g[x, y] == 99 ? "X" : g[x, y].ToString());
          sGrid += c + " ";
        }

        sGrid += "\n";
      }

      if (clean)
      {
        sGrid = sGrid.Replace("-1 -1 -1 -1 \n", "") // Remove preview lines
          .Replace(" \n", "\n")
          .ToLower()
          .Trim();
      }

      return sGrid;
    }

    #endregion

    #region Properties

    public Dictionary<Vector2, Block> Map => map;

    /// <summary>
    /// Gets a value indicating whether this grid is frozen.
    /// </summary>
    public bool IsFrozen => frozenCooldown > 0;

    public float FrozenCooldown => frozenCooldown;

    public bool IsTopFull => topReachedByBlock;

    public int MultiplierFrame { get; private set; }

    public Vector2 HighestBlock { get; private set; }

    public float ScrollSinceLastDing => scrollSinceLastDing;

    public int NonEmptyBlocksCount { get; private set; }

    #endregion

    #region Debug

    public void Print()
    {
      Debug.Log(ToString(ToIntArray()));
    }

#if UNITY_EDITOR

    public void ExportToDebugFile()
    {
      string gridTxt = Grid.ToString(ToIntArray());

      string path = UnityEditor.EditorUtility.SaveFilePanel("Export grid", ".", "grid", "grid");
      if (string.IsNullOrEmpty(path) == false)
      {
        System.IO.File.WriteAllText(path, gridTxt);
      }
    }

#endif

    #endregion
  }
}