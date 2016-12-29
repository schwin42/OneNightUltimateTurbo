using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEditor;
using System.Linq;

public class GameData : MonoBehaviour {

	static string sourceDir = "GameData";
	static string sourceFilename = "OneNightUltimateData.tsv"; 

	private static GameData _instance = null;
	public static GameData instance {
		get {
			if(_instance == null) {
				_instance = GameObject.FindObjectOfType<GameData>();
			}
			return _instance;
		}
	}

	public List<CardData> cardData = new List<CardData>();
	public List<CardData> cardPool = new List<CardData>(); //Includes copies of duplicate roles

	[MenuItem ("ONU/Load Data from File")]
	public static void LoadDataFromFile() {

		string[] lines = File.ReadAllLines(Application.dataPath + "/" + sourceDir + "/" + sourceFilename);
		List<string> headers = new List<string>();
		List<Dictionary<string, string>> roleDicts = new List<Dictionary<string, string>>();
		for(int i = 0; i < lines.Length; i++) {
			string[] cells = lines[i].Split('\t');
			if(i == 0) {
				//Record headers
				for(int j = 0; j < cells.Length; j++) {
					headers.Add(cells[j]);
				}
			} else {
				Dictionary<string, string> roleFields = new Dictionary<string, string>();
				for(int j = 0; j < cells.Length; j++) {
					roleFields.Add(headers[j], cells[j]);
				}
				roleDicts.Add(roleFields);
			}
		}

		instance.cardData = new List<CardData>();
		instance.cardPool = new List<CardData>();
		TeamName cardTeam = TeamName.None;
		foreach(Dictionary<string, string> dict in roleDicts) {
			Role cardRole = ((Role)Enum.Parse(typeof(Role), dict["Role"].Replace(" ", "")));
			switch(dict["Team"]) {
			case "Werewolf":
				cardTeam = TeamName.Werewolf;
				break;
			case "Vampire":
				cardTeam = TeamName.Vampire;
				break;
			case "Village":
				cardTeam = TeamName.Village;
				break;
			case "NoTeam":
				cardTeam = TeamName.None;
				break;
				default:
				Debug.LogError("Unhandled team: " + dict["Team"]);
				break;
			}
			Nature cardNature = ((Nature)Enum.Parse(typeof(Nature), dict["Nature"]));
			Selector cardSeedRequirement = Selector.None;
			if(!string.IsNullOrEmpty(dict["SeedRequirement"])) {
				string seedRequirement = dict["SeedRequirement"];
				try {
					cardSeedRequirement = new Selector(((Role)Enum.Parse(typeof(Role), seedRequirement)));
				} catch (Exception e) { }
				if (cardSeedRequirement.isEmpty) {
					try {
						string natureSubstring = seedRequirement.Substring (6);
						Nature nature = ((Nature)Enum.Parse (typeof(Nature), natureSubstring));
						cardSeedRequirement = new Selector (nature); //Start after "Nature"
					} catch (Exception e) {
					}
				}
				if (cardSeedRequirement.isEmpty) {
					cardSeedRequirement = new Selector (((SpecialSelection)Enum.Parse (typeof(SpecialSelection), seedRequirement)));
				}
			}
			Order cardOrder;
			string cardOrderString = dict["Order"];
			if(cardOrderString.Length == 0) {
				cardOrder = new Order();
			} else {
				bool isNegative = false;
				if(cardOrderString[0] == '-') {
					isNegative = true;
					cardOrderString = cardOrderString.Substring(1);
				}
				int number = Convert.ToInt32(cardOrderString.Substring(0, 1));
				cardOrderString = cardOrderString.Substring(1);
				string letter = "";
				if(cardOrderString.Length > 0) {
					letter = cardOrderString.Substring(0, 1);
				}
				cardOrder = new Order(number * (isNegative ? -1 : 1), letter);
			}
			string nightActionsString = dict["NightActions"];
			List<HiddenAction> cardNightActions = new List<HiddenAction>();
			if(nightActionsString != "") {
				string[] nightActionStrings = nightActionsString.Split(';');
				for(int i = 0; i < nightActionStrings.Length; i++) {
					string[] nightActionComponents = nightActionStrings[i].Split('(');
					string nightActionType = nightActionComponents[0];
					string targetString = nightActionComponents[1].Remove(nightActionComponents[1].IndexOf(')'));
					string[] stringTargets = targetString.Split(',');
					List<TargetType> actionTargets = new List<TargetType>();
					for(int j = 0; j < stringTargets.Length; j++) {
						TargetType targetType = (TargetType)Enum.Parse(typeof(TargetType), stringTargets[j]);
						actionTargets.Add(targetType);
					}
					ActionType actionType = (ActionType)Enum.Parse(typeof(ActionType), nightActionType);
					cardNightActions.Add(new HiddenAction(actionType, actionTargets));
				}
			}
			string cardDuskActions = dict["DuskActions"];
			int cardMaxQuantity = int.Parse(dict["MaxQuantity"]);
			CardData card = new CardData(cardRole) {
				team = cardTeam,
				nature = cardNature,
//				public virtual WinRequirement[] winRequirements { get { return team.winRequirements; } }
				order = cardOrder,
//				public Order order = Order.None;
//				public CohortType cohort = CohortType.None;
//				public Prompt promptIfCohort = null;
//				public Prompt prompt = null;

				nightActions = cardNightActions,
//				nightActionsIfCohort = cardNightActionsIfCohort;
				duskActions = cardDuskActions,
				seedRequirement = cardSeedRequirement,
				maxQuantity = cardMaxQuantity,
			};
			instance.cardData.Add(card);
			for(int i = 0; i < card.maxQuantity; i++) {
				instance.cardPool.Add(card);
			}
		}
		instance.cardData = instance.cardData.OrderBy(cd => cd.role.ToString()).ToList();
	}
}
