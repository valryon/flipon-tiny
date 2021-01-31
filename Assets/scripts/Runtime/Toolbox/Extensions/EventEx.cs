// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System;

namespace Pon
{
  public static class EventEx
  {
    #region Members

    #endregion

    #region Constructors

    #endregion

    #region Methods

    public static void Raise(this Action self)
    {
      var handler = self;
      if (handler != null) handler();
    }

    // Because it's not possible to have a generic with multiple params…
    // (http://stackoverflow.com/questions/15417174/using-the-params-keyword-for-generic-parameters-in-c-sharp)
    // Here we go:

    public static void Raise<T>(this Action<T> self, T arg01)
    {
      var handler = self;
      if (handler != null) handler(arg01);
    }

    public static void Raise<T1, T2>(this Action<T1, T2> self, T1 arg01, T2 arg02)
    {
      var handler = self;
      if (handler != null) handler(arg01, arg02);
    }

    public static void Raise<T1, T2, T3>(this Action<T1, T2, T3> self, T1 arg01, T2 arg02, T3 arg03)
    {
      var handler = self;
      if (handler != null) handler(arg01, arg02, arg03);
    }

    public static void Raise<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> self, T1 arg01, T2 arg02, T3 arg03, T4 arg04)
    {
      var handler = self;
      if (handler != null) handler(arg01, arg02, arg03, arg04);
    }

    #endregion
  }
}