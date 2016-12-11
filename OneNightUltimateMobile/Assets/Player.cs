using UnityEngine;
using System.Collections;
using System.Linq;


[System.Serializable]
public class Player : OnuGameObject
{
	public string playerName;

	public Card dealtCard;

	public int[] nightAction;
	public int dayVote;
	//	public Role originalRole;

	//	public Mark currentMark;
	//	public Artifact currentArtifact;

	public Player (string playerName)
	{
		this.playerName = playerName;
	}
}
