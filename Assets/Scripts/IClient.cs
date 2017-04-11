using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClient {

	GameMaster Gm { get; }

	string PlayerName { get; set; }
	string UserId { get; }

	void BeginSession();
	void JoinSession(string roomKey);
	void BeginGame();

	void SubmitNightAction(int[][] selection);
	void SubmitVote(int votee);

	void Disconnect ();

}
