using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

public class PlayerUi : MonoBehaviour {

	private enum UiScreen {
		Uninitialized = -1,
		Night_InputControl = 0,
//		Night_InputWait = 1,
//		Night_ObservationConfirm = 2,
//		Night_ObservationWait = 3,
		Day_Voting = 4,
//		Day_Result = 5,

	}

	private UiScreen currentScreen = UiScreen.Uninitialized;

	private Dictionary<UiScreen, GameObject> screenGosByEnum = new Dictionary<UiScreen, GameObject>();

	private Player player;

	Text playerName;

	//Night input screen
	Text nightInput_Title;
	Text nightInput_Description;
	Transform nightInput_ButtonBox;

	//Day voting
	Transform dayVoting_VoteButtonBox;
	Text dayVoting_Timer;

	//Result
	Text result_Title;
	Text result_Description;

	void Start () {
		foreach(UiScreen screen in Enum.GetValues(typeof(UiScreen))) {
			if(screen == UiScreen.Uninitialized) continue;
			screenGosByEnum[screen] = transform.Find(screen.ToString()).gameObject;
		}
	
	}

	void Update () { }

	public void Initialize(Player player) {
		playerName = transform.Find("PlayerName").GetComponent<Text>();

		//Night_InputControl
		nightInput_Title = transform.Find("Night_InputControl/Title").GetComponent<Text>();
		nightInput_Description = transform.Find("Night_InputControl/Description").GetComponent<Text>();

		nightInput_ButtonBox = transform.Find("Night_InputControl/InputPanel/Grid").transform;

		//Day_Voting
		dayVoting_VoteButtonBox = transform.Find("Day_Voting/VotePanel/Grid/");
		dayVoting_Timer = transform.Find("Day_Voting/Timer").GetComponent<Text>();

		this.player = player;
		playerName.text = player.name;
	}

	public void WriteRoleToTitle() {
		nightInput_Title.text = "You are the " + player.dealtCard.role.ToString() + " " + player.dealtCard.order.ToString();
	}

	public void DisplayPrompt() { //Show team alliegence, explain night action, and describe any special rules
		SetState(UiScreen.Night_InputControl);
	}

	public void EnableVoting() {
		SetState(UiScreen.Day_Voting);
	}

	private void AddLocationButton(string label, int locationId, Transform parent) {
		GameObject go = Instantiate(PrefabResource.instance.locationButton) as GameObject;
		go.transform.SetParent(parent.transform);
		Text uiText = go.GetComponentInChildren<Text>();
		uiText.text = label;
		OnuButton onuButton = go.GetComponent<OnuButton>();
		onuButton.Initialize(this, locationId);
	}

	public void HandleButtonClick(int locationId) {
		if(currentScreen == UiScreen.Night_InputControl) {
		switch(player.prompt.options) {
		case OptionsSet.None:
		case OptionsSet.May_CenterCard:
		case OptionsSet.Must_CenterCard:
			SubmitNightAction(new int[] { locationId });
			break;
		default:
			Debug.LogError("Unhandled options set: " + player.prompt.options);
			break;
		}
		} else if(currentScreen == UiScreen.Day_Voting) {
			SubmitVote(locationId);
		}
	}

	private void SetState(UiScreen targetScreen) {
		if(targetScreen == currentScreen) return;

		switch(targetScreen) {
		case UiScreen.Night_InputControl:
			//Team allegiance- You are on the werewolf team.
			//Nature clarity if relevant- You are a villageperson.
			//Special win conditions- If there are no other werewolves, you win if an *other* player dies.
			//Cohort type- You can see other werewolves.
			//Cohort players- Allen is a werewolf.
			//Location selection- You may look at the card of another player or two cards from the center.
			//Selection controls- [Buttons for the three center cards]
			List<string> descriptionStrings = new List<string>();
			descriptionStrings.Add(player.dealtCard.team.description);
			descriptionStrings.Add(player.prompt.cohortString);
			nightInput_Description.text = string.Join(" ", descriptionStrings.ToArray());
			foreach(ButtonInfo info in player.prompt.buttons) {
				AddLocationButton(info.label, info.locationId, nightInput_ButtonBox);
			}
			break;
		case UiScreen.Day_Voting:
			string observationString = "";
			foreach(Observation observation in player.observations) {
				string locationString = GameController.instance.locationsById[observation.locationId].name;
				string gamePieceString = GameController.instance.gamePiecesById[observation.gamePieceId].name;
				observationString += " You observed " + locationString + " to be the " + gamePieceString + ".";
			}
			nightInput_Description.text += observationString;

			//Create buttons
//			print("adding " + GameController.instance.players.Count + " buttons");
			foreach(Player p in GameController.instance.players) {
				if(p == player) continue;
				AddLocationButton(p.name, p.locationId, dayVoting_VoteButtonBox);
			}
			AddLocationButton("[No one]", -1, dayVoting_VoteButtonBox);

			//Set timer

			break;
		}

		if(currentScreen == UiScreen.Uninitialized) {
			foreach(KeyValuePair<UiScreen, GameObject> kp in screenGosByEnum) {
				kp.Value.SetActive(false);
			}
		} else {
			screenGosByEnum[currentScreen].SetActive(false);
		}
		screenGosByEnum[targetScreen].SetActive(true);
		currentScreen = targetScreen;
	}

	private void SubmitNightAction(int[] locationId) {
		foreach(Transform button in nightInput_ButtonBox.transform) {
			Destroy(button.gameObject);
		}
		GameController.SubmitNightAction(player, locationId);
	}

	private void SubmitVote(int locationId) {
		foreach(Transform button in dayVoting_VoteButtonBox.transform) {
			Destroy(button.gameObject);
		}
		GameController.SubmitVote(player, locationId);
	}

}
