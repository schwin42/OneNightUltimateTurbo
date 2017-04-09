using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class GameMaster {

	public enum GamePhase {
		Uninitialized = 0, //Actions: Player entry, select roles
		Night = 1, //Actions: Take night action
//		Night_Reveal = 2, //Actions: Confirm night reveal
		Day = 4, //Actions: Manipulate tokens, vote for players
		Result = 5, //Start new game, return to lobby
	}

	public Role[] deckBlueprint;

	public GameMaster()
	{
		locationsById = new List<ILocation>();
		gamePiecesById = new List<IGamePiece>();
	}

	public GameMaster (PlayerUi ui) {
		this.ui = ui;
		locationsById = new List<ILocation>();
		gamePiecesById = new List<IGamePiece>();
	} 

	public GamePhase currentPhase;

	//The deck will be selected/ randomly generated before game start
	public List<RealCard> gameDeck;

	//Configuration
	PlayerUi ui;

	//Game state
	public float gameId;
	public List<GamePlayer> players;
	public List<CenterCardSlot> centerSlots;

	//Bookkeeping
	List<GamePlayer> playersAwaitingResponseFrom;
	public List<IGamePiece> gamePiecesById;
	public List<ILocation> locationsById;

	public void StartGame(List<string> playersByClientId, Role[] deckList, bool randomizeDeck, int randomSeed = -1) { //All games run in parallel, so these parameters must be identical across clients
		if (currentPhase != GamePhase.Uninitialized) {
			Debug.LogWarning ("Start game called with game already in progress, aborting.");
			return;
		}

		//Instantiate deck
		gameDeck = new List<RealCard>();
		foreach(Role role in deckList) {
			gameDeck.Add(new RealCard(this, role));
		}

		//Prune deck
//		gameDeck = gameDeck.Take(playersByClientId.Count + 3).ToList();

		//Validate configuration
		if(gameDeck.Count != playersByClientId.Count + 3) {
			Debug.LogError("Invalid configuration: there are not exactly three more cards than players: player names, player ids = " + playersByClientId.Count + ", " + playersByClientId.Count + 
				", deck = " + gameDeck.Count + ", " + deckList.Length);
			return;
		}

		//Create players
		players = new List<GamePlayer>();
		for(int i = 0; i < playersByClientId.Count; i++) {
			players.Add(new GamePlayer(this, i, playersByClientId[i]));
		}

		//Shuffle deck
		if (randomizeDeck) {
			gameDeck = Utility.ShuffleCards (gameDeck, randomSeed);
		}

		string s = "Game deck: ";
		for(int i = 0; i < gameDeck.Count; i++) {
			RealCard card = gameDeck [i];
			s += card.name;
			if (i < gameDeck.Count - 1) {
				s += ", ";
			}
		}
		Debug.Log (s);

		//Deal cards
		foreach(GamePlayer player in players) {
			player.ReceiveDealtCard(PullFirstCardFromDeck());
		}

		if(ui != null) ui.SetGamePlayers ();
		if(ui != null) ui.WriteRoleToTitle ();

		centerSlots = new List<CenterCardSlot>();
		for(int i = 0; i < 3; i++) {
			centerSlots.Add(new CenterCardSlot(this, i, PullFirstCardFromDeck()));
		}
		if(gameDeck.Count != 0) {
			Debug.LogError("Deal left cards remaining in deck");
			return;
		}

		//Print player cards
		foreach(GamePlayer player in players) {
			Debug.Log(player.name + " is the " + player.dealtCard.data.role.ToString() + " " + player.dealtCard.data.order.ToString());
		}
		foreach(CenterCardSlot slot in centerSlots) {
			Debug.Log(slot.currentCard.data.role.ToString() + " is " + slot.name);
		}

		SetPhase(GamePhase.Night);
	}

	private void SetPhase(GamePhase targetPhase) {
		if(targetPhase == currentPhase) return;
		currentPhase = targetPhase;
		Debug.Log("Entering " + targetPhase + " phase.");
		switch(targetPhase) {
		case GamePhase.Night:

			//Prompt players for action and set controls
			foreach(GamePlayer player in players) {
				player.prompt = new RealizedPrompt(player.locationId, players, centerSlots); //Player and center card state is passed to give prompt concrete id choices
			}

			if(ui != null) ui.SetState(PlayerUi.UiScreen.Night_InputControl);

			//Wait for responses
			playersAwaitingResponseFrom = new List<GamePlayer>(players);

			break;
		case GamePhase.Day:
			ExecuteNightActionsInOrder();

				//Reveal information to seer roles
			if(ui != null) ui.SetState(PlayerUi.UiScreen.Day_Voting);

			playersAwaitingResponseFrom = new List<GamePlayer>(players);
			break;
		case GamePhase.Result:
			KillPlayers();
			DetermineWinners();
			if(ui != null) ui.SetState(PlayerUi.UiScreen.Result);
			break;
		}
	}

	public void KillPlayers() {
		//Tally votes
		List<Votee> votees = new List<Votee>();
		foreach(GamePlayer voter in players) {
			if(voter.votedLocation == -1) continue;
			if(votees.Count(v => v.player == voter.votedLocation) > 0) { 
				votees.Single(v => v.player == voter.votedLocation).count ++;
			} else {
				votees.Add(new Votee(voter.votedLocation));
			}
		}

		//Sort by descending votes
		votees = votees.OrderByDescending(v => v.count).ToList();

		for(int i = 0; i < votees.Count; i++ ) {
			Votee votee = votees[i];
			Debug.Log(locationsById[votee.player].name + " received " + votee.count + " votes.");
		}

		//Determine most number of votes
		if(votees.Count > 0) { //If nobody was voted to die, proceed to result evaluation
			int mostVotes = votees[0].count;
			Debug.Log("Most votes:" + locationsById[votees[0].player].name + " with " + votees[0].count);

			//Select votees with most number of votes over one
			List<int> playersToKill = votees.Where(v => v.count == mostVotes && v.count > 1).Select(v => v.player).ToList();

			//Kill all players with the highest number of votes (greater than one)
			foreach(int locationId in playersToKill) {
				GamePlayer playerToKill = players.Single(p => p.locationId == locationId);
				playerToKill.killed = true;
				Debug.Log("Killed " + playerToKill.name);
			}
		}
	}

	public void DetermineWinners() {
		foreach(GamePlayer player in players) {
			player.didWin = EvaluateRequirementRecursive(player, player.currentCard.winRequirements);
			if(player.didWin) { Debug.Log(player.name + " won."); } else {
				Debug.Log(player.name + " lost.");
			}
		}
	}

	private bool EvaluateRequirementRecursive(GamePlayer evaluatedPlayer, WinRequirement[] requirements) {
		//Get requirement relevant players
		foreach(WinRequirement requirement in requirements) {
			bool passed;
			List<GamePlayer> criteriaPlayers = SelectRelevantPlayers(evaluatedPlayer, requirement);
			if(criteriaPlayers.Count == 0) { 
				passed = requirement.fallback == null ? true : EvaluateRequirementRecursive(evaluatedPlayer, requirement.fallback);
			} else {
				bool relevantPlayerDied = criteriaPlayers.Count(p => p.killed) > 0;
				if(requirement.predicate == WinPredicate.MustDie) {
					if(relevantPlayerDied) {
						passed = true;
					} else {
						passed = false;
					}
				} else if(requirement.predicate == WinPredicate.MustNotDie) {
					if(relevantPlayerDied) {
						passed = false;
					} else {
						passed = true;
					}
				} else {
					Debug.LogError("Unexpected predicate: " + requirement.predicate);
					return false;
				}
			}

			if(passed) {
				continue;
			} else {
				return false;
			}
		}
		return true; //If evaluted all requirements without returning false, then return true
	}

	private List<GamePlayer> SelectRelevantPlayers(GamePlayer evaluatedPlayer, WinRequirement requirement) {
		if(requirement.subject.nature != Nature.None) {
			return players.Where(p => p.currentCard.data.nature == requirement.subject.nature).ToList();
		} else if (requirement.subject.role != Role.None) {
			return players.Where(p => p.currentCard.data.role == requirement.subject.role).ToList();
		} else if (requirement.subject.relation != Relation.None) {
			if(requirement.subject.relation == Relation.Self) {
			return players.Where(p => p.locationId == evaluatedPlayer.locationId).ToList();
			} else {
				Debug.LogError("Unhandled relation: " + requirement.subject.relation);
				return null;
			}
		} else {
			Debug.LogError("Unhandled win requirement type.");
			return null;
		}
	}

	public void ExecuteNightActionsInOrder ()
	{
		List<GamePlayer> actingPlayersByTurnOrder = players.Where (p => !p.dealtCard.data.order.isEmpty).OrderBy (p => p.dealtCard.data.order.primary).
			ThenBy (p => p.dealtCard.data.order.secondary).ToList ();
		for (int i = 0; i < actingPlayersByTurnOrder.Count; i++) {
			GamePlayer actingPlayer = actingPlayersByTurnOrder [i];
			List<int> skippableIndeces = new List<int>();
			for (int j = 0; j < actingPlayer.dealtCard.data.hiddenAction.Count; j++) {
				//TODO check if skippable due to fork
				if(skippableIndeces.Contains(j)) continue;
				SubAction subAction = actingPlayer.dealtCard.data.hiddenAction[j];
				if(actingPlayer.nightLocationSelection[j][0] == -1) {
					if(subAction.isMandatory) {
						Debug.LogError("Action is mandatory, but no selection was received.");
					}
					break; //Player chose not to act, end night action processing for this player
				}
				if(subAction.actionType == ActionType.ChooseFork) { //Instead of location ID, selection is chosen fork - 0 or 1
					//TODO Add fork case
					if(actingPlayer.nightLocationSelection[j].Length != 1) {
						Debug.LogError("Unexpected number of subaction selections for ChooseFork: " + actingPlayer.nightLocationSelection[j].Length);
						continue;
					} else {
						skippableIndeces.Add(j + 1 + (1 - actingPlayer.nightLocationSelection[j][0]));
					}
				} else if(subAction.actionType == ActionType.ViewOne) { //Lone werewolf, robber 2nd, insomniac, mystic wolf, apprentice seer

					//Get jth sub action of selection, which should be an array with one location id
					if(actingPlayer.nightLocationSelection[j].Length != 1) {
						Debug.LogError("Unexpected number of subaction selections for ViewOne: " + actingPlayer.nightLocationSelection[j].Length);
						continue;
					}
					int targetLocationId = actingPlayer.nightLocationSelection[j][0];


						actingPlayer.observations.Add(new Observation(targetLocationId, locationsById[targetLocationId].currentCard.gamePieceId));
				} else if(subAction.actionType == ActionType.SwapTwo) { //Robber 1st, troublemaker, drunk
					//Get cards to swap
					int[] targetLocationIds = actingPlayer.nightLocationSelection[j];
//					List<int> targetLocationIds = GetLocationIdsFromTargetInfo(actingPlayer.locationId, hiddenAction.targets, actingPlayer.nightLocationSelection.locationIds.ToList());

						ILocation firstTargetLocation = locationsById[targetLocationIds[0]];
						ILocation secondTargetLocation = locationsById[targetLocationIds[1]];
						RealCard firstTargetCard = firstTargetLocation.currentCard;
						RealCard secondTargetCard = secondTargetLocation.currentCard;
						firstTargetLocation.currentCard = secondTargetCard;
						secondTargetLocation.currentCard = firstTargetCard;

				} else if(subAction.actionType == ActionType.ViewTwo) { //Seer second option
					int[] targetLocationIds = actingPlayer.nightLocationSelection[j];
					actingPlayer.observations.Add(new Observation(targetLocationIds[0], locationsById[targetLocationIds[0]].currentCard.gamePieceId));
					actingPlayer.observations.Add(new Observation(targetLocationIds[1], locationsById[targetLocationIds[1]].currentCard.gamePieceId));
				} else {
					Debug.LogError("Unhandled action type: " + subAction.actionType);
				}
			}
		}
	}

	private RealCard PullFirstCardFromDeck() {
		RealCard card = gameDeck[0];
		gameDeck.Remove(card);
		return card;
	}

	public int RegisterGamePiece(IGamePiece gamePiece) {
		gamePiecesById.Add(gamePiece);
		return (gamePiecesById.Count - 1);
	}

	public int RegisterLocation(ILocation location) {
		locationsById.Add(location);
		return locationsById.Count - 1;
	}


	public void ReceiveDirective(GamePayload payload) {
		if(payload is NightActionPayload) {
			NightActionPayload nightAction = (NightActionPayload)payload;
			ReceiveNightAction(players.Single(gp => gp.clientId == nightAction.sourceClientId), nightAction.selection);
		} else if(payload is VotePayload) {
			VotePayload vote = (VotePayload)payload;
			ReceiveVote(players.Single(gp => gp.clientId == vote.sourceClientId), vote.voteeLocationId);
		} else {
			Debug.LogError("Unexpected type of game payload: " + payload.ToString());
		}
	}

	public void ReceiveNightAction(int sourceClientId, int[][] selection) {
		GamePlayer player = players.Single (gp => gp.clientId == sourceClientId);
		ReceiveNightAction (player, selection);
	}

	public void ReceiveNightAction(GamePlayer player, int[][] selection) {
		if(currentPhase != GamePhase.Night) {
			Debug.LogError("Received night action outside of Night_Input phase");
			return;
		}
		player.nightLocationSelection = selection;
		playersAwaitingResponseFrom.Remove(player);

		if(playersAwaitingResponseFrom.Count == 0) {
			SetPhase(GamePhase.Day);
		}
	}

	public void ReceiveVote(int sourceClientId, int locationId) {
		GamePlayer player = players.Single (gp => gp.clientId == sourceClientId);
		ReceiveVote (player, locationId);
	}

	public void ReceiveVote(GamePlayer player, int locationId) {
		if(currentPhase != GamePhase.Day) {
			Debug.LogError("Received night action outside of Night_Input phase");
			return;
		}

		player.votedLocation = locationId;
		playersAwaitingResponseFrom.Remove(player);

		if(playersAwaitingResponseFrom.Count == 0) {
			SetPhase(GamePhase.Result);
		}
	}

	private List<int> GetLocationIdsFromTargetInfo(int playerId, List<SelectableObjectType> targetTypes, List<int> specifiedTargets) {
		List<int> locationsIds = new List<int>();
		for(int i = 0; i < targetTypes.Count; i++) {
			if(targetTypes[i] == SelectableObjectType.Self) {
				locationsIds.Add(playerId);
			} else {
				locationsIds.Add(specifiedTargets[0]);
				specifiedTargets.RemoveAt(0);

			}
		}
		return locationsIds;
	}
	
}

