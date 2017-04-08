using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AsymRemoteManager : NetworkManager {

	public AsymClient asymClient;

//	public override void OnServerConnect(NetworkConnection connection) {
//		Debug.Log("OnServerConnect");
//	}
		
	public override void OnStartHost ()
	{
		Debug.Log("OnStartHost");
		PlayerUi.singleton.HandleHostStarted (asymClient.playerName);
	}

	public override void OnClientConnect (NetworkConnection conn)
	{
		Debug.Log ("OnClientConnect");
	}

	public override void OnClientDisconnect (NetworkConnection conn)
	{
		Debug.Log ("Lost connection");
	}

	public override void OnServerDisconnect (NetworkConnection conn)
	{
		Debug.Log ("Player left.");
	}

	public override void OnClientError (NetworkConnection conn, int errorCode)
	{
		Debug.LogError ("Client error: " + errorCode);
	}

	public override void OnServerError (NetworkConnection conn, int errorCode)
	{
		Debug.LogError ("Server error: " + errorCode);
	}
}
