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
			new WinRequirement (Nature.Werewolf, WinPredicate.MustDie)
		};
	}
}

[System.Serializable]
public class WerewolfTeam : Team
{
	public WerewolfTeam () {
		description = "You are on the werewolf team.";
		winRequirements = new WinRequirement[] {
			new WinRequirement (Nature.Werewolf, WinPredicate.MustNotDie),
			new WinRequirement (Role.Tanner, WinPredicate.MustNotDie)
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
			promptIfCohort = new Prompt("{0} is a werewolf.");
			prompt = new Prompt("There are no other werewolves. You may look at a card from the center.", OptionsSet.May_CenterCard);
			nightActions = new NightAction[] { new ViewOneNightAction(TargetType.SelectionA) };
			maxQuantity = 2;
			break;
		case Role.Villager:
			team = Team.Village;
			nature = Nature.Villageperson;
			prompt = new Prompt("You have no special abilities.");
			maxQuantity = 3;
			seedRequirement = Role.Villager;
			break;
		case Role.Robber:
			team = Team.Village;
			nature = Nature.Villageperson;
			order = new Order(6);
			prompt = new Prompt("You may swap cards with another player, then view your new card.", OptionsSet.May_OtherPlayer);
			nightActions = new NightAction[] {
				new SwapTwoNightAction (TargetType.Self, TargetType.SelectionA),
				new ViewOneNightAction(TargetType.Self),
			};
			break;
		case Role.Drunk:
			team = Team.Village;
			nature = Nature.Villageperson;
			order = new Order(8);
			prompt = new Prompt("You must swap your card with a card in the center and may not view your new card.", OptionsSet.Must_CenterCard);
			nightActions = new NightAction[] { new SwapTwoNightAction(TargetType.Self, TargetType.SelectionA) };
			break;
		}
	
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
//[System.Serializable]
//public class Minion : Card
//{
//	public Minion () : base() {
//		team = Team.Werewolf;
//		nature = Nature.Villageperson;
//		order = new Order(3);
//	}
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

public class WinRequirement
{
	public Role role = Role.None;
	public Nature nature = Nature.None;
	public WinPredicate predicate;

	public WinRequirement (Role role, WinPredicate predicate) {
		this.role = role;
		this.predicate = predicate;
	}

	public WinRequirement (Nature nature, WinPredicate predicate)
	{
		this.nature = nature;
		this.predicate = predicate;
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

	public Order() { }

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

public enum NightActionType {
	ViewOne = 0,
	SwapTwo = 1,
	ViewUpToTwo = 2,
}

public enum TargetType {
	Self = -1,
	SelectionA = 0,
	SelectionB = 1,
}

public abstract class NightAction {


}

public class ViewOneNightAction : NightAction {
	public TargetType target;
	public ViewOneNightAction(TargetType target) {
		this.target = target;
	}
}

public class SwapTwoNightAction : NightAction {
	public TargetType targetA;
	public TargetType targetB;
	public SwapTwoNightAction(TargetType targetA, TargetType targetB) {
		this.targetA = targetA;
		this.targetB = targetB;
	}
}

public class ViewUpToTwoNightAction : NightAction {
	public TargetType[] targets;
	public ViewUpToTwoNightAction(params TargetType[] targets) {
		this.targets = targets;
	}
}
