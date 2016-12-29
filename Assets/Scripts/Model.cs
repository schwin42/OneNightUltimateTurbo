using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class Team
{
	public TeamName name;
	public string description;
	public WinRequirement[] winRequirements;

	public static List<Team> teams = new List<Team> () {
		new Team() {
			name = TeamName.Village,
			description = "You are on the village team.",
			winRequirements = new WinRequirement[] {
				new NatureWinRequirement (Nature.Werewolf, WinPredicate.MustDie, 
					new WinRequirement[] { new NatureWinRequirement(Nature.Villageperson, WinPredicate.MustNotDie) } )
			}
		},
		new Team() {
			name = TeamName.Werewolf,
			description = "You are on the werewolf team.",
			winRequirements = new WinRequirement[] {
				new NatureWinRequirement (Nature.Werewolf, WinPredicate.MustNotDie, new WinRequirement[] {
					new NatureWinRequirement(Nature.Villageperson, WinPredicate.MustDie),
					new RelationWinRequirement(Relation.Self, WinPredicate.MustNotDie)
				}),
				new RoleWinRequirement (Role.Tanner, WinPredicate.MustNotDie)
			}
		},
		new Team() {
			name = TeamName.Vampire,
			description = "You are on the vampire team.",
			winRequirements = new WinRequirement[] {
				new NatureWinRequirement (Nature.Vampire, WinPredicate.MustNotDie, new WinRequirement[] {
					new NatureWinRequirement(Nature.Villageperson, WinPredicate.MustDie),
					new RelationWinRequirement(Relation.Self, WinPredicate.MustNotDie)
				}),
				new RoleWinRequirement (Role.Tanner, WinPredicate.MustNotDie)
			}
		},
		new Team() {
			name = TeamName.None,
			description = "You are not on a team.",
			winRequirements = new WinRequirement[] { },
		},
	};

//	public static Team Village = new Team() {
//		name = TeamName.Village,
//		description = "You are on the village team.",
//		winRequirements = new WinRequirement[] {
//			new NatureWinRequirement (Nature.Werewolf, WinPredicate.MustDie, 
//				new WinRequirement[] { new NatureWinRequirement(Nature.Villageperson, WinPredicate.MustNotDie) } )
//		}
//	};

//	public static Team Werewolf = new Team() {
//
//	};

//	public static Team Vampire = new Team() {
//
//	};
}

public class NoTeam : Team {
	public NoTeam() {
		description = "You are not on the villager team or the werewolf team.";
	}
}

[System.Serializable]
public enum TeamName
{
	None = -1,
	Village = 0,
	Werewolf = 1,
	Vampire = 2,
}

[System.Serializable]
public enum Nature
{
	None = -1,
	Villageperson = 0,
	Werewolf = 1,
	Vampire = 2,
	Variable = 3,
}

[System.Serializable]
public enum CohortType {
	None = -1,
	WerewolfNature = 0,
	Mason = 1,
	VampireNature = 2,
	Assassin = 3,
}

[System.Serializable]
public class CardData
{
	//Game rules
	public Role role = Role.None;
	public TeamName team = TeamName.None;
	public Nature nature;
	public WinRequirement[] winRequirements;
	public Order order = Order.None;
	public CohortType cohort = CohortType.None;
	public Prompt promptIfCohort = null;
	public Prompt prompt = null;
	public List<HiddenAction> nightActions = new List<HiddenAction>();
	public List<HiddenAction> nightActionsIfCohort = new List<HiddenAction>();
	public string duskActions = null;
	public string duskActionsIfCohort = null;

	//Deckbuilding
	public Selector seedRequirement = null;
	public int maxQuantity = 1;

