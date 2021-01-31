// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;
using UnityEngine;

namespace Pon
{
  public enum MovementType
  {
    None,
    Fall,
    Horizontal,
    Wait
  }

  [Serializable]
  public class Block
  {
    #region Members

    public int x;
    public int y;
    public Vector2 position;
    public Vector2 movingSpeed = Vector2.one; // ⚠️ Change it in BlockScript.cs!

    public Action<bool, bool> OnActivationChanged;
    public Action OnEmpty;
    public Action<int, int, Action> OnEmptyWithAnimation;
    public Action<int, int, BlockDefinition, Action> OnConversionAnimation;
    public Action<BlockDefinition> OnNewDefinition;
    public Action OnNewNeighbor;

    #endregion

    #region Constructor

    public Block(Vector2 p)
    {
      x = (int) p.x;
      y = (int) p.y;
      IsEmpty = true;

      position = new Vector2(x, y);

      UpdateState();
    }

    public Block()
      : this(Vector2.zero)
    {
    }

    public Block(int x, int y)
      : this(new Vector2(x, y))
    {
    }

    #endregion

    #region Public methods

    public void SetPosition(int px, int py)
    {
      x = px;
      y = py;
      position = new Vector2(px, py);
    }

    public void SetFallPosition(float py)
    {
      position += new Vector2(0, py);
      FallMomentum = 0f;
      Movement = MovementType.Fall;
    }

    public void SetDefinition(BlockDefinition def, bool active)
    {
      Empty();

      definition = def;
      IsEmpty = definition == null;
      Chainable = false;
      UpdateState();

      OnNewDefinition.Raise(definition);

      if (active) Activate(false);
      else Deactivate();
    }

    /// <summary>
    /// Block has reach a new line
    /// </summary>
    public void LineUp()
    {
      y += 1;
      PreviousY += 1;
    }

    /// <summary>
    /// Empty the block
    /// </summary>
    public void Empty()
    {
      Right = null;
      Left = null;

      definition = null;
      IsEmpty = true;
      movement = MovementType.None;
      isBeingRemoved = false;
      IsConverting = false;

      UpdateState();

      OnEmpty.Raise();
    }

    public void EmptyWithAnimation(int n, int count)
    {
      isBeingRemoved = true;
      UpdateState();

      if (OnEmptyWithAnimation != null)
      {
        OnEmptyWithAnimation.Raise(n, count, Empty);
      }
      else
      {
        Empty();
      }
    }

    /// <summary>
    /// Make the block inactive
    /// </summary>
    public void Deactivate()
    {
      IsActive = false;
      Chainable = false;
      OnActivationChanged.Raise(IsActive, false);
    }

    /// <summary>
    /// Make the block active
    /// </summary>
    public void Activate(bool animate)
    {
      if (IsActive == false)
      {
        IsActive = true;
        Chainable = false;
        OnActivationChanged.Raise(IsActive, animate);
      }
    }

    private void UpdateState()
    {
      // Try to reduce how often we compute those stuff. It's VERY expensive
      CanCombo = (IsEmpty == false && definition.isGarbage == false) && IsEmpty == false && isActive &&
                 movement == MovementType.None && isBeingRemoved == false
                 && IsConverting == false
                 && FrameCount >= 2;

      CanMove = (IsEmpty || (IsEmpty == false && definition.isGarbage == false))
                && (isActive || IsEmpty)
                && (movement == MovementType.None || InterruptableFall)
                && isBeingRemoved == false;

      IsDestructable = IsActive && IsEmpty == false && IsBeingRemoved == false && Movement == MovementType.None &&
                       Definition.isGarbage == false && IsConverting == false;
    }

