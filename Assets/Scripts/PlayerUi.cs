﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;
using System.Linq;

public class PlayerUi : MonoBehaviour
{

	private static PlayerUi _singleton;

	public static PlayerUi singleton {
		get {
			if (_singleton == null) {
				_singleton = GameObject.FindObjectOfType<PlayerUi> ();
			}
			return _singleton;
		}
	}

	public enum UiScreen
	{
		Uninitialized = -1,
		PlayerEntry = 0,
		Night_InputControl = 1,
		Lobby = 2,
		Day_Voting = 3,
		Result = 4,

	}

	//Night State
	private UiScreen currentScreen = UiScreen.Uninitialized;
	private List<int> pendingSelection;
	private List<List<int>> _nightSelections;
	private int lastSelection = -1;
	private List<int> skippableSubactionIndeces;

	//Day State
	float timer; //In seconds
	int currentVote = -2; //Vote by location id, with -1 indicating vote for no one and -2 indicating no vote

	private Dictionary<UiScreen, GameObject> screenGosByEnum = new Dictionary<UiScreen, GameObject> ();

	IClient client;
	GamePlayer gamePlayer;

	Text playerName;

	//Player Entry
	InputField playerEntry_NameField;
	InputField playerEntry_AddressField;
	Button playerEntry_HostButton;
	Button playerEntry_JoinButton;

	//Lobby
	Text lobby_PlayersLabel;
	Button lobby_StartButton;
	Text lobby_AddressLabel;

	//Night input screen
	Text night_Title;
	Text night_Description;
	Transform night_ButtonBox;

	//Day voting
	Transform day_VoteButtonBox;
	Text day_Description;
	Text day_TimeRemaining;
	Text day_DeckDisplay;
	ToggleGroup day_ToggleGroup;

	//Result
	Text result_Title;
	Text result_Description;

	public void Initialize (IClient client)
	{

		this.client = client;

		foreach (UiScreen screen in Enum.GetValues(typeof(UiScreen))) {
			if (screen == UiScreen.Uninitialized)
				continue;
			screenGosByEnum [screen] = transform.Find (screen.ToString ()).gameObject;
		}

		playerName = transform.Find ("PlayerName").GetComponent<Text> ();

		//PlayerEntry
		playerEntry_NameField = transform.Find ("PlayerEntry/NameField").GetComponent<InputField> ();
		playerEntry_AddressField = transform.Find ("PlayerEntry/AddressField").GetComponent<InputField> ();
		playerEntry_HostButton = transform.Find ("PlayerEntry/HostButton").GetComponent<Button> ();
		playerEntry_JoinButton = transform.Find ("PlayerEntry/JoinButton").GetComponent<Button> ();

		//Lobby
		lobby_PlayersLabel = transform.Find ("Lobby/Description").GetComponent<Text> ();
		lobby_StartButton = transform.Find ("Lobby/StartButton").GetComponent<Button> ();
		lobby_AddressLabel = transform.Find ("Lobby/Address").GetComponent<Text> ();

		//Night_InputControl
		night_Title = transform.Find ("Night_InputControl/Title").GetComponent<Text> ();
		night_Description = transform.Find ("Night_InputControl/Description").GetComponent<Text> ();
		night_ButtonBox = transform.Find ("Night_InputControl/Grid").transform;

		//Day_Voting
		day_VoteButtonBox = transform.Find ("Day_Voting/Panel/Grid/");
		day_Description = transform.Find ("Day_Voting/Description").GetComponent<Text> ();
		day_TimeRemaining = transform.Find("Day_Voting/TimeRemaining").GetComponent<Text>();
		day_DeckDisplay = transform.Find ("Day_Voting/Panel/DeckDisplay/Text").GetComponent<Text> ();
		day_ToggleGroup = day_VoteButtonBox.GetComponent<ToggleGroup> ();

		//Result
		result_Title = transform.Find ("Result/Title").GetComponent<Text> ();
		result_Description = transform.Find ("Result/Description").GetComponent<Text> ();

		SetState (UiScreen.PlayerEntry);

	}

	void Update() {
		if (currentScreen == UiScreen.Day_Voting) {
			timer -= Time.deltaTime;
			day_TimeRemaining.text = GetTimerText (timer);
			if (timer <= 0 && currentVote == -2) {
				SubmitVote (-1);
			}
		}
	}

	public void WriteRoleToTitle ()
	{
		night_Title.text = "You are the " + gamePlayer.dealtCard.data.role.ToString () + " " + gamePlayer.dealtCard.data.order.ToString ();
	}

