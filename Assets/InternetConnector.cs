using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using SimpleJSON;
using System.Linq;

public enum ErrorType
{
	UnableToAuthenticate,
}

public class InternetConnector : RemoteConnector
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

	public override void BeginSession (OnumClient client, string name)
	{
		Origin origin = new Origin (client);
		JSONNode node = new JSONObject ();
		node.Add ("action", "open");
		node.Add ("user", name);
		string json = node.ToString ();

		DispatchWebRequest (origin, json, RequestType.StartSession);
	}

	public override void JoinSession (OnumClient client, string name, string roomKey)
	{
//		print ("Attempting to join session");
		Origin origin = new Origin (client);
		JSONNode node = new JSONObject ();
		node.Add ("action", "join");
		node.Add ("user", name);
		node.Add ("key", roomKey);
		string json = node.ToString ();

		DispatchWebRequest (origin, json, RequestType.JoinSession);

	}

	public override void StartGame (OnumClient client, StartGamePayload payload)
	{
		Origin origin = new Origin (client, payload);
		JSONNode node = new JSONObject ();
		node.Add ("action", "start");
		node.Add ("user", client.selfUserId);
		node.Add ("accessKey", client.accessKey);
		string json = node.ToString ();
		DispatchWebRequest (origin, json, RequestType.StartGame);
	}

	public override void BroadcastPayload (OnumClient client, RemotePayload payload)
	{
		Origin origin = new Origin (client);
		DispatchBroadcast (origin, payload);
	}

	public override void Disconnect (OnumClient client)
	{
	}

	//	public void HandlePayloadReceived(RemotePayload payload) {
	//		client.HandleRemotePayload(payload);
	//	}

	private void DispatchWebRequest (Origin origin, string postJson, RequestType request)
	{
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

		print (origin.client.selfUserId + "Sending web request: " + ENDPOINT + ", " + postJson);
		WWW www = new WWW (ENDPOINT, postData, headers);

		yield return www;

		if (www.error != null) {
			print ("Error received: " + www.error + ", " + www.text);
		}

		print (origin.client.UserId + " received www: " + www.text);

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
		case RequestType.Wait: //Receive payload
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
			case "InitiateGame":
				int randomSeed = node ["randomSeed"];
				origin.client.HandleGameStarted (randomSeed);
				break;
			case "SubmitAction":
				List<List<int>> selection = new List<List<int>>();
				JSONNode actionNode = node["gameAction"];
				string sourceUserId = actionNode["sourceUserId"];
				JSONArray rows = actionNode["selection"].AsArray;
				for(int i = 0; i < rows.Count; i++) {
					JSONArray columns = rows[i].AsArray;
					List<int> destColumns = new List<int>();
					for(int j = 0; j < columns.Count; j++) {
						destColumns.Add(columns[j].AsInt);
					}
					selection.Add(destColumns);
				}
				origin.client.HandleActionMessage(sourceUserId, selection.Select(a => a.ToArray()).ToArray());
				break;
			case "SubmitVote":
				JSONNode voteNode = node["vote"];
				sourceUserId = voteNode["sourceUserId"];
				int votee = voteNode["votee"];
				origin.client.HandleVoteMessage(sourceUserId, votee);
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

	private void DispatchBroadcast (Origin origin, RemotePayload payload)
	{
		JSONNode node = new JSONObject ();
		node.Add ("action", "tell");
		node.Add ("accessKey", origin.client.accessKey);
		JSONArray userArray = new JSONArray ();
		for (int i = 0; i < origin.client.connectedUsers.Count; i++) {
			userArray.Add (origin.client.connectedUsers [i]);
		}
		node.Add ("users", userArray);

		JSONNode payloadNode = new JSONObject ();
		if (payload is StartGamePayload) {
			payloadNode.Add ("message", PayloadType.InitiateGame.ToString ());
			payloadNode.Add ("randomSeed", ((StartGamePayload)origin.payload).randomSeed);
		}else if(payload is ActionPayload) {
			payloadNode.Add("message", PayloadType.SubmitAction.ToString());
			JSONNode actionNode = new JSONObject();
			actionNode.Add("sourceUserId", origin.client.selfUserId);

			int[][] intSelection = ((ActionPayload)payload).selection;
			JSONArray jsonSelection = new JSONArray();
			for(int i = 0; i < intSelection.Length; i++) {
				JSONArray jsonCells = new JSONArray();
				for(int j = 0; j < intSelection[i].Length; j++) {
					jsonCells.Add(intSelection[i][j]);
				}
				jsonSelection.Add(jsonCells);
			}

			actionNode.Add("selection", jsonSelection);
			payloadNode.Add("gameAction", actionNode);
		} else if (payload is VotePayload) {
			
			payloadNode.Add("message", PayloadType.SubmitVote.ToString());
			JSONNode voteNode = new JSONObject();
			voteNode.Add("sourceUserId", origin.client.selfUserId);
			voteNode.Add("votee", ((VotePayload)payload).voteeLocationId);
			payloadNode.Add("vote", voteNode);
		} else {
			Debug.LogError ("Unhandled payload type: " + payload);
		}

		node.Add("payload", payloadNode);
		DispatchWebRequest (origin, node.ToString (), RequestType.BroadcastEvent);
	}

	public class Origin
	{
		public OnumClient client;
		public RemotePayload payload;

		public Origin (OnumClient client)
		{
			this.client = client;
		}

		public Origin (OnumClient client, RemotePayload payload)
		{
			this.client = client;
			this.payload = payload;
		}
	}
}
