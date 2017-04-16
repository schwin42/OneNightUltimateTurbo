using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using SimpleJSON;
using System.Linq;

public enum ErrorType
{
	Generic,
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

	//State
	public Dictionary<OnutClient, List<CoroutineInfo>> activeCoroutinesByClient = new Dictionary<OnutClient, List<CoroutineInfo>> ();
	public int nextRequestId = 0;

	public override void BeginSession (OnutClient client, string name) {
		JSONNode node = new JSONObject ();
		node.Add ("action", "open");
		node.Add ("user", name);
		string json = node.ToString ();

		DispatchWebRequest (client, json, RequestType.StartSession);
	}

	public override void JoinSession (OnutClient client, string name, string roomKey) {
		JSONNode node = new JSONObject ();
		node.Add ("action", "join");
		node.Add ("user", name);
		node.Add ("key", roomKey);
		string json = node.ToString ();

		DispatchWebRequest (client, json, RequestType.JoinSession);

	}

	public override void StartGame (OnutClient client, StartGamePayload payload)
	{
		JSONNode node = new JSONObject ();
		node.Add ("action", "start");
		node.Add ("user", client.selfUserId);
		JSONArray userArray = new JSONArray ();
		for (int i = 0; i < client.connectedUsers.Count; i++) {
			userArray.Add (client.connectedUsers [i]);
		}
		node.Add ("users", userArray);
		node.Add ("accessKey", client.accessKey);
		JSONNode payloadNode = new JSONObject ();
		if (payload is StartGamePayload) {
			payloadNode.Add ("message", PayloadType.InitiateGame.ToString ());
			payloadNode.Add ("randomSeed", ((StartGamePayload)payload).randomSeed);
		}
		node.Add("payload", payloadNode);
		string json = node.ToString ();
		DispatchWebRequest (client, json, RequestType.StartGame);
	}

	public override void BroadcastPayload (OnutClient client, RemotePayload payload) {
		DispatchBroadcast (client, payload);
	}

	public override void Disconnect (OnutClient client) {
		foreach(CoroutineInfo ci in activeCoroutinesByClient[client]) {
			StopCoroutine(ci.iEnumerator);
		}
		activeCoroutinesByClient.Remove(client);
	}

	//	public void HandlePayloadReceived(RemotePayload payload) {
	//		client.HandleRemotePayload(payload);
	//	}

	private void DispatchWebRequest (OnutClient client, string postJson, RequestType request)
	{
		int requestId = nextRequestId++;
		Origin origin = new Origin(client, requestId);
		CoroutineInfo coroutine = new CoroutineInfo(requestId, SendWebRequest (origin, postJson, request));
		if(activeCoroutinesByClient.ContainsKey(client)) {
			activeCoroutinesByClient[client].Add(coroutine);
		} else {
			List<CoroutineInfo> list = new List<CoroutineInfo>();
			list.Add(coroutine);
			activeCoroutinesByClient.Add(client, list);
		}

		StartCoroutine (coroutine.iEnumerator);
	}
	//add success and error callbacks

	private IEnumerator SendWebRequest (Origin origin, string postJson, RequestType request) {

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

		print("dequeuing coroutine with count for client: " + activeCoroutinesByClient[origin.client].Count);
		activeCoroutinesByClient[origin.client].Remove(activeCoroutinesByClient[origin.client].Single(ci => ci.requestId == origin.requestId));

		if (www.error != null) {
			origin.client.HandleRemoteError(ErrorType.Generic, www.error + ", " + www.text);
			yield break;
		}

		print (origin.client.UserId + " received www: " + www.text);

		switch (request) {
		case RequestType.StartSession:
			JSONNode node = JSON.Parse (www.text);
			string userId = node ["userId"];
			string accessKey = node ["accessKey"];
			string roomKey = node ["key"];
			origin.client.HandleSessionStarted (userId, accessKey, roomKey);
			DispatchWait (origin.client);
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
			DispatchWait (origin.client);
			break;
		case RequestType.Wait: //Receive payload
			node = JSON.Parse (www.text);
			string message = node ["message"];
			switch (message) {
			case "player joined":
				userId = node ["userId"];
				origin.client.HandleOtherJoined (userId);
				break;
			case "player left":
				userId = node["userId"];
				origin.client.HandleOtherLeft(userId);
				break;
//			case "start":
//				//Game started, but I don't think we really care
//				break;
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
			DispatchWait (origin.client);
			break;
		case RequestType.StartGame:
			//Should be no need to do anything here


//				//Check for game started message, and dispatch start game payload if it exists
//			node = JSON.Parse (www.text);
//			message = node ["message"];
//			if (message == "started") {
////				DispatchBroadcast (origin, origin.payload);
//			} else {
//				Debug.LogError ("Unexpected start game response: " + message);
//			}
////				client.HandleRemotePayload (payload);
			break;
		case RequestType.BroadcastEvent:
				//Will be received by wait call, do nothing
			break;
		default:
			Debug.LogError ("Unhandled response type: " + request);
			break;
		}
	}

	private void DispatchWait (OnutClient client)
	{
		JSONNode node = new JSONObject ();
		node.Add ("action", "wait");
		node.Add ("userId", client.selfUserId);
		node.Add ("accessKey", client.accessKey);
		string json = node.ToString ();
		DispatchWebRequest (client, json, RequestType.Wait);
	}

	private void DispatchBroadcast (OnutClient client, RemotePayload payload)
	{
		JSONNode node = new JSONObject ();
		node.Add ("action", "tell");
		node.Add ("accessKey", client.accessKey);
		JSONArray userArray = new JSONArray ();
		for (int i = 0; i < client.connectedUsers.Count; i++) {
			userArray.Add (client.connectedUsers [i]);
		}
		node.Add ("users", userArray);

		JSONNode payloadNode = new JSONObject ();
		if (payload is StartGamePayload) {
			payloadNode.Add ("message", PayloadType.InitiateGame.ToString ());
			payloadNode.Add ("randomSeed", ((StartGamePayload)payload).randomSeed);
		}else if(payload is ActionPayload) {
			payloadNode.Add("message", PayloadType.SubmitAction.ToString());
			JSONNode actionNode = new JSONObject();
			actionNode.Add("sourceUserId", client.selfUserId);

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
			voteNode.Add("sourceUserId", client.selfUserId);
			voteNode.Add("votee", ((VotePayload)payload).voteeLocationId);
			payloadNode.Add("vote", voteNode);
		} else {
			Debug.LogError ("Unhandled payload type: " + payload);
		}

		node.Add("payload", payloadNode);
		DispatchWebRequest (client, node.ToString (), RequestType.BroadcastEvent);
	}

	public class Origin
	{
		public OnutClient client;
		public int requestId;

		public Origin (OnutClient client, int requestId) {
			this.client = client;
			this.requestId = requestId;
		}
	}

	public struct CoroutineInfo {
		public int requestId;
		public IEnumerator iEnumerator;

		public CoroutineInfo (int requestId, IEnumerator coroutine) {
			this.requestId = requestId;
			this.iEnumerator = coroutine;
		}
	}
}