	private void AddActionButton(string label, int selectionId) {
		GameObject go = Instantiate (PrefabResource.instance.hiddenActionButton) as GameObject;
		go.transform.SetParent (night_ButtonBox, false);
		go.GetComponentInChildren<Text> ().text = label;
		OnuButton onuButton = go.GetComponent<OnuButton> ();
		onuButton.Initialize (this, selectionId);
	}

	private void AddVoteButton(string label, int selectionId) {
		GameObject go = Instantiate (PrefabResource.instance.voteButton) as GameObject;
		go.transform.SetParent (day_VoteButtonBox, false);
		go.GetComponentInChildren<Text> ().text = label;
		OnuToggle onuToggle = go.GetComponent<OnuToggle> ();
		onuToggle.Initialize (this, day_ToggleGroup, selectionId, -2); //Negative two indicates no selection
	}

	public void HandleButtonClick (Button button, int selection)
	{
		button.interactable = false;
		if (selection == -2) { //Didn't even have an action. Use existing derived selection
			CompleteNightAction (_nightSelections);
		} else if (selection == -1) {
			_nightSelections = new List<List<int>> ();
			for (int i = 0; i < gamePlayer.prompt.hiddenAction.Count; i++) {
				_nightSelections.Add (new List<int> { -1 });
			}
			CompleteNightAction (_nightSelections);
		} else {
			pendingSelection.Add (selection);
			lastSelection = selection;
			TryResolveSelection ();
		}
	}

	public void HandleToggleChange(int selection) {
		currentVote = selection;
		SubmitVote (selection);
	}

	private bool TryResolveSubActionSelection (List<SelectableObjectType> patients, List<int> pendingSelection, out List<int> subActionSelection)
	{
		subActionSelection = new List<int> ();
		for (int i = 0; i < patients.Count; i++) {
			switch (patients [i]) {
				case SelectableObjectType.LastTarget:
					subActionSelection.Add (lastSelection);
					break;
				case SelectableObjectType.TargetCenterCard:
				case SelectableObjectType.TargetAnyPlayer:
				case SelectableObjectType.TargetOtherPlayer:
					if (pendingSelection.Count > 0) {
						subActionSelection.Add (pendingSelection [0]);
						pendingSelection.RemoveAt (0);	
					} else {
						Debug.Log ("Waiting for input...");
						return false;
					}
					break;
				case SelectableObjectType.TargetFork:
					if (pendingSelection.Count > 0) {
						subActionSelection.Add (pendingSelection [0]);
						skippableSubactionIndeces.Add (pendingSelection [0]);
						pendingSelection.RemoveAt (0);	
					} else {
						Debug.Log ("Waiting for input...");
						return false;
					}
					break;
				case SelectableObjectType.Self:
					subActionSelection.Add (gamePlayer.locationId);
					break;
				default:
					Debug.LogError ("Unexpected target type: " + patients [i].ToString ());
					break;
			}
		}
		return true;
	}

	private void TryResolveSelection ()
	{
		//Iterate over button groups and try to fill in selections.
		for (int i = 0; i < gamePlayer.prompt.hiddenAction.Count; i++) {

			if (_nightSelections.Count > i)
				continue;
			if (skippableSubactionIndeces.Contains (i)) {
				_nightSelections.Add (new List<int> { -1 });
				continue;
			}
			List<int> subActionSelection;
			if (TryResolveSubActionSelection (gamePlayer.prompt.hiddenAction [i].targets, pendingSelection.ToList (), out subActionSelection)) {
				pendingSelection = new List<int> ();
				_nightSelections.Add (subActionSelection);
				continue;
			} else {
				//create buttons and wait for input
				ClearBox (night_ButtonBox);
				foreach (ButtonInfo info in gamePlayer.prompt.buttonGroupsBySubactionIndex[i]) {
					AddActionButton (info.label, info.locationId);
				}
				return;
			}
		}

		//If selections are resolved, check lastSelection to see if player chose anything.
		if (lastSelection == -1) {
			//If not, give ready button which will in term submit full hidden action
			ClearBox (night_ButtonBox);
			AddActionButton ("Ready", -2);
		} else {
			//If so, submit full hidden action
			CompleteNightAction (_nightSelections);
		}
	}

	private void ClearBox (Transform box)
	{
		foreach (Transform child in box) {
			Destroy (child.gameObject);
		}
	}