    /// <summary>
    /// Convert the block (garbage for example) to another one
    /// </summary>
    public float ConvertTo(int n, int count, BlockDefinition blockDefinition, Action convertCallback = null)
    {
      IsConverting = true;

      float baseTime = 0.8f;
      if (IsEmpty == false && definition.isGarbage)
      {
        if (count > 1) baseTime = 1.2f;
      }

      var fm = baseTime - (n * 0.05f);

      // Animate if possible
      if (OnConversionAnimation != null)
      {
        OnConversionAnimation.Raise(n, count, blockDefinition, () =>
        {
          SetDefinition(blockDefinition, true);
          FallMomentum = fm;
          Chainable = true;
          convertCallback.Raise();
        });
      }
      else
      {
        SetDefinition(blockDefinition, true);
        FallMomentum = fm;
        Chainable = true;
        convertCallback.Raise();
      }

      return fm;
    }

    public sbyte ToInt()
    {
      if (IsEmpty || IsBeingRemoved || !isActive)
      {
        return 0;
      }

      if (definition.isGarbage)
      {
        return 99;
      }

      return definition.id;
    }

    #endregion

    #region Properties

    private bool isActive;

    /// <summary>
    /// Block is active (not in stash).
    /// </summary>
    public bool IsActive
    {
      get => isActive;
      set
      {
        isActive = value;
        UpdateState();
      }
    }


    private BlockDefinition definition;

    public BlockDefinition Definition => definition;

    private bool isBeingRemoved;

    /// <summary>
    /// Currently being removed (still there but animated and soon destroyed).
    /// </summary>
    public bool IsBeingRemoved => isBeingRemoved;

    public bool IsConverting { get; private set; }

    private MovementType movement;

    /// <summary>
    /// Moving down, left or right.
    /// </summary>
    public MovementType Movement
    {
      get => movement;
      set
      {
        PreviousMovement = movement;
        movement = value;

        if (PreviousMovement != movement)
        {
          FrameCount = 0;
        }

        UpdateState();
      }
    }

    public MovementType PreviousMovement { get; private set; }

    /// <summary>
    /// If moving horizontally, the old X position.
    /// </summary>
    public int PreviousX { get; set; }

    /// <summary>
    /// If falling, Y position to reach.
    /// </summary>
    public int PreviousY { get; set; }

    public int DirectionX { get; set; }

    /// <summary>
    /// Empty tile.
    /// </summary>
    public bool IsEmpty { get; private set; }

    public int FrameCount;

    // No get/set? WHY?!
    // Because it's too expensive here.

    /// <summary>
    /// Available for combo.
    /// </summary>
    public bool CanCombo;

    /// <summary>
    /// Can be moved horizontally.
    /// </summary>
    public bool CanMove;

    public bool IsDestructable;

    public bool InterruptableFall => (Movement == MovementType.Fall && (position.y > y + 1));

    private Block right;

    public Block Left { get; private set; }

    public Block Right
    {
      get => right;
      set
      {
        right = value;

        if (right != null)
        {
          right.Left = this;

          right.OnNewNeighbor.Raise();

          // Raise events on all children
          var b = Leftest;
          while (b != null)
          {
            b.OnNewNeighbor.Raise();
            b = b.Right;
          }
        }
      }
    }

    public Block Leftest
    {
      get
      {
        if (Left == null) return this;

        var l = Left;
        while (l != null && l.Left != null)
        {
          if (l.Left != null)
          {
            l = l.Left;
          }
          else
          {
            break;
          }
        }

        return l;
      }
    }

    public Block Rightest
    {
      get
      {
        if (Right == null) return this;

        var r = Right;
        while (r != null && r.Right != null)
        {
          if (r.Right != null)
          {
            r = r.Right;
          }
          else
          {
            break;
          }
        }

        return r;
      }
    }

    public float FallDuration { get; set; }

    public float FallMomentum { get; set; }

    public bool Chainable;

    #endregion

    #region Debug

    public override string ToString()
    {
      return string.Format("[{3} X={0}, Y={1} P={6}, IsActive={2}, Movement={4}, Removed={5}]",
        x,
        y,
        IsActive,
        (IsEmpty ? "EMPTY" : (definition.isGarbage ? "GARBAGE" : definition.name)),
        Movement,
        IsBeingRemoved,
        position);
    }

    #endregion
  }
}