using UnityEngine;
using System.Collections;
using System.Linq;

[System.Serializable]
public abstract class Team
{
	public static VillageTeam Village { get { return new VillageTeam (); } }
	public static WerewolfTeam Werewolf { get { return new WerewolfTeam (); } }
	public static NoTeam None { get { return new NoTeam (); } }


	public string description;
	public WinRequirement[] winRequirements;
}

[System.Serializable]
public class VillageTeam : Team
{
	public VillageTeam () {
		description = "You are on the village team.";
		winRequirements = new WinRequirement[] {
			new NatureWinRequirement (Nature.Werewolf, WinPredicate.MustDie, 
				new WinRequirement[] { new NatureWinRequirement(Nature.Villageperson, WinPredicate.MustNotDie) } )
		};
	}
}

[System.Serializable]
public class WerewolfTeam : Team
{
	public WerewolfTeam () {
		description = "You are on the werewolf team.";
		winRequirements = new WinRequirement[] {
			new NatureWinRequirement (Nature.Werewolf, WinPredicate.MustNotDie, new WinRequirement[] {
				new NatureWinRequirement(Nature.Villageperson, WinPredicate.MustDie),
				new RelationWinRequirement(Relation.Self, WinPredicate.MustNotDie)
			}),
			new RoleWinRequirement (Role.Tanner, WinPredicate.MustNotDie)
		};
	}
//	public override WinRequirement[] winRequirements { get { return new WinRequirement[] { 
//			new WinRequirement (Nature.Werewolf, WinPredicate.MustNotDie),
//			new WinRequirement(Role.Tanner, WinPredicate.MustNotDie),
//		}; } }
}

public class NoTeam : Team {
	public NoTeam() {
		description = "You are not on the villager team or the werewolf team.";
	}
}

[System.Serializable]
public enum Nature
{
	None = 0,
	Villageperson = 1,
	Werewolf = 2,
	Vampire = 3,
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
public class Card
{
	//Game rules
	public Role role = Role.None;
	public Team team = Team.None;
	public Nature nature;
	public virtual WinRequirement[] winRequirements { get { return team.winRequirements; } }
	public Order order = Order.None;
	public CohortType cohort = CohortType.None;
	public Prompt promptIfCohort = null;
	public Prompt prompt = null;
	public NightAction[] nightActions = new NightAction[] { };
	public NightAction[] nightActionsIfCohort = new NightAction[] { }; //Needed for vampires placing mark of the vampire?

	//Deckbuilding
	public Role seedRequirement = Role.None;
	public int maxQuantity = 1;

	public Card (Role role) {
		this.role = role;
		switch(role) {
		case Role.Werewolf:
			team = Team.Werewolf;
			nature = Nature.Werewolf;
			order = new Order(2);
			cohort = CohortType.WerewolfNature;
			promptIfCohort = new Prompt("{0} is the other werewolf.");
			prompt = new Prompt("There are no other werewolves. You may look at a card from the center.", OptionsSet.May_CenterCard);
			nightActions = new NightAction[] { new ViewOneAction(TargetType.SelectionA) };
			maxQuantity = 2;
			break;
		case Role.Villager:
			team = Team.Village;
			nature = Nature.Villageperson;
			prompt = new Prompt("You have no special abilities.");
			maxQuantity = 3;
			seedRequirement = Role.Villager;
			break;
		case Role.Robber: //Not implemented
			team = Team.Village;
			nature = Nature.Villageperson;
			order = new Order(6);
			prompt = new Prompt("You may swap cards with another player, then view your new card.", OptionsSet.May_OtherPlayer);
			nightActions = new NightAction[] {
				new SwapTwoAction (TargetType.Self, TargetType.SelectionA),
				new ViewOneAction(TargetType.Self),
			};
			break;
		case Role.Drunk: //Not implemented
			team = Team.Village;
			nature = Nature.Villageperson;
			order = new Order(8);
			prompt = new Prompt("You must swap your card with a card in the center and may not view your new card.", OptionsSet.Must_CenterCard);
			nightActions = new NightAction[] { new SwapTwoAction(TargetType.Self, TargetType.SelectionA) };
			break;
		case Role.Mason:
			team = Team.Village;
			nature = Nature.Villageperson;
			order = new Order(4);
			cohort = CohortType.Mason;
			promptIfCohort = new Prompt("{0} is the other mason.");
			prompt = new Prompt("There are no other masons.");
			maxQuantity = 2;
			seedRequirement = Role.Mason;
			break;
		case Role.Minion:
			team = Team.Werewolf;
			nature = Nature.Villageperson;
			order = new Order(3);
			cohort = CohortType.WerewolfNature;
			promptIfCohort = new Prompt("{0} is a werewolf.");
			prompt = new Prompt("There are no werewolves. You win only if another villager dies.");
			maxQuantity = 1;
			break;
		}

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

public enum Role {
	None = -1,
	Werewolf = 0,
	Mason = 1,
	Tanner = 2,
	Villager = 3,
	Robber = 4,
	Drunk = 5,
	Minion = 6,
	Seer = 7,
	Troublemaker = 8,
	Insomniac = 9,
}

//
//[System.Serializable]
//public class Seer : Card
//{
//	public Seer () : base() {
//		team = Team.Village;
//		nature = Nature.Villageperson;
//		order = new Order(5);
//	}
//}
//
//[System.Serializable]
//public class Troublemaker : Card
//{
//	public Troublemaker () : base() {
//		team = Team.Village;
//		nature = Nature.Villageperson;
//		order = new Order(7);
//	}
//}
//

//}
//
//[System.Serializable]
//public class Tanner : Card
//{
//	//How to modify werewolf win condition so that tanner can't die
//	public Tanner () : base() {
//		nature = Nature.Villageperson;
//	}
//
//	public override WinRequirement[] winRequirements {
//		get {
//			return new WinRequirement[] {
//				new WinRequirement( Role.Tanner, WinPredicate.MustDie )
//			};
//		}
//	}
//}

//
//public class Insomniac : Card {
//	public Insomniac () : base() {
//		team = Team.Village;
//		nature = Nature.Villageperson;
//		order = new Order(9);
//	}
//}

public enum WinPredicate
{
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

public class Order {
	public int? primary;
	public string secondary;

	public static Order None {
		get {
			return new Order();
		}
	}

	public Order() {
		this.primary = null;
		this.secondary = "";
	}

	public Order(int primary, string secondary = "") {
		this.primary = primary;
		this.secondary = secondary;
	}

	public override string ToString() {
		
		return primary.HasValue ? "(+" +  primary.ToString() + secondary + ")" : "";
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

public abstract class NightAction {
}

public class ViewOneAction : NightAction {
	public TargetType target;
	public ViewOneAction(TargetType target) {
		this.target = target;
	}
}

public class SwapTwoAction : NightAction {
	public TargetType targetA;
	public TargetType targetB;
	public SwapTwoAction(TargetType targetA, TargetType targetB) {
		this.targetA = targetA;
		this.targetB = targetB;
	}
}

public class ViewUpToTwoAction : NightAction {
	public TargetType[] targets;
	public ViewUpToTwoAction(params TargetType[] targets) {
		this.targets = targets;
	}
}
