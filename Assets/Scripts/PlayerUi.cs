using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;

public class PlayerUi : MonoBehaviour {

	public enum UiScreen {
		Uninitialized = -1,
		PlayerEntry = 0,
		Night_InputControl = 1,
		Lobby = 2,
		Day_Voting = 3,
		Result = 4,

	}

	private UiScreen currentScreen = UiScreen.Uninitialized;

	private Dictionary<UiScreen, GameObject> screenGosByEnum = new Dictionary<UiScreen, GameObject>();

	Client client;
	GamePlayer gamePlayer;

	Text playerName;

	//Player
	InputField playerEntry_NameField;

	//Lobby
	Text lobby_playersLabel;
	Button lobby_startButton;

	//Night input screen
	Text nightInput_Title;
	Text nightInput_Description;
	Transform nightInput_ButtonBox;
	List<int> night_Selections;

	//Day voting
	Transform day_VoteButtonBox;
	Text day_Description;
//	Text day_Timer;

	//Result
	Text result_Title;
	Text result_Description;

	public void Initialize(Client client) {

		this.client = client;

		foreach (UiScreen screen in Enum.GetValues(typeof(UiScreen))) {
			if (screen == UiScreen.Uninitialized) continue;
			screenGosByEnum[screen] = transform.Find(screen.ToString()).gameObject;
		}

		playerName = transform.Find("PlayerName").GetComponent<Text>();

		//PlayerEntry
		playerEntry_NameField = transform.Find("PlayerEntry/NameField").GetComponent<InputField>();

		//Lobby
		lobby_playersLabel = transform.Find("Lobby/Description").GetComponent<Text>();
		lobby_startButton = transform.Find ("Lobby/StartButton").GetComponent<Button> ();

		//Night_InputControl
		nightInput_Title = transform.Find("Night_InputControl/Title").GetComponent<Text>();
		nightInput_Description = transform.Find("Night_InputControl/Description").GetComponent<Text>();

		nightInput_ButtonBox = transform.Find("Night_InputControl/Grid").transform;

		//Day_Voting
		day_VoteButtonBox = transform.Find("Day_Voting/Grid/");
		day_Description = transform.Find("Day_Voting/Description").GetComponent<Text>();
//		day_Timer = transform.Find("Day_Voting/Timer").GetComponent<Text>();

		//Result
		result_Title = transform.Find("Result/Title").GetComponent<Text>();
		result_Description = transform.Find("Result/Description").GetComponent<Text>();

		SetState(UiScreen.PlayerEntry);

	}