[System.Serializable]
public class RealizedPrompt {
	public string cohortString = "";
	public List<SubAction> hiddenAction;
	public List<List<ButtonInfo>> buttonGroupsBySubactionIndex = new List<List<ButtonInfo>>();

	public RealizedPrompt(int selfLocationId, List<GamePlayer> players, List<CenterCardSlot> centerCards) {
		//Evalutate cohort and realize strings
		GamePlayer self = players.Single(gp => gp.locationId == selfLocationId);
		if(self.dealtCard.data.cohort.isEmpty) {
			if(self.dealtCard.data.prompt != null) {
				cohortString = self.dealtCard.data.prompt;
				hiddenAction = self.dealtCard.data.hiddenAction;
			}
		} else {
			List<GamePlayer> cohorts = self.dealtCard.data.cohort.FilterPlayersByDealtCard(
				players.Where(p => p.locationId != self.locationId).ToList()).ToList();
			self.cohortLocations = cohorts.Select(p => p.locationId).ToArray();
			if(cohorts.Count == 0) {
				cohortString = self.dealtCard.data.prompt;
				hiddenAction = self.dealtCard.data.hiddenAction;
			} else {
				for(int i = 0; i < cohorts.Count; i++) {
					if( i != 0) {
						cohortString += " ";
					}
					cohortString += string.Format(self.dealtCard.data.promptIfCohort, cohorts[i].name);
				}
				hiddenAction = self.dealtCard.data.hiddenActionIfCohort;
			}
		}

		for(int i = 0; i < hiddenAction.Count; i++) { //For each sub action
			List<ButtonInfo> buttonGroup = new List<ButtonInfo>();
			//Find option targets and generate options set
			for(int j = 0; j < hiddenAction[i].targets.Count; j++) { //For each target type
				if (hiddenAction[i].targets[j] == SelectableObjectType.TargetAnyPlayer) {
					for (int k = 0; k < players.Count; k++) {
						GamePlayer p = players[k];
						buttonGroup.Add(new ButtonInfo(p.locationId, p.name));
					}
					if(!hiddenAction[i].isMandatory) buttonGroup.Add(new ButtonInfo(-1, "Pass"));
					break;
				} else if (hiddenAction[i].targets[j] == SelectableObjectType.TargetOtherPlayer) {
					for (int k = 0; k < players.Count; k++) {
						GamePlayer p = players[k];
						if(p.locationId == self.locationId) continue;
						buttonGroup.Add(new ButtonInfo(p.locationId, p.name));
					}
					if(!hiddenAction[i].isMandatory) buttonGroup.Add(new ButtonInfo(-1, "Pass"));
					break;
				} else if (hiddenAction[i].targets[j] == SelectableObjectType.TargetCenterCard) {
					for (int k = 0; k < centerCards.Count; k++) {
						CenterCardSlot ccs = centerCards[k];
						buttonGroup.Add(new ButtonInfo(ccs.locationId, ccs.name));
					}
					if(!hiddenAction[i].isMandatory) buttonGroup.Add(new ButtonInfo(-1, "Pass"));
					break;
				} else if(hiddenAction[i].targets[j] == SelectableObjectType.TargetFork) {
					for(int k = 0; k < 2; k++) {
						buttonGroup.Add(new ButtonInfo(k, "Option #" + (k + 1).ToString()));
					}
					if(!hiddenAction[i].isMandatory) buttonGroup.Add(new ButtonInfo(-1, "Pass"));
					break;
				}
			}
			buttonGroupsBySubactionIndex.Add(buttonGroup);
		}
	}
}

