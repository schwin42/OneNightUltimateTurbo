using UnityEngine;
using System.Collections;
using System.Linq;

[System.Serializable]
public abstract class OnuGameObject {
	public int runtimeId;

	public OnuGameObject() {
		runtimeId = GameController.RegisterOnuGameObject(this);
	}
}

[System.Serializable]
public abstract class Team
{
	public static VillageTeam Village { get { return new VillageTeam (); } }
	public static WerewolfTeam Werewolf { get { return new WerewolfTeam (); } }
	public abstract WinRequirement[] winRequirements { get; }
}

[System.Serializable]
public class VillageTeam : Team
{
	public override WinRequirement[] winRequirements { get { return new WinRequirement[] {
			new WinRequirement (Nature.Villageperson, WinPredicate.MustNotDie)
		}; } }
}

[System.Serializable]
public class WerewolfTeam : Team
{
	public override WinRequirement[] winRequirements { get { return new WinRequirement[] { 
			new WinRequirement (Nature.Werewolf, WinPredicate.MustNotDie),
//			new WinRequirement(Card.Tanner, Predicate.MustNotDie),
		}; } }
}

[System.Serializable]
public enum Nature
{
	None = 0,
	Villageperson = 1,
	Werewolf = 2,
}

[System.Serializable]
public abstract class Card : OnuGameObject
{
	//Rules configuration
	public Team team = null;
	public Nature nature;
	public virtual WinRequirement[] winRequirements { get { return team.winRequirements; } }
	public int quantity = 1;
	public Order order = Order.None;
	public InputInfo inputInfo;

	public static Card Werewolf { get { return new Werewolf(); } }
	public static Card Villager { get { return new Villager(); } }
	public static Card Robber { get { return new Robber(); } }
	public static Card Seer { get { return new Seer(); } }
	public static Card Troublemaker { get { return new Troublemaker(); } }
	public static Card Minion { get { return new Minion(); } }
	public static Card Tanner { get { return new Tanner(); } }
}

[System.Serializable]
public class Werewolf : Card
{
//	public override string GeneratePrompt {
//		get {
//			if(GameController.players.Where(p => p.dealtCard is Werewolf && p.dealtCard.id != id)
//		}
//	}

	public Werewolf() : base() {
		
		team = Team.Werewolf;
		nature = Nature.Werewolf;
		quantity = 2;
		order = new Order(2);



//		inputInfo = new InputInfo(StateCondition.AtLeastOneOtherWerewolf, 
//			new Prompt("Other werewolves are revealed to you." , ),
//			new Prompt("There are no other werewolves. You may look at a card from the center.", OptionsSet.May_CenterCard)
//		);
	}
}

[System.Serializable]
public class Villager : Card
{
	public Villager () : base() {
		team = Team.Village;
		nature = Nature.Villageperson;
		quantity = 3;
	}
}

[System.Serializable]
public class Robber : Card
{
	public Robber () : base() {
		team = Team.Village;
		nature = Nature.Villageperson;
		order = new Order(6);
	}
}

[System.Serializable]
public class Seer : Card
{
	public Seer () : base() {
		team = Team.Village;
		nature = Nature.Villageperson;
		order = new Order(5);
	}
}

[System.Serializable]
public class Troublemaker : Card
{
	public Troublemaker () : base() {
		team = Team.Village;
		nature = Nature.Villageperson;
		order = new Order(7);
	}
}

[System.Serializable]
public class Minion : Card
{
	public Minion () : base() {
		team = Team.Werewolf;
		nature = Nature.Villageperson;
		order = new Order(3);
	}
}

[System.Serializable]
public class Tanner : Card
{
	//How to modify werewolf win condition so that tanner can't die
	public Tanner () : base() {
		nature = Nature.Villageperson;
	}

	public override WinRequirement[] winRequirements {
		get {
			return new WinRequirement[] {
				new WinRequirement( Card.Tanner, WinPredicate.MustDie )
			};
		}
	}
}

[System.Serializable]
public class Mason : Card { //When randomly selecting cards, always use both Masons
	public Mason () : base() {
		team = Team.Village;
		nature = Nature.Villageperson;
		quantity = 2;
		order = new Order(4);
	}
}

public class Insomniac : Card {
	public Insomniac () : base() {
		team = Team.Village;
		nature = Nature.Villageperson;
		order = new Order(9);
	}
}

public enum WinPredicate
{
	MustNotDie = 0,
	MustDie = 1,
}

public class WinRequirement
{
	public Card role = null;
	public Nature nature = Nature.None;
	public WinPredicate predicate;

	public WinRequirement (Card role, WinPredicate predicate) {
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

	public Order() {

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
public enum StateCondition {
	Always = 0, //If no condition is specified, promptIfTrue is used
	AtLeastOneOtherWerewolf = 1,
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

public class InputInfo {
	public StateCondition condition = StateCondition.Always;
	public Prompt promptIfTrue;
	public Prompt promptIfFalse;

	public InputInfo(Prompt prompt) {
		this.promptIfTrue = prompt;
	}
	public InputInfo(StateCondition condition, Prompt promptIfTrue, Prompt promptIfFalse) {
		this.condition = condition;
		this.promptIfTrue = promptIfTrue;
		this.promptIfFalse = promptIfFalse;
	}


}

[System.Serializable]
public class Prompt {
	public string description;
	public OptionsSet options;

	public Prompt(string description, OptionsSet options = OptionsSet.None) {
		this.description = description;
		this.options = options;
	}
}
