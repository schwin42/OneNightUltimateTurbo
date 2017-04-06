using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AsymRemoteManager : NetworkManager {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}



	public override void OnServerConnect(NetworkConnection connection) {
		Debug.Log("Player Connected.");
	}
		
	public override void OnStartHost ()
	{
		Debug.Log("Host started");
	}
}
