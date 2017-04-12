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
		Origin origin = new Origin (client);
		JSONNode node = new JSONObject ();
		node.Add ("action", "open");
		node.Add ("user", name);
		string json = node.ToString ();

		DispatchWebRequest (origin, json, RequestType.StartSession);
	}

	public override void JoinSession (SymClient client, string name, string roomKey)
	{
		print ("Attempting to join session");
		Origin origin = new Origin (client);
		JSONNode node = new JSONObject ();
		node.Add ("action", "join");
		node.Add ("user", name);
		node.Add ("key", roomKey);
		string json = node.ToString ();

		DispatchWebRequest (origin, json, RequestType.JoinSession);

	}

	public override void StartGame (SymClient client, StartGamePayload payload)
	{
		Origin origin = new Origin (client, payload);
		JSONNode node = new JSONObject ();
		node.Add ("action", "start");
		node.Add ("user", client.selfUserId);
		node.Add ("accessKey", client.accessKey);
		string json = node.ToString ();
		DispatchWebRequest (origin, json, RequestType.StartGame);
	}

	public override void BroadcastMessage (SymClient client, RemotePayload payload) {
		Origin origin = new Origin (client);
		DispatchBroadcast (origin, payload);
	}

	public override void Disconnect (SymClient client)
	{
	}

	//	public void HandlePayloadReceived(RemotePayload payload) {
	//		client.HandleRemotePayload(payload);
	//	}

	private void DispatchWebRequest (Origin origin, string postJson, RequestType request) {
		StartCoroutine (SendWebRequest (origin, postJson, request));
	}
	//add success and error callbacks

	private IEnumerator SendWebRequest (Origin origin, string postJson, RequestType request)
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

		print ("www returned for " + origin.client.UserId + ": " + www.text);

		switch (request) {
			case RequestType.StartSession:
				JSONNode node = JSON.Parse (www.text);
				string userId = node ["userId"];
				string accessKey = node ["accessKey"];
				string roomKey = node ["key"];
				origin.client.HandleSessionStarted (userId, accessKey, roomKey);
				DispatchWait (origin);
				break;
			case RequestType.JoinSession:
				node = JSON.Parse (www.text);
				accessKey = node ["accessKey"];
				if (accessKey == null) {
					origin.client.HandleRemoteError (ErrorType.UnableToAuthenticate);
					yield break;
				}
				userId = node ["userId"];
				List<string> users = new List<string> ();
				for (int i = 0; i < node ["users"].Count; i++) {
					users.Add (node ["users"] [i]);
				}
				origin.client.HandleJoinedSession (userId, accessKey, users);
				DispatchWait (origin);
				break;
			case RequestType.Wait:
				node = JSON.Parse (www.text);
				string message = node ["message"];
				switch (message) {
					case "player joined":
						userId = node ["userId"];
						origin.client.HandleOtherJoined (userId);
						break;
					case "start":
						//Game started, but I don't think we really care
						break;
					default:
						Debug.LogError ("Unhandled message: " + www.text);
						break;
				}

				string payload = node ["payload"];
//				client.HandleRemotePayload (payload);
				DispatchWait (origin);
				break;
			case RequestType.StartGame:
				//Check for game started message, and dispatch start game payload if it exists
				node = JSON.Parse (www.text);
				message = node ["message"];
				if (message == "started") {
					DispatchBroadcast (origin, origin.payload);
				} else {
					Debug.LogError ("Unexpected start game response: " + message);
				}
//				client.HandleRemotePayload (payload);
				break;
			case RequestType.BroadcastEvent:
				//Will be received by wait call, do nothing
				break;
			default:
				Debug.LogError ("Unhandled response type: " + request);
				break;
		}
	}

	private void DispatchWait (Origin origin)
	{
		JSONNode node = new JSONObject ();
		node.Add ("action", "wait");
		node.Add ("userId", origin.client.selfUserId);
		node.Add ("accessKey", origin.client.accessKey);
		string json = node.ToString ();
		DispatchWebRequest (origin, json, RequestType.Wait);
	}

	private void DispatchBroadcast (Origin origin, RemotePayload payload) {
		JSONNode node = new JSONObject ();
		node.Add ("action", "tell");
		JSONArray userArray = new JSONArray ();
		for (int i = 0; i < origin.client.connectedUsers.Count; i++) {
			userArray.Add (origin.client.connectedUsers [i]);
		}
		node.Add ("users", userArray);

		if (payload is StartGamePayload) {
			JSONNode payloadNode = new JSONObject ();
			payloadNode.Add ("message", PayloadType.InitiateGame.ToString ());
			payloadNode.Add ("myData", ((StartGamePayload)origin.payload).randomSeed);
			node.Add ("payload", payloadNode);
		} else {
			Debug.LogError ("Unhandled payload type: " + payload);
		}

		DispatchWebRequest (origin, node.ToString(), RequestType.BroadcastEvent);
	}

	public class Origin {
		public SymClient client;
		public RemotePayload payload;

		public Origin (SymClient client) {
			this.client = client;
		}

		public Origin(SymClient client, RemotePayload payload) {
			this.client = client;
			this.payload = payload;
		}
	}
}