	public void SetState (UiScreen targetScreen)
	{
		if (targetScreen == currentScreen)
			return;

		switch (targetScreen) {
			case UiScreen.PlayerEntry:
				playerEntry_HostButton.interactable = true;
				playerEntry_JoinButton.interactable = true;
				break;
			case UiScreen.Lobby:

				AsymClient asymClient = client as AsymClient;
				if (asymClient != null) {
					if (asymClient.localServer != null) {
						print ("is host, ip address: " + Network.player.ipAddress);
						lobby_AddressLabel.text = Network.player.ipAddress;
					} else {
						print ("is client, network address: " + asymClient.client.connection.address);
						lobby_AddressLabel.text = asymClient.client.connection.address;
					}
				}
				break;
			case UiScreen.Night_InputControl:
					//Team allegiance- You are on the werewolf team.
					//Nature clarity if relevant- You are a villageperson.
					//Special win conditions- If there are no other werewolves, you win if an *other* player dies.
					//Cohort type- You can see other werewolves.
					//Cohort players- Allen is a wersewolf.
					//Location selection- You may look at the card of another player or two cards from the center.
					//Selection controls- [Buttons for the three center cards]
				List<string> descriptionStrings = new List<string> ();
				descriptionStrings.Add (Team.teams.Single (t => t.name == gamePlayer.dealtCard.data.team).description);
				descriptionStrings.Add (gamePlayer.prompt.promptText);
				night_Description.text = string.Join (" ", descriptionStrings.ToArray ());

				_nightSelections = new List<List<int>> ();
				pendingSelection = new List<int> ();
				skippableSubactionIndeces = new List<int> ();
				TryResolveSelection ();
				break;
			case UiScreen.Day_Voting:

					//Create buttons 
				foreach (GamePlayer p in client.Gm.players) {
					AddVoteButton (p.name, p.locationId);
				}
				AddVoteButton ("[No one]", -1);

				//Set initial time remaining
				timer = client.Gm.gameSettings.gameTimer;
				day_TimeRemaining.text = GetTimerText (timer);
				currentVote = -2;

				string descriptionText = "";
					//Write description
					//Dealt role- "You were dealt the werewolf."
				descriptionText += "You were dealt the " + gamePlayer.dealtCard.data.role.ToString () + ". ";
					//Team allegiance- "The werewolf is on the werewolf team"
				descriptionText += "The " + gamePlayer.dealtCard.data.role.ToString () + " is on the " + gamePlayer.dealtCard.data.team.ToString () + ". ";
					//Nature clarity if relevant- "The minion is a villageperson."
				descriptionText += "The " + gamePlayer.dealtCard.data.role.ToString () + " is a " + gamePlayer.dealtCard.data.nature + ". ";
					//Special win conditions- "If there are no other werewolves, the minion wins if an *other* player dies."
					//Cohort type- "You can see other werewolves."
					//Cohort players- "Allen was dealt the werewolf."
				if (gamePlayer.prompt.cohorts != null) {
					if (gamePlayer.prompt.cohorts.Length == 0) {
						descriptionText += "You observed that no one was dealt a " + gamePlayer.dealtCard.data.cohort.ToString () + ". ";
					} else {
						foreach (int locationId in gamePlayer.prompt.cohorts) {
							descriptionText += "You observed that " + client.Gm.locationsById [locationId].name + " was dealt a " + gamePlayer.dealtCard.data.cohort.ToString () + ". ";
						}
					}
				}
				//Observation- "You observed center card #2 to be the seer at +2";
				foreach (Observation observation in gamePlayer.observations) {
					descriptionText += "You observed " + GetPlayerName(observation.locationId) + " to be the " +
					client.Gm.gamePiecesById [observation.gamePieceId].name + " at " + gamePlayer.dealtCard.data.order.ToString () + ".";
				}

				//Night selection reminder- "You swapped cards between Jimmy and yourself"
				List<int> skippableIndeces = new List<int>();
				for (int i = 0; i < gamePlayer.dealtCard.data.hiddenAction.Count; i++) {
					SubAction subAction = gamePlayer.dealtCard.data.hiddenAction [i];
					switch (subAction.actionType) {
						case ActionType.ChooseFork:
							skippableIndeces.Add (_nightSelections [i][0]);
							break;
						case ActionType.SwapTwo:
							if (_nightSelections [i][0] == -1) {
								descriptionText += "You chose not to swap cards.";
							} else {
								descriptionText += "You swapped cards between " + GetPlayerName (_nightSelections [i] [0]) + " and " + GetPlayerName (_nightSelections [i] [1]) + " at " + gamePlayer.dealtCard.data.order.ToString() + ".";
							}
							break;
						case ActionType.ViewOne:
						case ActionType.ViewTwo:
							//Observations already handled, do nothing.
							break;
						default:
							Debug.LogError ("Unhandled subaction type: " + subAction);
							break;
					}
				}

				day_Description.text = descriptionText;

			//Set deck display
				List<Role> randomDeckList = client.Gm.gameSettings.deckList.OrderBy (x => UnityEngine.Random.value).ToList ();
				string deckString = "";
				for (int i = 0; i < randomDeckList.Count; i++) {
				
					if (i != 0) {
						deckString += "\n";
					}
					deckString += randomDeckList [i].ToString ();
				}
				day_DeckDisplay.text = deckString;
				break;
			case UiScreen.Result:
				result_Title.text = gamePlayer.didWin ? "You won!" : "You lost!";
				string descriptionString = "";
					//Current player's identity "You are the werewolf."
				descriptionString += "You are the " + gamePlayer.currentCard.name + ". ";
					//Player(s) that died "Frank and Ellen died."
					//Dying players' identities "Frank was the werewolf. Ellen was the mason."
				GamePlayer[] killedPlayers = client.Gm.players.Where (p => p.killed == true).ToArray ();
				for (int i = 0; i < killedPlayers.Length; i++) {
					descriptionString += killedPlayers [i].name + " the " + killedPlayers [i].currentCard.name + " died with " + client.Gm.players.Count (gp => gp.votedLocation == killedPlayers [i].locationId) + " votes. ";
				}
				if (killedPlayers.Length == 0) {
					descriptionString += "No one received enough votes to be killed.";
				}
				result_Description.text = descriptionString;
				break;
		}

		if (currentScreen == UiScreen.Uninitialized) {
			foreach (KeyValuePair<UiScreen, GameObject> kp in screenGosByEnum) {
				kp.Value.SetActive (false);
			}
		} else {
			screenGosByEnum [currentScreen].SetActive (false);
		}
		screenGosByEnum [targetScreen].SetActive (true);
		currentScreen = targetScreen;
	}

