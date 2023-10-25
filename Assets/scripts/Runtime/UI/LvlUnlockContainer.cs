using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LvlUnlockContainer : MonoBehaviour
{
	public bool[] LvlUnlockStates;

	public void SetupState(int index, bool isUnlocked)
	{
		LvlUnlockStates[index] = isUnlocked;
	}

	// Print Level Unlock States for Testing/Debugging
	public void PrintArray()
	{
		//Debug.Log($"[{string.Join(",", LvlUnlockStates)}]");
	}
}
