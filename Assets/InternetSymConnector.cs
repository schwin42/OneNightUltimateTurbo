using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using SimpleJSON;

public enum ErrorType
{
	UnableToAuthenticate,
}

public class InternetSymConnector : SymRemoteConnector
{

	public enum RequestType
	{
		StartSession,
		JoinSession,
		StartGame,
		BroadcastEvent,
		Wait,
	}



	private const string ENDPOINT = "http://54.224.112.1:3000";

	//public

	public override void BeginSession (SymClient client, string name)
	{
		JSONNode node = new JSONObject ();
		node.Add ("action", "open");
		node.Add ("user", name);
		string json = node.ToString ();

		DispatchWebRequest (client, json, RequestType.StartSession);
	}

	public override void JoinSession (SymClient client, string name, string roomKey)
	{
		print ("Attempting to join session");

		JSONNode node = new JSONObject ();
		node.Add ("action", "join");
		node.Add ("user", name);
		node.Add ("key", roomKey);
		string json = node.ToString ();

		DispatchWebRequest (client, json, RequestType.JoinSession);

	}

	public override void StartGame (SymClient client)
	{
		JSONNode node = new JSONObject ();
		node.Add ("action", "start");
		node.Add ("user", name);
		node.Add ("accessKey", client.accessKey);
		string json = node.ToString ();
		DispatchWebRequest (client, json, RequestType.StartGame);
	}

	public override void BroadcastMessage (SymClient client, RemotePayload payload)
	{

	
	}

	public override void Disconnect (SymClient client)
	{
	}

	//	public void HandlePayloadReceived(RemotePayload payload) {
	//		client.HandleRemotePayload(payload);
	//	}

	private void DispatchWebRequest (SymClient client, string postJson, RequestType response)
	{
	
		StartCoroutine (SendWebRequest (client, postJson, response));
	}
	//add success and error callbacks

	private IEnumerator SendWebRequest (SymClient client, string postJson, RequestType response)
	{

		Dictionary<string, string> headers = new Dictionary<string, string> ();
		headers ["Accept"] = "application/json";
		headers ["Content-type"] = "application/json";


		byte[] postData = null;
		if (postJson != null) {
			UTF8Encoding encoding = new UTF8Encoding ();
			postData = encoding.GetBytes (postJson);
		}

		print ("Sending web request: " + ENDPOINT + ", " + postJson);
		WWW www = new WWW (ENDPOINT, postData, headers);

		yield return www;

		if (www.error != null) {
			print ("Error received: " + www.error + ", " + www.text);
		}

		print ("www returned: " + www.text + " + " + ", + error: " + www.error);

		switch (response) {
			case RequestType.StartSession:
				JSONNode node = JSON.Parse (www.text);
				string userId = node ["userId"];
				string accessKey = node ["accessKey"];
				string roomKey = node ["key"];
				client.HandleSessionStarted (userId, accessKey, roomKey);
				DispatchWait (client);
				break;
			case RequestType.JoinSession:
				node = JSON.Parse (www.text);
				accessKey = node ["accessKey"];
				if (accessKey == null) {
					client.HandleRemoteError (ErrorType.UnableToAuthenticate);
					yield break;
				}
				userId = node ["userId"];
				List<string> users = new List<string> ();
				for (int i = 0; i < node ["users"].Count; i++) {
					users.Add (node ["users"] [i]);
				}
//				JSONArray userArray = node ["users"].AsArray;
//				List<string> users = new List<string> ();
//				foreach (JSONNode childNode in userArray) {
//					users.Add (childNode.ToString ());
//				}
				client.HandleJoinedSession (userId, accessKey, users);
				DispatchWait (client);
				break;
			case RequestType.Wait:
				node = JSON.Parse (www.text);
				string message = node ["message"];
				switch (message) {
					case "player joined":
						userId = node ["userId"];
						client.HandleOtherJoined (userId);
						break;
					default:
						Debug.LogError ("Unhandled message: " + www.text);
						break;
				}

				string payload = node ["payload"];
//				client.HandleRemotePayload (payload);
				DispatchWait (client);
				break;
			case RequestType.StartGame:
				//Check for game started message, and dispatch start game payload if it exists
//				node = JSON.Parse (www.text);
//				message = node ["message"];
//				if (message == "started") {
//
//				} else {
//					Debug.LogError("Unexpected start game response
//				}
//				client.HandleRemotePayload (payload);
				break;
			case RequestType.BroadcastEvent:
				//Will be received by wait call, do nothing
				break;
			default:
				Debug.LogError ("Unhandled response type: " + response);
				break;
		}
	}

	private void DispatchWait (SymClient client)
	{
		JSONNode node = new JSONObject ();
		node.Add ("action", "wait");
		node.Add ("userId", client.selfUserId);
		node.Add ("accessKey", client.accessKey);
		string json = node.ToString ();
		DispatchWebRequest (client, json, RequestType.Wait);
	}
}