[System.Serializable]
public struct ButtonInfo {
	public int locationId;
	public string label;
	public ButtonInfo(int ogoId, string label) {
		this.locationId = ogoId;
		this.label = label;
	}
}

[System.Serializable]
public class RealCard : IGamePiece {
	
	private int _gamePieceId;
	public int gamePieceId {
		get {
			return _gamePieceId;
		}
	}
	public string name {
		get {
			return data.role.ToString();
		}
	}

	public WinRequirement[] winRequirements {
		get {
			return data.winRequirement != null ? new WinRequirement[] { data.winRequirement } : Team.teams.Single(t => t.name == data.team).winRequirements;
		}
	}

	//any viewed roles (e.g., Doppleganger, paranormal investigator, copycat)

	public CardData data;

	public RealCard(GameMaster gameMaster, Role role) {
		data = GameData.instance.cardData.Single (cd => cd.role == role);
		_gamePieceId = gameMaster.RegisterGamePiece(this);
//		Debug.Log("Registered " + role.ToString() + " as gamePieceId = " + gamePieceId);
	}
}

[System.Serializable]
public class CenterCardSlot : ILocation {
	private int _locationId;
	public int locationId { get {
			return _locationId;
		}
	}
	public string name { 
		get {
			return "center card #" + (centerCardIndex + 1);
		}
	}
	public int centerCardIndex;
	public RealCard dealtCard;
	private RealCard _currentCard;
	public RealCard currentCard {
		get {
			return _currentCard;
		}
		set {
			_currentCard = value;
		}
	}
	public CenterCardSlot(GameMaster gameMaster, int index, RealCard card) {
		this.centerCardIndex = index;
		this.dealtCard = card;
		this._currentCard = card;
		_locationId = gameMaster.RegisterLocation(this);
		Debug.Log("Registered center card #" + (index + 1) + " as locationId = " + _locationId);
	}
}

[System.Serializable]
public struct Observation {
	public int locationId;
	public int gamePieceId;
	public Observation(int locationId, int gamePieceId) {
		this.locationId = locationId;
		this.gamePieceId = gamePieceId;
	}
}

[System.Serializable]
public class Votee {
	public int player;
	public int count;
	public Votee(int player) {
		this.player = player;
		this.count = 1;
	}
}