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
			name = TeamName.VillageTeam,
			description = "You are on the village team.",
			winRequirements = new WinRequirement[] {
				new WinRequirement (new Selector(Nature.Werewolf), WinPredicate.MustDie, 
					new WinRequirement[] { new WinRequirement(new Selector(Nature.Villageperson), WinPredicate.MustNotDie, null) } )
			}
		},
		new Team() {
			name = TeamName.WerewolfTeam,
			description = "You are on the werewolf team.",
			winRequirements = new WinRequirement[] {
				new WinRequirement (new Selector(Nature.Werewolf), WinPredicate.MustNotDie, new WinRequirement[] {
					new WinRequirement(new Selector(Nature.Villageperson), WinPredicate.MustDie, null),
					new WinRequirement(new Selector(Relation.Self), WinPredicate.MustNotDie, null)
				}),
				new WinRequirement (new Selector(Role.Tanner), WinPredicate.MustNotDie, null)
			}
		},
		new Team() {
			name = TeamName.VampireTeam,
			description = "You are on the vampire team.",
			winRequirements = new WinRequirement[] {
				new WinRequirement (new Selector(Nature.Vampire), WinPredicate.MustNotDie, new WinRequirement[] {
					new WinRequirement(new Selector(Nature.Villageperson), WinPredicate.MustDie, null),
					new WinRequirement(new Selector(Relation.Self), WinPredicate.MustNotDie, null)
				}),
				new WinRequirement (new Selector(Role.Tanner), WinPredicate.MustNotDie, null)
			}
		},
		new Team() {
			name = TeamName.NoTeam,
			description = "You are not on a team.",
			winRequirements = new WinRequirement[] { },
		},
	};
}

public class NoTeam : Team {
	public NoTeam() {
		description = "You are not on the villager team or the werewolf team.";
	}
}

[System.Serializable]
public enum TeamName
{
	NoTeam = -1,
	VillageTeam = 0,
	WerewolfTeam = 1,
	VampireTeam = 2,
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

//[System.Serializable]
//public enum CohortType {
//	None = -1,
//	WerewolfNature = 0,
//	Mason = 1,
//	VampireNature = 2,
//	Assassin = 3,
//}

[System.Serializable]
public class CardData
{
	//Game rules
	public Role role = Role.None;
	public TeamName team = TeamName.NoTeam;
	public Nature nature;
	public WinRequirement winRequirement;
	public Order order = Order.None;
	public Selector cohort = Selector.None;
	public string promptIfCohort = null;
	public string prompt = null;
	public List<SubAction> hiddenAction = new List<SubAction>();
	public List<SubAction> hiddenActionIfCohort = new List<SubAction>();
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

public class WinRequirement
{
	public Selector subject;
	public WinPredicate predicate;
	public WinRequirement[] fallback; //Requirement to use if selected role doesn't exist (use for villagers, minion, apprentice tanner, apprentice assassin)

	public WinRequirement (Selector subject, WinPredicate predicate, WinRequirement[] fallback)
	{
		this.subject = subject;
		this.predicate = predicate;
		this.fallback = fallback;
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
public enum SelectableObjectType {
	Self = 0,
	TargetCenterCard = 1,
	TargetOtherPlayer = 2,
	TargetAnyPlayer = 3,
	LastTarget = 4,
	TargetFork = 5,
}

[System.Serializable]
public enum ActionType {
	None = -1,
	ViewOne = 0,
	SwapTwo = 1,
	ViewTwo = 2,
	ChooseFork = 3,
}

[System.Serializable]
public class SubAction {
	public ActionType actionType;
	public List<SelectableObjectType> targets;
	public bool isMandatory;


	public SubAction(ActionType actionType, List<SelectableObjectType> targets, bool isMandatory) {
		this.actionType = actionType;
		this.targets = targets;
		this.isMandatory = isMandatory;
	}
}

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
	public Relation relation = Relation.None;
	public bool isEmpty = true;

	public static Selector None {
		get {
			return new Selector ();
		}
	}

	private Selector() { }

	public override string ToString() {
		if(role != Role.None) {
			return role.ToString();
		} else if(nature != Nature.None) {
			return nature.ToString();
		} else if (relation != Relation.None) {
			return relation.ToString();
		} else {
			Debug.LogError("unhandled selector type");
			return null;
		}
	}

	public Selector(Role role) {
		this.role = role;
		this.isEmpty = false;
	}

	public Selector(Nature nature) {
		this.nature = nature;
		this.isEmpty = false;
	}

	public Selector(SpecialSelection specialSelection) {
		this.specialSelection = specialSelection;
		this.isEmpty = false;
	}

	public Selector(Relation relation) {
		this.relation = relation;
		this.isEmpty = false;
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
				cardAtFirstIndex = cardData.FirstOrDefault(cd => cd.duskActions.Contains("Place"));
				break;
			case SpecialSelection.CardSwapper:
				Debug.LogWarning("Special selection not handled: " + specialSelection);
				return -1;
			case SpecialSelection.MoveOrViewer:
				Debug.LogWarning("Special selection not handled: " + specialSelection);
				return -1;
			case SpecialSelection.SeerOrApprenticeSeer:
				cardAtFirstIndex = cardData.FirstOrDefault(cd => cd.role == Role.Seer || cd.role == Role.ApprenticeSeer);
				break;
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

	public List<GamePlayer> FilterPlayersByDealtCard(List<GamePlayer> players) {
		if (role != Role.None) {
			return players.Where(p => p.dealtCard.data.role == role).ToList();
		} else if (nature != Nature.None) {
			return players.Where(p => p.dealtCard.data.nature == nature).ToList();
		} else if (specialSelection != SpecialSelection.None) {
			switch(specialSelection) {
			case SpecialSelection.MarkPlacer:
				return players.Where(p => p.dealtCard.data.duskActions.Contains("Place")).ToList();
			case SpecialSelection.CardSwapper:
				Debug.Log("Special selection not handled: " + specialSelection);
				return new List<GamePlayer>();
			case SpecialSelection.MoveOrViewer:
				Debug.Log("Special selection not handled: " + specialSelection);
				return new List<GamePlayer>();
			case SpecialSelection.SeerOrApprenticeSeer:
				return players.Where(p => p.dealtCard.data.role == Role.Seer || p.dealtCard.data.role == Role.ApprenticeSeer).ToList();
			default:
				Debug.LogError("Special selection not handled: " + specialSelection);
				return new List<GamePlayer>();
			}
		} else {
			Debug.LogError("Called filter on empty selector.");
			return new List<GamePlayer>();
		}
	}
}
