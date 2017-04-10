using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IClient {

	GameMaster Gm { get; }

	string PlayerName { get; set; }
	int ClientId { get; }

	void JoinSession(string networkAddress = null);
	void BeginGame();

	void SubmitNightAction(int[][] selection);
	void SubmitVote(int votee);


}
