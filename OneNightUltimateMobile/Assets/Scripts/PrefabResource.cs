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

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
