// Tiny Flipon by Damien Mayance
// This file is subject to the terms and conditions defined in
// file 'LICENSE.md', which is part of this source code package.

using System.Collections.Generic;
using UnityEngine;

namespace Pon
{
  public interface IBlockFactory
  {
    List<BlockDefinition> GetNormalBlocks(int level = 99, int width = 0);

    List<BlockDefinition> GetGarbages(int level = 99);

    BlockDefinition GetRandomNormal(int level, int width, GameRandom gameRandom = null,
      List<BlockDefinition> excludedDef = null);

    BlockDefinition GetRandomGarbage(int level = 99);
  }
}