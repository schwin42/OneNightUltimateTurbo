using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class GameController : MonoBehaviour {

	public enum GamePhase {
		Pregame = 0, //Actions: Player entry, select roles
		Night_Input = 1, //Actions: Take night action
//		Night_Reveal = 2, //Actions: Confirm night reveal
		Day = 4, //Actions: Manipulate tokens, vote for players
		Result = 5, //Start new game, return to lobby
	}

	private static GameController _instance;
	public static GameController instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<GameController>();
			}
			return _instance;
		}
	}

	public GamePhase currentPhase;

	//The deck will be selected/ randomly generated before game start
	public List<RealCard> deck;

	public string[] playerNames;

	//Game state

	public List<Player> players;
	public List<CenterCardSlot> centerCards;


	//Bookkeeping
	List<PlayerUi> playerUis;
	static Dictionary<Player, PlayerUi> playerUisByPlayer;
	List<Player> playersAwaitingResponseFrom;
	public List<IGamePiece> gamePiecesById = new List<IGamePiece>();
	public List<ILocation> idsToLocations = new List<ILocation>();

	void Start() {
		playerUis = GameObject.FindObjectsOfType<PlayerUi>().ToList();

		StartGame(
			new string[] { "Allen", "Becky", "Chris", "David", "Ellen", "Frank", },
			new Role[] { Role.Werewolf, Role.Werewolf, Role.Mason, Role.Mason, Role.Villager, Role.Villager, Role.Villager, Role.Villager, Role.Villager } 
			);

	}

	public void StartGame(string[] playerNames, Role[] deckList) {
		deck = new List<RealCard>();
		this.playerNames = playerNames;
		foreach(Role role in deckList) {
			deck.Add(new RealCard(role));
		}

		SetPhase(GamePhase.Night_Input);
	}

	private static void SetPhase(GamePhase targetPhase) {
		if(targetPhase == instance.currentPhase) return;
		instance.currentPhase = targetPhase;
		print("Entering " + targetPhase + " phase.");
		switch(targetPhase) {
		case GamePhase.Night_Input:
			if(instance.deck.Count != instance.playerNames.Length + 3) {
				Debug.LogError("Invalid configuration: there are not exactly three more cards than players: players = " + instance.playerNames.Length + 
					", deck = " + instance.deck.Count);
				return;
			}

			//Start game

			//Create players
			playerUisByPlayer = new Dictionary<Player, PlayerUi>();
			instance.players = new List<Player>();
			for(int i = 0; i < instance.playerNames.Length; i++) {
				Player player = new Player(instance.playerNames[i]);
				instance.players.Add(player);
				instance.playerUis[i].Initialize(player);
				playerUisByPlayer.Add(player, instance.playerUis[i]);
			}

			//Shuffle cards
			System.Random rnd = new System.Random();
			instance.deck = instance.deck.OrderBy(item => rnd.Next()).ToList();

			//Deal cards
			foreach(Player player in instance.players) {
				player.ReceiveDealtCard(PullFirstCardFromDeck());
				playerUisByPlayer[player].WriteRoleToTitle();
			}
			instance.centerCards = new List<CenterCardSlot>();
			for(int i = 0; i < 3; i++) {
				instance.centerCards.Add(new CenterCardSlot(i, PullFirstCardFromDeck()));
			}
			if(instance.deck.Count != 0) {
				Debug.LogError("Deal left cards remaining in deck");
				return;
			}

			//Print player cards
			foreach(Player player in instance.players) {
				print(player.name + " is the " + player.dealtCard.role.ToString() + " " + player.dealtCard.order.ToString());
			}
			foreach(CenterCardSlot slot in instance.centerCards) {
				print(slot.currentCard.role.ToString() + " is in the center");
			}

			//Prompt players for action and set controls
			foreach(Player player in instance.players) {
				player.prompt = new RealizedPrompt(player);
				playerUisByPlayer[player].SetState(PlayerUi.UiScreen.Night_InputControl);
			}

			//Wait for responses
			instance.playersAwaitingResponseFrom = new List<Player>(instance.players);

			break;
		case GamePhase.Day:
			instance.ExecuteNightActionsInOrder();

			//Reveal information to seer roles
			foreach(Player player in instance.players) {
				playerUisByPlayer[player].SetState(PlayerUi.UiScreen.Day_Voting);
			}

			instance.playersAwaitingResponseFrom = new List<Player>(instance.players);
			break;
		case GamePhase.Result:
			KillPlayers();
			DetermineWinners();
			foreach(Player displayPlayer in GameController.instance.players) {
				playerUisByPlayer[displayPlayer].SetState(PlayerUi.UiScreen.Result);
			}
			break;
		}
	}

	public static void KillPlayers() {
		//Tally votes
		List<Votee> votees = new List<Votee>();
		foreach(Player voter in instance.players) {
			if(voter.locationIdVote == -1) continue;
			if(votees.Count(v => v.player == voter.locationIdVote) > 0) { 
				votees.Single(v => v.player == voter.locationIdVote).count ++;
			} else {
				votees.Add(new Votee(voter.locationIdVote));
			}
		}

		//Sort by descending votes
		votees = votees.OrderByDescending(v => v.count).ToList();

		for(int i = 0; i < votees.Count; i++ ) {
			Votee votee = votees[i];
			print(instance.idsToLocations[votee.player].name + " received " + votee.count + " votes.");
		}

		//Determine most number of votes
		if(votees.Count > 0) { //If nobody was voted to die, proceed to result evaluation
			int mostVotes = votees[0].count;
			print("Most votes:" + instance.idsToLocations[votees[0].player].name + ", " + votees[0].count);

			//Select votees with most number of votes over one
			List<int> playersToKill = votees.Where(v => v.count == mostVotes && v.count > 1).Select(v => v.player).ToList();

			//Kill all players with the highest number of votes (greater than one)
			foreach(int locationId in playersToKill) {
				Player playerToKill = instance.players.Single(p => p.locationId == locationId);
				playerToKill.killed = true;
				print("Killed " + playerToKill.name);
			}
		}
	}

	public static void DetermineWinners() {
		foreach(Player player in instance.players) {
			player.didWin = EvaluateRequirementRecursive(player, player.currentCard.winRequirements);
			if(player.didWin) { print(player.name + " won."); } else {
				print(player.name + " lost.");
			}
		}
	}

	private static bool EvaluateRequirementRecursive(Player evaluatedPlayer, WinRequirement[] requirements) {
		//Get requirement relevant players
		foreach(WinRequirement requirement in requirements) {
			bool passed;
			List<Player> criteriaPlayers = SelectRelevantPlayers(evaluatedPlayer, requirement);
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

//			//Get role criteria list
//			List<Player> criteriaPlayers = SelectRelevantPlayers(evaluatedPlayer, requirement);
//			if(criteriaPlayers.Count == 0) criteriaPlayers = SelectRelevantPlayers(evaluatedPlayer, requirement.fallback);
//
//			bool relevantPlayerDied = criteriaPlayers.Count(p => p.killed) > 0;
//			if(requirement.predicate == WinPredicate.MustDie) {
//				if(relevantPlayerDied) {
//					continue;
//				} else {
//					return false;
//				}
//			} else if(requirement.predicate == WinPredicate.MustNotDie) {
//				if(relevantPlayerDied) {
//					return false;
//				} else {
//					continue;
//				}
//			}
//		}
//		return true;
//	}

	private static List<Player> SelectRelevantPlayers(Player evaluatedPlayer, WinRequirement requirement) {
		if(requirement is NatureWinRequirement) {
			return instance.players.Where(p => p.currentCard.nature == ((NatureWinRequirement)requirement).nature).ToList();
		} else if (requirement is RoleWinRequirement) {
			return instance.players.Where(p => p.currentCard.role == ((RoleWinRequirement)requirement).role).ToList();
		} else if (requirement is RelationWinRequirement) {
			RelationWinRequirement relationRequirement = requirement as RelationWinRequirement;
			if(relationRequirement.relation == Relation.Self) {
			return instance.players.Where(p => p.locationId == evaluatedPlayer.locationId).ToList();
			} else {
				Debug.LogError("Unhandled relation: " + relationRequirement.relation);
				return null;
			}
		} else {
			Debug.LogError("Unhandled win requirement type.");
			return null;
		}
	}

	private void ExecuteNightActionsInOrder ()
	{
		List<Player> actingPlayersByTurnOrder = instance.players.Where (p => p.dealtCard.order.primary.HasValue).OrderBy (p => p.dealtCard.order.primary).
			ThenBy (p => p.dealtCard.order.secondary).ToList ();
		for (int i = 0; i < actingPlayersByTurnOrder.Count; i++) {
			Player actingPlayer = actingPlayersByTurnOrder [i];
			for (int j = 0; j < actingPlayer.dealtCard.nightActions.Length; j++) {
				if(actingPlayer.dealtCard.nightActions[j] is ViewOneAction) { //Lone werewolf, robber 2nd, insomniac, mystic wolf, apprentice seer
					ViewOneAction vonAction = ((ViewOneAction)actingPlayer.dealtCard.nightActions[j]);
					int targetLocationId = vonAction.target == TargetType.Self ? actingPlayer.locationId : 
						actingPlayer.nightLocationSelection.locationIds[((int)vonAction.target)];
					if(targetLocationId == -1) {
						//TODO Notify "You chose not to view a card."
					} else {
						actingPlayer.observations.Add(new Observation(targetLocationId, idsToLocations[targetLocationId].currentCard.gamePieceId));
					}
				} else if(actingPlayer.dealtCard.nightActions[j] is SwapTwoAction) { //Robber 1st, troublemaker, drunk

				} else if(actingPlayer.dealtCard.nightActions[j] is ViewUpToTwoAction) { //Seer

				}
			}
		}
	}

	private static RealCard PullFirstCardFromDeck() {
		RealCard card = instance.deck[0];
		instance.deck.Remove(card);
		return card;
	}

	public static int RegisterGamePiece(IGamePiece gamePiece) {
		instance.gamePiecesById.Add(gamePiece);
		return instance.gamePiecesById.Count - 1;
	}

	public static int RegisterLocation(ILocation location) {
		instance.idsToLocations.Add(location);
		return instance.idsToLocations.Count - 1;
	}

	public static void SubmitNightAction(Player player, int[] targetLocationIds) {
		if(instance.currentPhase != GamePhase.Night_Input) {
			Debug.LogError("Received night action outside of Night_Input phase");
			return;
		}
		player.nightLocationSelection = new Selection(targetLocationIds);
		instance.playersAwaitingResponseFrom.Remove(player);

		if(instance.playersAwaitingResponseFrom.Count == 0) {
			SetPhase(GamePhase.Day);
		}
	}

	public static void SubmitVote(Player player, int locationId) {
		if(instance.currentPhase != GamePhase.Day) {
			Debug.LogError("Received night action outside of Night_Input phase");
			return;
		}

		player.locationIdVote = locationId;
		instance.playersAwaitingResponseFrom.Remove(player);

		if(instance.playersAwaitingResponseFrom.Count == 0) {
			SetPhase(GamePhase.Result);
		}
	}
}

[System.Serializable]
public class RealizedPrompt {
	public string cohortString = "";
	public OptionsSet options;
	public List<ButtonInfo> buttons = new List<ButtonInfo>();

	public RealizedPrompt(Player player) {
		//Evalutate cohort and realize strings
		if(player.dealtCard.cohort == CohortType.None) {
			if(player.dealtCard.prompt != null) {
				cohortString = player.dealtCard.prompt.explanation;
				options = player.dealtCard.prompt.options;
			}
		} else {
			List<Player> cohorts = null;
			switch(player.dealtCard.cohort) {
			case CohortType.WerewolfNature:
				cohorts = GameController.instance.players.Where (p => p.dealtCard.nature == Nature.Werewolf && p.name != player.name).ToList();
				break;
			case CohortType.Mason:
				cohorts = GameController.instance.players.Where (p => p.dealtCard.role == Role.Mason && p.name != player.name).ToList();
				break;
			}
			player.cohortLocations = cohorts.Select(p => p.locationId).ToArray();
			if(cohorts.Count == 0) {
				cohortString = player.dealtCard.prompt.explanation;
				options = player.dealtCard.prompt.options;
			} else {
				for(int i = 0; i < cohorts.Count; i++) {
					if( i != 0) {
						cohortString += " ";
					}
					cohortString += string.Format(player.dealtCard.promptIfCohort.explanation, cohorts[i].name);
				}
				options = player.dealtCard.promptIfCohort.options;
			}
		}

		switch(options) {
		case OptionsSet.None:
			buttons.Add(new ButtonInfo(-1, "Ready"));
			break;
		case OptionsSet.May_CenterCard: //Apprentice seer, lone werewolf
			for (int i = 0; i < GameController.instance.centerCards.Count; i++) {
				CenterCardSlot slot = GameController.instance.centerCards[i];
				buttons.Add(new ButtonInfo(slot.locationId, "Center Card #" + (i + 1).ToString()));
			}
			buttons.Add(new ButtonInfo(-1, "Pass"));
			break;
		case OptionsSet.Must_CenterCard: //Drunk and copycat
			for (int i = 0; i < GameController.instance.centerCards.Count; i++) {
				CenterCardSlot slot = GameController.instance.centerCards[i];
				buttons.Add(new ButtonInfo(slot.locationId, "Center Card #" + (i + 1).ToString()));
			}
			break;
		case OptionsSet.May_OtherPlayer: //Robber, mystic wolf
			Debug.Log("Not implemented: may other player");
			buttons.Add(new ButtonInfo(-1, "Ready"));
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
public class RealCard : Card, IGamePiece {
	
	private int _gamePieceId;
	public int gamePieceId {
		get {
			return _gamePieceId;
		}
	}
	public string name {
		get {
			return role.ToString();
		}
	}
	//any viewed roles (e.g., Doppleganger, paranormal investigator, copycat)

	public RealCard(Role role) : base(role) {
		_gamePieceId = GameController.RegisterGamePiece(this);
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
	}
	public CenterCardSlot(int index, RealCard card) {
		this.centerCardIndex = index;
		this.dealtCard = card;
		this._currentCard = card;
		_locationId = GameController.RegisterLocation(this);
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
public struct Selection {
	public int[] locationIds;
	public Selection(params int[] locationIds) {
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