	public void WriteRoleToTitle() {
		nightInput_Title.text = "You are the " + gamePlayer.dealtCard.data.role.ToString() + " " + gamePlayer.dealtCard.data.order.ToString();
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
		switch(gamePlayer.prompt.options) {
		case OptionsSet.None:
		case OptionsSet.May_CenterCard:
		case OptionsSet.Must_CenterCard:
		case OptionsSet.May_OtherPlayer:
			SubmitNightAction(new int[] { locationId });
			break;
		case OptionsSet.May_TwoOtherPlayers:
				night_Selections.Add(locationId);
				if(night_Selections.Count > 1) {
					SubmitNightAction(night_Selections.ToArray());
				}
			break;
		default:
			Debug.LogError("Unhandled options set: " + gamePlayer.prompt.options);
			break;
		}
		} else if(currentScreen == UiScreen.Day_Voting) {
			SubmitVote(locationId);
		}
	}

	public void SetState(UiScreen targetScreen)
	{
		if (targetScreen == currentScreen) return;

			switch (targetScreen) {
				case UiScreen.Night_InputControl:
					//Team allegiance- You are on the werewolf team.
					//Nature clarity if relevant- You are a villageperson.
					//Special win conditions- If there are no other werewolves, you win if an *other* player dies.
					//Cohort type- You can see other werewolves.
					//Cohort players- Allen is a wersewolf.
					//Location selection- You may look at the card of another player or two cards from the center.
					//Selection controls- [Buttons for the three center cards]
					List<string> descriptionStrings = new List<string>();
					descriptionStrings.Add(Team.teams.Single(t => t.name == gamePlayer.dealtCard.data.team).description);
					descriptionStrings.Add(gamePlayer.prompt.cohortString);
					nightInput_Description.text = string.Join(" ", descriptionStrings.ToArray());
					foreach (ButtonInfo info in gamePlayer.prompt.buttons) {
						AddLocationButton(info.label, info.locationId, nightInput_ButtonBox);
					}
					night_Selections = new List<int>();
					break;
				case UiScreen.Day_Voting:

					//Create buttons 
			foreach(GamePlayer p in client.gameMaster.players) {
						AddLocationButton(p.name, p.locationId, day_VoteButtonBox);
					}
					AddLocationButton("[No one]", -1, day_VoteButtonBox);

					string descriptionText = "";
					//Write description
					//Dealt role- "You were dealt the werewolf."
					descriptionText += "You were dealt the " + gamePlayer.dealtCard.data.role.ToString() + ". ";
					//Team allegiance- "The werewolf is on the werewolf team"
					descriptionText += "The " + gamePlayer.dealtCard.data.role.ToString() + " is on the " + gamePlayer.dealtCard.data.team + ". ";
					//Nature clarity if relevant- "The minion is a villageperson."
					descriptionText += "The " + gamePlayer.dealtCard.data.role.ToString() + " is a " + gamePlayer.dealtCard.data.nature + ". ";
					//Special win conditions- "If there are no other werewolves, the minion wins if an *other* player dies."
					//Cohort type- "You can see other werewolves."
					//Cohort players- "Allen was dealt the werewolf."
					if (gamePlayer.cohortLocations != null) {
						if (gamePlayer.cohortLocations.Length == 0) {
							descriptionText += "You observed that no one was dealt a " + gamePlayer.dealtCard.data.cohort.ToString() + ". ";
						} else {
							foreach (int locationId in gamePlayer.cohortLocations) {
								//TODO Restore observation logging
								//						descriptionText += "You observed that " + GameController.instance.idsToLocations[locationId].name + " was dealt a " + player.dealtCard.data.cohort.ToString() + ". ";
							}
						}
					}
					//Observation- "You observed center card #2 to be the seer at +2";
					foreach (Observation observation in gamePlayer.observations) {

						//TODO Restore observation
						//				descriptionText += "You observed " + GameController.instance.idsToLocations[observation.locationId].name + " to be the " + 
						//					GameController.instance.gamePiecesById[observation.gamePieceId].name + " at " + player.dealtCard.data.order.ToString();
					}
					day_Description.text = descriptionText;

					//Set timer

					break;
				case UiScreen.Result:
					result_Title.text = gamePlayer.didWin ? "You won!" : "You lost!";
					string descriptionString = "";
					//Current player's identity "You are the werewolf."
					descriptionString += "You are the " + gamePlayer.currentCard.name + ". ";
					//Player(s) that died "Frank and Ellen died."
					//Dying players' identities "Frank was the werewolf. Ellen was the mason."
					//TODO Restore death printing
					//			GamePlayer[] killedPlayers = GameController.instance.players.Where(p => p.killed == true).ToArray();
					//			for(int i = 0; i < killedPlayers.Length; i++) {
					//				descriptionString += killedPlayers[i].name + " the " + killedPlayers[i].currentCard.name + " died with X votes. ";
					//			}
					result_Description.text = descriptionString;
					break;
			}

			if (currentScreen == UiScreen.Uninitialized) {
				foreach (KeyValuePair<UiScreen, GameObject> kp in screenGosByEnum) {
					kp.Value.SetActive(false);
				}
			} else {
				screenGosByEnum[currentScreen].SetActive(false);
			}
			screenGosByEnum[targetScreen].SetActive(true);
			currentScreen = targetScreen;
	}

	private void SubmitNightAction(int[] locationId) {

		Selection selection = new Selection(locationId);
		foreach(Transform button in nightInput_ButtonBox.transform) {
			Destroy(button.gameObject);
		}

		client.SubmitNightAction (selection);
	}

	private void SubmitVote(int locationId) {

		foreach(Transform button in day_VoteButtonBox.transform) {
			Destroy(button.gameObject);
		}
		client.SubmitVote (locationId);
	}

	public void HandleJoinButtonPressed()
	{
		//Set persistent player name
		client.SetName(playerEntry_NameField.text);
		playerName.text = client.playerName;

		//Join game
		client.JoinGame();
		
		SetState(UiScreen.Lobby);
	}

	public void HandlePlayersUpdated(List<string> playerNames) {
			print("Players updated");
			string s = "";
			for (int i = 0; i < playerNames.Count; i++) {
				s += playerNames[i];
				if (i < playerNames.Count - 1) {
					s += "\n";
				}
			}
			lobby_playersLabel.text = s;
		}

	public void HandleStartGameCommand() {
		lobby_startButton.interactable = false;
		client.BeginGame ();
	}

	public void SetGamePlayers() {
		gamePlayer = client.gameMaster.players.Single (gp => gp.clientId == client.selfClientId);
	}
}
