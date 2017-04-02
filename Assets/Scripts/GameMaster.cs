using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class GameMaster {

	public enum GamePhase {
		Pregame = 0, //Actions: Player entry, select roles
		Night = 1, //Actions: Take night action
//		Night_Reveal = 2, //Actions: Confirm night reveal
		Day = 4, //Actions: Manipulate tokens, vote for players
		Result = 5, //Start new game, return to lobby
	}

	public Role[] deckBlueprint;

	public GameMaster () {
	
		locationsById = new List<ILocation>();
		gamePiecesById = new List<IGamePiece>();
	} 

	public GamePhase currentPhase;

	//The deck will be selected/ randomly generated before game start
	public List<RealCard> gameDeck;

	//Game state
	public float gameId;
	public List<GamePlayer> players;
	public List<CenterCardSlot> centerCards;

	//Bookkeeping
//	List<PlayerUi> playerUis;
	List<GamePlayer> playersAwaitingResponseFrom;
	public List<IGamePiece> gamePiecesById;
	public List<ILocation> locationsById;

	public void StartGame(Dictionary<int, string> connectedNamesByClientId, Role[] deckList, bool randomizeDeck, float randomSeed = -1.0F) { //All games run in parallel, so these parameters must be identical across clients


		gameDeck = new List<RealCard>();
		foreach(Role role in deckList) {
			gameDeck.Add(new RealCard(this, role));
		}

		if(gameDeck.Count != connectedNamesByClientId.Count + 3) {
			Debug.LogError("Invalid configuration: there are not exactly three more cards than players: players = " + connectedNamesByClientId.Count + 
				", deck = " + gameDeck.Count);
			return;
		}

		//Create players
		players = new List<GamePlayer>();
		foreach(KeyValuePair<int, string> kp in connectedNamesByClientId) {
			players.Add(new GamePlayer(this, kp.Key, kp.Value));
		}

//		for(int i = 0; i < playerNames.Length; i++) {
//			GamePlayer player = new GamePlayer(playerNames[i]);
//			playersByClientId.Add(connector.conn, player);
//		}

		PlayerUi.Initialize(players);

		//Shuffle cards
		if(randomizeDeck) {
			gameDeck = gameDeck.OrderBy( x => randomSeed ).ToList();
		}

		//Deal cards
		foreach(GamePlayer player in players) {
			player.ReceiveDealtCard(PullFirstCardFromDeck());
		}
		PlayerUi.WriteRoleToTitle();

		centerCards = new List<CenterCardSlot>();
		for(int i = 0; i < 3; i++) {
			centerCards.Add(new CenterCardSlot(this, i, PullFirstCardFromDeck()));
		}
		if(gameDeck.Count != 0) {
			Debug.LogError("Deal left cards remaining in deck");
			return;
		}

		//Print player cards
		foreach(GamePlayer player in players) {
			Debug.Log(player.name + " is the " + player.dealtCard.data.role.ToString() + " " + player.dealtCard.data.order.ToString());
		}
		foreach(CenterCardSlot slot in centerCards) {
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
				player.prompt = new RealizedPrompt(player.locationId, players, centerCards); //Player and center card state is passed to give prompt concrete id choices
			}

			PlayerUi.SetState(PlayerUi.UiScreen.Night_InputControl);

			//Wait for responses
			playersAwaitingResponseFrom = new List<GamePlayer>(players);

			break;
		case GamePhase.Day:
			ExecuteNightActionsInOrder();

			//Reveal information to seer roles
			PlayerUi.SetState(PlayerUi.UiScreen.Day_Voting);

			playersAwaitingResponseFrom = new List<GamePlayer>(players);
			break;
		case GamePhase.Result:
			KillPlayers();
			DetermineWinners();
			PlayerUi.SetState(PlayerUi.UiScreen.Result);
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
			Debug.Log("Most votes:" + locationsById[votees[0].player].name + ", " + votees[0].count);

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
		if(requirement is NatureWinRequirement) {
			return players.Where(p => p.currentCard.data.nature == ((NatureWinRequirement)requirement).nature).ToList();
		} else if (requirement is RoleWinRequirement) {
			return players.Where(p => p.currentCard.data.role == ((RoleWinRequirement)requirement).role).ToList();
		} else if (requirement is RelationWinRequirement) {
			RelationWinRequirement relationRequirement = requirement as RelationWinRequirement;
			if(relationRequirement.relation == Relation.Self) {
			return players.Where(p => p.locationId == evaluatedPlayer.locationId).ToList();
			} else {
				Debug.LogError("Unhandled relation: " + relationRequirement.relation);
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
			for (int j = 0; j < actingPlayer.dealtCard.data.nightActions.Count; j++) {
				HiddenAction hiddenAction = actingPlayer.dealtCard.data.nightActions[j];
				if(hiddenAction.actionType == ActionType.ViewOne) { //Lone werewolf, robber 2nd, insomniac, mystic wolf, apprentice seer
					int targetLocationId = hiddenAction.targets[0] == TargetType.Self ? actingPlayer.locationId : 
						actingPlayer.nightLocationSelection.locationIds[((int)hiddenAction.targets[0])];
					if(targetLocationId == -1) {
						//TODO Notify "You chose not to view a card."
					} else {
						actingPlayer.observations.Add(new Observation(targetLocationId, locationsById[targetLocationId].currentCard.gamePieceId));
					}
				} else if(hiddenAction.actionType == ActionType.SwapTwo) { //Robber 1st, troublemaker, drunk
					//Get cards to swap
					List<int> targetLocationIds = GetLocationIdsFromTargetInfo(actingPlayer.locationId, hiddenAction.targets, actingPlayer.nightLocationSelection.locationIds.ToList());
					ILocation firstTargetLocation = locationsById[targetLocationIds[0]];
					ILocation secondTargetLocation = locationsById[targetLocationIds[1]];
					RealCard firstTargetCard = firstTargetLocation.currentCard;
					RealCard secondTargetCard = secondTargetLocation.currentCard;
					firstTargetLocation.currentCard = secondTargetCard;
					secondTargetLocation.currentCard = firstTargetCard;


//				} else if(actingPlayer.dealtCard.data.nightActions[j].actionType == ActionType.ViewUpToTwo) { //Seer

				} else {
					Debug.LogError("Unhandled action type: " + hiddenAction.actionType);
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
		return gamePiecesById.Count - 1;
	}

	public int RegisterLocation(ILocation location) {
		locationsById.Add(location);
		return locationsById.Count - 1;
	}


	public void ReceiveDirective(GamePayload payload) {
		if(payload is NightActionPayload) {
			NightActionPayload nightAction = (NightActionPayload)payload;
			SubmitNightAction(players.Single(gp => gp.clientId == payload.sourceClientId), nightAction.selection);
		} else if(payload is VotePayload) {
			VotePayload vote = (VotePayload)payload;
			SubmitVote(players.Single(gp => gp.clientId == payload.sourceClientId), vote.voteeLocationId);
		} else {
			Debug.LogError("Unexpected type of game payload: " + payload.ToString());
		}
	}

	public void SubmitNightAction(GamePlayer player, Selection selection) {
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

	public void SubmitVote(GamePlayer player, int locationId) {
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

	private List<int> GetLocationIdsFromTargetInfo(int playerId, List<TargetType> targetTypes, List<int> specifiedTargets) {
		List<int> locationsIds = new List<int>();
		for(int i = 0; i < targetTypes.Count; i++) {
			if(targetTypes[i] == TargetType.Self) {
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
	public OptionsSet options;
	public List<ButtonInfo> buttons = new List<ButtonInfo>();

	public RealizedPrompt(int selfLocationId, List<GamePlayer> players, List<CenterCardSlot> centerCards) {
		//Evalutate cohort and realize strings
		GamePlayer self = players.Single(gp => gp.locationId == selfLocationId);
		if(self.dealtCard.data.cohort.isEmpty) {
			if(self.dealtCard.data.prompt != null) {
				cohortString = self.dealtCard.data.prompt.explanation;
				options = self.dealtCard.data.prompt.options;
			}
		} else {
			List<GamePlayer> cohorts = self.dealtCard.data.cohort.FilterPlayersByDealtCard(
				players.Where(p => p.locationId != self.locationId).ToList()).ToList();
			self.cohortLocations = cohorts.Select(p => p.locationId).ToArray();
			if(cohorts.Count == 0) {
				cohortString = self.dealtCard.data.prompt.explanation;
				options = self.dealtCard.data.prompt.options;
			} else {
				for(int i = 0; i < cohorts.Count; i++) {
					if( i != 0) {
						cohortString += " ";
					}
					cohortString += string.Format(self.dealtCard.data.promptIfCohort.explanation, cohorts[i].name);
				}
				options = self.dealtCard.data.promptIfCohort.options;
			}
		}

		switch(options) {
		case OptionsSet.None:
			buttons.Add(new ButtonInfo(-1, "Ready"));
			break;
		case OptionsSet.May_CenterCard: //Apprentice seer, lone werewolf
			for (int i = 0; i < centerCards.Count; i++) {
				CenterCardSlot slot = centerCards[i];
				buttons.Add(new ButtonInfo(slot.locationId, "Center Card #" + (i + 1).ToString()));
			}
			buttons.Add(new ButtonInfo(-1, "Pass"));
			break;
		case OptionsSet.Must_CenterCard: //Drunk and copycat
			for (int i = 0; i < centerCards.Count; i++) {
				CenterCardSlot slot = centerCards[i];
				buttons.Add(new ButtonInfo(slot.locationId, "Center Card #" + (i + 1).ToString()));
			}
			break;
		case OptionsSet.May_OtherPlayer: //Robber, mystic wolf
			Debug.Log("Not implemented: may other player");
			buttons.Add(new ButtonInfo(-1, "Ready"));
			break;
		case OptionsSet.May_TwoOtherPlayers:
			for(int i = 0; i < players.Count; i++) {
				GamePlayer p = players[i];
				if(p.locationId == self.locationId) continue;
				buttons.Add(new ButtonInfo(p.locationId, p.name));
			}
			buttons.Add(new ButtonInfo(-1, "Pass"));
			break;
		default:
			Debug.LogError("Unhandled options set: " + options);
			break;
		}

//		this.options = player.dealtCard.prom prompt.options;

		//Derive cohort string from abstract prompt and game state
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
			return data.winRequirements != null && data.winRequirements.Length != 0 ? data.winRequirements : Team.teams.Single(t => t.name == data.team).winRequirements;
		}
	}

	//any viewed roles (e.g., Doppleganger, paranormal investigator, copycat)

	public CardData data;

	public RealCard(GameMaster gameMaster, Role role) {
		data = GameData.instance.cardData.Single (cd => cd.role == role);
		_gamePieceId = gameMaster.RegisterGamePiece(this);
		Debug.Log("Registered " + role.ToString() + " as gamePieceId = " + gamePieceId);
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
public class Selection {
	public bool isEmpty = true;
	public int[] locationIds;

	public static Selection None () {
		return new Selection();
	}

	private Selection() { }

	public Selection(params int[] locationIds) {
		this.isEmpty = true;
		this.locationIds = locationIds;
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