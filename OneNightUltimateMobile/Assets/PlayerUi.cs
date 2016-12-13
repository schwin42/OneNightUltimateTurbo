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

	//Night input screen
	Text playerName;
	Text title;
	Text description;
	Transform buttonBox;

	void Start () {
//		var thing= Enum.GetValues(typeof(UiScreen));
		foreach(UiScreen screen in Enum.GetValues(typeof(UiScreen))) {
			if(screen == UiScreen.Uninitialized) continue;
			screenGosByEnum[screen] = transform.Find(screen.ToString()).gameObject;
		}
	
	}

	void Update () { }

	public void Initialize(Player player) {
		title = transform.Find("Night_InputControl/Title").GetComponent<Text>();
		description = transform.Find("Night_InputControl/Description").GetComponent<Text>();
		playerName = transform.Find("Night_InputControl/PlayerName").GetComponent<Text>();
		buttonBox = transform.Find("Night_InputControl/InputPanel/Grid").transform;

		this.player = player;
		playerName.text = player.name;
	}

	public void WriteRoleToTitle() {
		title.text = "You are the " + player.dealtCard.role.ToString() + " " + player.dealtCard.order.ToString();
	}

	public void DisplayPrompt() { //Show team alliegence, explain night action, and describe any special rules
		SetState(UiScreen.Night_InputControl);
	}

	public void DisplayObservation() {
		string observationString = "";
		foreach(Observation observation in player.observations) {
			string locationString = GameController.instance.locationsById[observation.locationId].name;
			string gamePieceString = GameController.instance.gamePiecesById[observation.gamePieceId].name;
			observationString += " You observed " + locationString + " to be the " + gamePieceString + ".";
		}
		description.text += observationString;
	}

	private void AddNightActionButton(string label, int oguId) {
		GameObject go = Instantiate(PrefabResource.instance.nightActionButton) as GameObject;
		go.transform.SetParent(buttonBox.transform);
		Text uiText = go.GetComponentInChildren<Text>();
		uiText.text = label;
		OnuButton onuButton = go.GetComponent<OnuButton>();
		onuButton.Initialize(this, oguId);
	}

	public void HandleButtonClick(int oguId) {
		switch(player.prompt.options) {
		case OptionsSet.None:
		case OptionsSet.May_CenterCard:
		case OptionsSet.Must_CenterCard:
			SubmitNightAction(new int[] { oguId });
			break;
		default:
			Debug.LogError("Unhandled options set: " + player.prompt.options);
			break;
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
			description.text = string.Join(" ", descriptionStrings.ToArray());
			foreach(ButtonInfo info in player.prompt.buttons) {
				AddNightActionButton(info.label, info.locationId);
			}
			break;
		case UiScreen.Day_Voting:

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

	private void SubmitNightAction(int[] oguIds) {
		foreach(Transform button in buttonBox.transform) {
			Destroy(button.gameObject);
		}
		GameController.SubmitNightAction(player, oguIds);
	}

}
