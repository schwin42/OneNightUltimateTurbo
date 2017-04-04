using UnityEngine;
using System.Collections;

public class PrefabResource : MonoBehaviour {

	private static PrefabResource _instance;
	public static PrefabResource instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<PrefabResource>();
			}
			return _instance;
		}
	}

	public GameObject locationButton;
}