	public CardData (Role role) {
		this.role = role;
		//Role ideas
		//The outcast- nature: villageperson, team: village, if the outcast dies, the villagers and the werewolves win, but the outcast loses
		//The halfblood/ daywalker

		//
		//[System.Serializable]
		//public class Mason : Card { //When randomly selecting cards, always use both Masons
		//	public Mason () : base() {
		//		team = Team.Village;
		//		nature = Nature.Villageperson;
		//		quantity = 2;
		//		order = new Order(4);
		//		seedRequirement = Role.Mason;
		//	}
		//}
	}
//	public static Card Robber { get { return new Robber(); } }
//	public static Card Seer { get { return new Seer(); } }
//	public static Card Troublemaker { get { return new Troublemaker(); } }
//	public static Card Minion { get { return new Minion(); } }
//	public static Card Tanner { get { return new Tanner(); } }
}

[System.Serializable]
public enum Role {
	None,
	Werewolf,
	Villager,
	Robber,
	Drunk,
	Mason,
	Minion,
	Insomniac,
	Seer,
	Troublemaker,
	Tanner,
	Hunter,
	MysticWolf,
	ApprenticeSeer,
	AlphaWolf,
	ParanormalInvestigator,
	Bodyguard,
	Copycat,
	Doppelgänger,
	Vampire,
	TheCount,
	Renfield,
	Diseased,
	Cupid,
	Instigator,
	Priest,
	Assassin,
	ApprenticeAssassin,
	Sentinel,
	ApprenticeTanner,
	Thing,
	Marksman,
	Witch,
	Pickpocket,
	VillageIdiot,
	AuraSeer,
	Gremlin,
	Squire,
	Beholder,
	Revealer,
	Curator,
	DreamWolf,
	Cursed,
	Prince,
	TheMaster,
}

public enum WinPredicate
{
	None = -1,
	MustNotDie = 0,
	MustDie = 1,
}

public enum Relation {
	None = -1,
	Self = 0,
}

public abstract class WinRequirement
{
	public WinPredicate predicate;
	public WinRequirement[] fallback; //Requirement to use if selected role doesn't exist (use for villagers, minion, apprentice tanner, apprentice assassin)
	public bool isEmpty {
		get {
			return predicate == WinPredicate.None;
		}
	}

	public WinRequirement (WinPredicate predicate, WinRequirement[] fallback)
	{
		this.predicate = predicate;
		this.fallback = fallback;
	}
}

public class RoleWinRequirement : WinRequirement {
	public Role role = Role.None;
	public RoleWinRequirement (Role role, WinPredicate predicate, WinRequirement[] fallback = null) : base(predicate, fallback) {
		this.role = role;
	}
}

public class NatureWinRequirement : WinRequirement {
	public Nature nature = Nature.None;
	public NatureWinRequirement (Nature nature, WinPredicate predicate, WinRequirement[] fallback = null) : base(predicate, fallback) {
		this.nature = nature;
	}
}

public class RelationWinRequirement : WinRequirement {
	public Relation relation = Relation.None;
	public RelationWinRequirement (Relation relation, WinPredicate predicate, WinRequirement[] fallback = null) : base(predicate, fallback) {
		this.relation = relation;
	}
}

[System.Serializable]
public class Order {
	public bool isEmpty = false;
	public int primary;
	public string secondary = "";

	public static Order None {
		get {
			return new Order();
		}
	}

	public Order() { 
		this.isEmpty = true;
	}

	public Order(int primary, string secondary = "") {
		this.primary = primary;
		this.secondary = secondary;
	}

	public override string ToString() {
		
		return !isEmpty ? "(+" +  primary.ToString() + secondary + ")" : "";
	}
}

[System.Serializable]
public enum OptionsSet {
	None = 0,
	May_CenterCard = 1,
	May_OtherPlayerOrTwoCenterCards = 2,
	May_OtherPlayer = 3,
	May_TwoOtherPlayers = 4,
	Must_CenterCard = 5,
	May_UpToTwoOtherPlayersCars = 6,
}

[System.Serializable]
public class Prompt {
	public string explanation;
	public OptionsSet options;

	public Prompt(string explanation, OptionsSet options = OptionsSet.None) {
		this.explanation = explanation;
		this.options = options;
	}
}

public enum TargetType {
	Self = -1,
	SelectionA = 0,
	SelectionB = 1,
}

