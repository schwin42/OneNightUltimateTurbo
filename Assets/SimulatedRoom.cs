using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulatedRoom : MonoBehaviour {

	//Configuration
	public GameObject clientPrefab;
	public int playerCount = 5;

	//Bookkeeping 
	List<GameController> playerGcs;

	// Use this for initialization
	void Start () {
		for(int i = 0; i < playerCount; i++) {
			GameObject go = Instantiate(clientPrefab) as GameObject;
			playerGcs.Add(go.GetComponent<GameController>());
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyUp(KeyCode.Q)) {
			playerGcs[0].StartGame();
		}
	}
}
