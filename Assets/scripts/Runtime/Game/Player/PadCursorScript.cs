// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

namespace Pon
{
  /// <summary>
  /// Cursor script.
  /// </summary>
  public class PadCursorScript : CursorScript
  {
    #region Properties

    public override BlockScript Target
    {
      get => target;

      set =>
        // No fancy scale animation
        target = value;
    }

    #endregion
  }
}