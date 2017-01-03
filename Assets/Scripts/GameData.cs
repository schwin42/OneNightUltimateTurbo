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

			string cardDuskActions = dict["DuskActions"];
			int cardMaxQuantity = int.Parse(dict["MaxQuantity"]);
			CardData card = new CardData(cardRole) {
				team = cardTeam,
				nature = ((Nature)Enum.Parse(typeof(Nature), dict["Nature"])),
//				public virtual WinRequirement[] winRequirements { get { return team.winRequirements; } }
				order = ParseOrder(dict["Order"]),
				cohort = ParseSelector(dict["Cohort"]),
//				public CohortType cohort = CohortType.None;
				promptIfCohort = ParsePrompt(dict["PromptIfCohortText"], dict["PromptIfCohortTarget"]),
				prompt = ParsePrompt(dict["PromptText"], dict["PromptTarget"]),
				nightActions = ParseHiddenActionSeries(dict["NightActions"]),
//				nightActionsIfCohort = cardNightActionsIfCohort;
				duskActions = cardDuskActions,
				seedRequirement = ParseSelector(dict["SeedRequirement"]),
				maxQuantity = cardMaxQuantity,
			};
			instance.cardData.Add(card);
			for(int i = 0; i < card.maxQuantity; i++) {
				instance.cardPool.Add(card);
			}
		}
		instance.cardData = instance.cardData.OrderBy(cd => cd.role.ToString()).ToList();
	}

	private static Selector ParseSelector(string selectorString) {
		Selector selector = Selector.None;
		if(!string.IsNullOrEmpty(selectorString)) {
			string seedRequirement = selectorString;
			try {
				selector = new Selector(((Role)Enum.Parse(typeof(Role), seedRequirement)));
			} catch (Exception e) { }
			if (selector.isEmpty) {
				try {
					string natureSubstring = seedRequirement.Substring (6);
					Nature nature = ((Nature)Enum.Parse (typeof(Nature), natureSubstring));
					selector = new Selector (nature); //Start after "Nature"
				} catch (Exception e) {
				}
			}
			if (selector.isEmpty) {
				selector = new Selector (((SpecialSelection)Enum.Parse (typeof(SpecialSelection), seedRequirement)));
			}
		}
		return selector;
	}

	private static Prompt ParsePrompt(string userText, string targetType) {
		Prompt prompt;
		if(userText != "") {
			prompt = new Prompt(userText, 
				string.IsNullOrEmpty(targetType) ? OptionsSet.None : (OptionsSet)Enum.Parse(typeof(OptionsSet), targetType));
		} else {
			prompt = new Prompt();
		}
		return prompt;
	}

	private static Order ParseOrder(string orderString) {
		if(orderString.Length == 0) {
			return new Order();
		} else {
			bool isNegative = false;
			if(orderString[0] == '-') {
				isNegative = true;
				orderString = orderString.Substring(1);
			}
			int number = Convert.ToInt32(orderString.Substring(0, 1));
			orderString = orderString.Substring(1);
			string letter = "";
			if(orderString.Length > 0) {
				letter = orderString.Substring(0, 1);
			}
			return new Order(number * (isNegative ? -1 : 1), letter);
		}
	}

	private static List<HiddenAction> ParseHiddenActionSeries(string hiddenActionSeries) {
		List<HiddenAction> hiddenActions = new List<HiddenAction>();
		if(hiddenActionSeries != "") {
			string[] nightActionStrings = hiddenActionSeries.Split(';');
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
				hiddenActions.Add(new HiddenAction(actionType, actionTargets));
			}
		}
		return hiddenActions;
	}
}
