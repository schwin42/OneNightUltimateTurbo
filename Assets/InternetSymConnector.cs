using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using SimpleJSON;

public class InternetSymConnector : SymRemoteConnector {

	public enum ResponseType {
		SessionStarted,
		JoinedSession,
		MessageReceived,
	}

	private const string ENDPOINT = "http://54.224.112.1:3000";

	//public 

	public override void BeginSession(SymClient client, string name) {
		JSONNode node = new JSONObject();
		node.Add("action", "open");
		node.Add("user", name);
		string json = node.ToString();

		DispatchWebRequest(client, json, ResponseType.SessionStarted);
	}

	public override void JoinSession(SymClient client, string name, string roomKey) {
		print("Attempting to join session");

		JSONNode node = new JSONObject();
		node.Add("action", "join");
		node.Add("user", name);
		node.Add("key", roomKey);
		string json = node.ToString();

		DispatchWebRequest(client, json, ResponseType.JoinedSession);

	}

	public override void BroadcastMessage(SymClient client, RemotePayload payload) {
		JSONNode node = new JSONObject();
		node.Add("action", "start");
		node.Add("user", name);
		node.Add("accessKey", client.accessKey);
		string json = node.ToString();
		DispatchWebRequest(client, json, ResponseType.MessageReceived);
	
	}

	public override void Disconnect(SymClient client) { }

//	public void HandlePayloadReceived(RemotePayload payload) {
//		client.HandleRemotePayload(payload);
//	}

	private void DispatchWebRequest(SymClient client, string postJson, ResponseType response) {
	
		StartCoroutine(SendWebRequest(client, postJson, response));
	} //add success and error callbacks

	private IEnumerator SendWebRequest(SymClient client, string postJson, ResponseType response) {

		Dictionary<string, string> headers = new Dictionary<string, string>();
		headers ["Accept"] = "application/json";
		headers ["Content-type"] = "application/json";


		byte[] postData = null;
		if(postJson != null) {
			UTF8Encoding encoding = new UTF8Encoding();
			postData = encoding.GetBytes(postJson);
		}

		print("Sending web request: " + ENDPOINT + ", " + postJson);
		WWW www = new WWW(ENDPOINT, postData, headers);

		yield return www;

		print("www returned: " + www.text + " + " + ", + error: " + www.error);

		switch(response) {
		case ResponseType.SessionStarted:
			JSONNode node = JSON.Parse(www.text);
			string userId = node["userId"];
			string accessKey = node["accessKey"];
			string roomKey = node["key"];
			client.HandleSessionStarted(userId, accessKey, roomKey);

			DispatchWait(client);
			break;
		case ResponseType.JoinedSession:
			node = JSON.Parse(www.text);
			userId = node["userId"];
			accessKey = node["accessKey"];
			client.HandleJoinedSession(userId, accessKey);

			DispatchWait(client);
			break;
		case ResponseType.MessageReceived:
			node = JSON.Parse(www.text);
			string message = node["message"];
			print("Message received: " + message);
			DispatchWait(client);
			break;
		default:
			Debug.LogError("Unhandled response type: " + response);
			break;
		}
	}

	private void DispatchWait(SymClient client) {
		JSONNode node = new JSONObject();
		node.Add("action", "wait");
		node.Add("userId", client.selfUserId);
		node.Add("accessKey", client.accessKey);
		string json = node.ToString();
		DispatchWebRequest(client, json, ResponseType.MessageReceived);
	}
}
