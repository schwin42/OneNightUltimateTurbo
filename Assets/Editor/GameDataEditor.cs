using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameDataEditor : MonoBehaviour {

	[MenuItem("ONU/Load Data from File")]
	public static void LoadDataFromFile()
	{
		GameData.instance.LoadDataFromFile();
	}
}