[System.Serializable]
public enum ActionType {
	None = -1,
	ViewOne = 0,
	SwapTwo = 1,
	ViewUpToTwo = 2,
}

[System.Serializable]
public class HiddenAction {
	public ActionType actionType;
	public List<TargetType> targets;

	public HiddenAction(ActionType actionType, List<TargetType> targets) {
		this.actionType = actionType;
		this.targets = targets;
	}
}

//[System.Serializable]
//public class ViewOneAction : HiddenAction {
//	public TargetType target;
//	public ViewOneAction(TargetType target) {
//		this.target = target;
//	}
//}
//
//[System.Serializable]
//public class SwapTwoAction : HiddenAction {
//	public TargetType targetA;
//	public TargetType targetB;
//	public SwapTwoAction(TargetType targetA, TargetType targetB) {
//		this.targetA = targetA;
//		this.targetB = targetB;
//	}
//}
//
//[System.Serializable]
//public class ViewUpToTwoAction : HiddenAction {
//	public TargetType[] targets;
//	public ViewUpToTwoAction(params TargetType[] targets) {
//		this.targets = targets;
//	}
//}

public enum SpecialSelection {
	None = -1,
	MarkPlacer = 0,
	CardSwapper = 1,
	MoveOrViewer = 2,
	SeerOrApprenticeSeer = 3,
}

[System.Serializable]
public class Selector {

	public Role role = Role.None;
	public Nature nature = Nature.None;
	public SpecialSelection specialSelection = SpecialSelection.None;

	public static Selector None {
		get {
			return new Selector () {
				role = Role.None,
				nature = Nature.None,
				specialSelection = SpecialSelection.None,
			};
		}
	}

	private Selector() { }

	public Selector(Role role) {
		this.role = role;
	}

	public Selector(Nature nature) {
		this.nature = nature;
	}

	public Selector(SpecialSelection specialSelection) {
		this.specialSelection = specialSelection;
	}

	public bool isEmpty {
		get {
			return role == Role.None && nature == Nature.None /*&& specialSelection == SpecialSelection.None*/; //TODO Implement special selection
		}
	}

//	public List<CardData> Filter(List<CardData> cardData) {
//		if (role != Role.None) {
//			return cardData.Where (cd => cd.role == role).ToList();
//		} else if (nature != Nature.None) {
//			return cardData.Where (cd => cd.nature == nature).ToList();
//		} else {
//			//TODO Implement special cases
//			Debug.LogError("Special selection not implemented.");
//			return null;
//		}
//	}

	public int TryGetFirstIndex(List<CardData> cardData) { //Currently assumes selectee exists in the list
		CardData cardAtFirstIndex;
		if (role != Role.None) {
			cardAtFirstIndex = cardData.FirstOrDefault (cd => cd.role == role);
		} else if (nature != Nature.None) {
			cardAtFirstIndex = cardData.FirstOrDefault (cd => cd.nature == nature);
		} else if (specialSelection != SpecialSelection.None) {
			switch(specialSelection) {
			case SpecialSelection.MarkPlacer:
				return cardData.IndexOf(cardData.First(cd => cd.duskActions.Contains("Place")));
			case SpecialSelection.CardSwapper:
				Debug.Log("Special selection not handled: " + specialSelection);
				return -1;
			case SpecialSelection.MoveOrViewer:
				Debug.Log("Special selection not handled: " + specialSelection);
				return -1;
			case SpecialSelection.SeerOrApprenticeSeer:
				return cardData.IndexOf(cardData.First(cd => cd.role == Role.Seer || cd.role == Role.ApprenticeSeer));
			default:
				Debug.LogError("Special selection not handled: " + specialSelection);
				return -1;
			}
		} else {
			Debug.LogError("Called filter on empty selector. Check if selector is empty.");
			return -1;
		}

		if(cardAtFirstIndex != null) {
			return cardData.IndexOf(cardAtFirstIndex);
		} else {
			return -1;
		}
	}
}