	private void CompleteNightAction (List<List<int>> selection)
	{
		ClearBox (night_ButtonBox);

		client.SubmitNightAction (selection.Select (a => a.ToArray ()).ToArray ());
	}

	private void SubmitVote (int locationId) {
		currentVote = locationId;
		client.SubmitVote (locationId);
	}

	public void HandleJoinButtonPressed () {

		//Set persistent player name
		client.PlayerName = playerEntry_NameField.text;
		playerName.text = client.PlayerName;

		playerEntry_HostButton.interactable = false;
		playerEntry_JoinButton.interactable = false;

		//Join game
		AsymClient asymClient = client as AsymClient;
		if (asymClient != null) {
			client.JoinSession (playerEntry_AddressField.text);
		} else {
			client.JoinSession ();
		}
	}

	public void HandleHostButtonPressed ()
	{
		AsymClient asymClient = client as AsymClient;
		if (asymClient == null) {
			Debug.LogError ("Host button pressed outside of asym client");
			return;
		}

		client.PlayerName = playerEntry_NameField.text;
		playerName.text = client.PlayerName;

		//Disable button
		playerEntry_HostButton.interactable = false;
		playerEntry_JoinButton.interactable = false;

		asymClient.HostSession ();
	}

	public void HandlePlayersUpdated (List<string> playerNames)
	{
		string s = "";
		for (int i = 0; i < playerNames.Count; i++) {
			s += playerNames [i];
			if (i < playerNames.Count - 1) {
				s += "\n";
			}
		}
		lobby_PlayersLabel.text = s;
	}

	public void HandleStartGameCommand ()
	{
		lobby_StartButton.interactable = false;
		client.BeginGame ();
	}

	public void SetGamePlayers ()
	{
		gamePlayer = client.Gm.players.Single (gp => gp.clientId == client.ClientId);
	}

	public void HandleHostStarted (string hostPlayerName)
	{
		lobby_PlayersLabel.text = hostPlayerName;
		SetState (UiScreen.Lobby);
	}

	public void HandleClientJoined (string clientName)
	{
		lobby_PlayersLabel.text = clientName;
		SetState (UiScreen.Lobby);
	}

	private string GetPlayerName(int locationId) {
		return locationId == gamePlayer.locationId ? "yourself" : client.Gm.locationsById [locationId].name;
	}

	private string GetTimerText(float seconds) {
		if (seconds < 0.0f) {
			return "00\"00'000";
		} else {
			TimeSpan t = TimeSpan.FromSeconds( seconds );
			return string.Format("{0:D2}\"{1:D2}'{2:D3}",  
				t.Minutes, 
				t.Seconds, 
				t.Milliseconds);
		}
	}
}
