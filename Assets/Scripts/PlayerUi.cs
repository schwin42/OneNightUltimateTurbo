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
		Night_InputControl = 0,
//		Night_InputWait = 1,
//		Night_ObservationConfirm = 2,
//		Night_ObservationWait = 3,
		Day_Voting = 4,
		Result = 5,

	}

	public static bool uiEnabled = true;

	public static List<PlayerUi> playerUis;

	public static void Initialize(List<Player> players) {
		if(!uiEnabled) return;
		playerUis = GameObject.FindObjectsOfType<PlayerUi>().ToList();
		for(int i = 0; i < players.Count; i++) {
			if (playerUis.Count > i) {
				playerUis [i].Initialize (players [i]);
			}
		}
	}

	public static void WriteRoleToTitle() {
		if(!uiEnabled) return;
		foreach(PlayerUi playerUi in playerUis) {
			playerUi.Instance_WriteRoleToTitle();
		}
	}

	public static void SetState(UiScreen screen) {
		if(!uiEnabled) return;
		foreach(PlayerUi playerUi in playerUis) {
			playerUi.Instance_SetState(screen);
		}
	}

	private UiScreen currentScreen = UiScreen.Uninitialized;

	private Dictionary<UiScreen, GameObject> screenGosByEnum = new Dictionary<UiScreen, GameObject>();

	private Player player;

	Text playerName;

	//Night input screen
	Text nightInput_Title;
	Text nightInput_Description;
	Transform nightInput_ButtonBox;
	List<int> night_Selections;

	//Day voting
	Transform day_VoteButtonBox;
	Text day_Description;
	Text day_Timer;

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

	private void Initialize(Player player) {
		playerName = transform.Find("PlayerName").GetComponent<Text>();

		//Night_InputControl
		nightInput_Title = transform.Find("Night_InputControl/Title").GetComponent<Text>();
		nightInput_Description = transform.Find("Night_InputControl/Description").GetComponent<Text>();

		nightInput_ButtonBox = transform.Find("Night_InputControl/InputPanel/Grid").transform;

		//Day_Voting
		day_VoteButtonBox = transform.Find("Day_Voting/VotePanel/Grid/");
		day_Description = transform.Find("Day_Voting/Description").GetComponent<Text>();
		day_Timer = transform.Find("Day_Voting/Timer").GetComponent<Text>();

		//Result
		result_Title = transform.Find("Result/Title").GetComponent<Text>();
		result_Description = transform.Find("Result/Description").GetComponent<Text>();

		this.player = player;
		playerName.text = player.name;
	}

	private void Instance_WriteRoleToTitle() {
		nightInput_Title.text = "You are the " + player.dealtCard.data.role.ToString() + " " + player.dealtCard.data.order.ToString();
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
		case OptionsSet.May_TwoOtherPlayers:
				night_Selections.Add(locationId);
				if(night_Selections.Count > 1) {
					SubmitNightAction(night_Selections.ToArray());
				}
			break;
		default:
			Debug.LogError("Unhandled options set: " + player.prompt.options);
			break;
		}
		} else if(currentScreen == UiScreen.Day_Voting) {
			SubmitVote(locationId);
		}
	}

	private void Instance_SetState(UiScreen targetScreen) {
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
			descriptionStrings.Add(Team.teams.Single(t => t.name == player.dealtCard.data.team).description);
			descriptionStrings.Add(player.prompt.cohortString);
			nightInput_Description.text = string.Join(" ", descriptionStrings.ToArray());
			foreach(ButtonInfo info in player.prompt.buttons) {
				AddLocationButton(info.label, info.locationId, nightInput_ButtonBox);
			}
			night_Selections = new List<int>();
			break;
		case UiScreen.Day_Voting:

			//Create buttons
//			print("adding " + GameController.instance.players.Count + " buttons");
			foreach(Player p in GameController.instance.players) {
				if(p == player) continue;
				AddLocationButton(p.name, p.locationId, day_VoteButtonBox);
			}
			AddLocationButton("[No one]", -1, day_VoteButtonBox);

			string descriptionText = "";
			//Write description
			//Dealt role- "You were dealt the werewolf."
			descriptionText += "You were dealt the " + player.dealtCard.data.role.ToString() + ". ";
			//Team allegiance- "The werewolf is on the werewolf team"
			descriptionText += "The " + player.dealtCard.data.role.ToString() + " is on the " + player.dealtCard.data.team + ". ";
			//Nature clarity if relevant- "The minion is a villageperson."
			descriptionText += "The " + player.dealtCard.data.role.ToString() + " is a " + player.dealtCard.data.nature + ". ";
			//Special win conditions- "If there are no other werewolves, the minion wins if an *other* player dies."
			//Cohort type- "You can see other werewolves."
			//Cohort players- "Allen was dealt the werewolf."
			if(player.cohortLocations != null) {
				if(player.cohortLocations.Length == 0) {
					descriptionText += "You observed that no one was dealt a " + player.dealtCard.data.cohort.ToString() + ". ";
				} else {
					foreach(int locationId in player.cohortLocations) {
						descriptionText += "You observed that " + GameController.instance.idsToLocations[locationId].name + " was dealt a " + player.dealtCard.data.cohort.ToString() + ". ";
					}
				}
			}
			//Observation- "You observed center card #2 to be the seer at +2";
			foreach(Observation observation in player.observations) {
				descriptionText += "You observed " + GameController.instance.idsToLocations[observation.locationId].name + " to be the " + 
					GameController.instance.gamePiecesById[observation.gamePieceId].name + " at " + player.dealtCard.data.order.ToString();
			}
			day_Description.text = descriptionText;

			//Set timer

			break;
		case UiScreen.Result:
			result_Title.text = player.didWin ? "You won!" : "You lost!";
			string descriptionString = "";
			//Current player's identity "You are the werewolf."
			descriptionString += "You are the " + player.currentCard.name + ". ";
			//Player(s) that died "Frank and Ellen died."
			//Dying players' identities "Frank was the werewolf. Ellen was the mason."
			Player[] killedPlayers = GameController.instance.players.Where(p => p.killed == true).ToArray();
			for(int i = 0; i < killedPlayers.Length; i++) {
				descriptionString += killedPlayers[i].name + " the " + killedPlayers[i].currentCard.name + " died with X votes. ";
			}
			result_Description.text = descriptionString;
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
		Selection selection = new Selection(locationId);
		foreach(Transform button in nightInput_ButtonBox.transform) {
			Destroy(button.gameObject);
		}
		GameController.SubmitNightAction(player, selection);
	}

	private void SubmitVote(int locationId) {
		foreach(Transform button in day_VoteButtonBox.transform) {
			Destroy(button.gameObject);
		}
		GameController.SubmitVote(player, locationId);
	}

}
