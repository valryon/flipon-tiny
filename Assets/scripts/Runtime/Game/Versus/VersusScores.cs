namespace Pon.Versus
{
  public static class VersusScores
  {
    private static int P1 = 0;
    private static int P2 = 0;
    private static int P3 = 0;
    private static int P4 = 0;
    private static int P5 = 0;
    private static int P6 = 0;
    private static int P7 = 0;
    private static int P8 = 0;

    public static void Reset()
    {
      P1 = 0;
      P2 = 0;
      P3 = 0;
      P4 = 0;
      P5 = 0;
      P6 = 0;
      P7 = 0;
      P8 = 0;
    }

    public static void Win(int playerIndex)
    {
      switch (playerIndex)
      {
        case 0:
          P1++;
          break;
        case 1:
          P2++;
          break;
        case 2:
          P3++;
          break;
        case 3:
          P4++;
          break;
        case 4:
          P5++;
          break;
        case 5:
          P6++;
          break;
        case 6:
          P7++;
          break;
        case 7:
          P8++;
          break;
      }
    }

    public static int Get(int playerIndex)
    {
      switch (playerIndex)
      {
        case 0: return P1;
        case 1: return P2;
        case 2: return P3;
        case 3: return P4;
        case 4: return P5;
        case 5: return P6;
        case 6: return P7;
        case 7: return P8;
      }

      return 0;
    }
  }
}