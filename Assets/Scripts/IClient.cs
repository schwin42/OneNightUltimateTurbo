using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClient {

	GameMaster Gm { get; }

	string UserId { get; }

	void BeginSession(string name);
	void JoinSession(string name, string roomKey);
	void InitiateGame();

	void SubmitNightAction(int[][] selection);
	void SubmitVote(int votee);

	void Disconnect ();

}
