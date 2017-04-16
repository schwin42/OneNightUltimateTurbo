using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
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

	public List<CardData> cardData;
	public List<CardData> totalCardPool; //Includes copies of duplicate roles
	public List<CardData> readyPool; //Pool of cards that are currently implemented


	public void LoadDataFromFile() {

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
		instance.totalCardPool = new List<CardData>();
		instance.readyPool = new List<CardData>();
		TeamName cardTeam = TeamName.NoTeam;
		foreach(Dictionary<string, string> dict in roleDicts) {
			Role cardRole = ((Role)Enum.Parse(typeof(Role), dict["Role"].Replace(" ", "")));
			switch(dict["Team"]) {
			case "Werewolf":
				cardTeam = TeamName.WerewolfTeam;
				break;
			case "Vampire":
				cardTeam = TeamName.VampireTeam;
				break;
			case "Village":
				cardTeam = TeamName.VillageTeam;
				break;
			case "NoTeam":
				cardTeam = TeamName.NoTeam;
				break;
				default:
				Debug.LogError("Unhandled team: " + dict["Team"]);
				break;
			}

			string cardPrompt = dict["PromptText"];
			int cardMaxQuantity = int.Parse(dict["MaxQuantity"]);

			CardData card = new CardData(cardRole) {
				team = cardTeam,
				nature = ((Nature)Enum.Parse(typeof(Nature), dict["Nature"])),
				winRequirement = ParseWinRequirementSeries(dict["WinRequirements"]),
				order = ParseOrder(dict["Order"]),
				cohort = ParseSelector(dict["Cohort"]),
				promptIfCohort = dict["PromptIfCohortText"],
				prompt = cardPrompt,
				hiddenAction = ParseHiddenActionSeries(dict["NightAction"]),
				hiddenActionIfCohort = ParseHiddenActionSeries(dict["NightActionIfCohort"]),
				seedRequirement = ParseSelector(dict["SeedRequirement"]),
				maxQuantity = cardMaxQuantity,
			};

			//Add to records
			instance.cardData.Add(card);
			bool isImplemented = dict["Status"] == "Implemented";
			for(int i = 0; i < card.maxQuantity; i++) {
				instance.totalCardPool.Add(card);
				if(isImplemented) instance.readyPool.Add(card);
			}
		}
		instance.cardData = instance.cardData.OrderBy(cd => cd.role.ToString()).ToList();
	}

	#pragma warning disable 0168 //Suppress unused variable "e" warnings
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
				} catch (Exception e) {	}
			}
			if (selector.isEmpty) {
				selector = new Selector (((SpecialSelection)Enum.Parse (typeof(SpecialSelection), seedRequirement)));
			}
		}
		return selector;
	}
	#pragma warning restore 0168

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

	private static List<SubAction> ParseHiddenActionSeries(string hiddenActionSeries) {
		List<SubAction> hiddenActions = new List<SubAction>();
		if(hiddenActionSeries != "") {
			string[] nightSubactions = hiddenActionSeries.Split(';');
			for(int i = 0; i < nightSubactions.Length; i++) {
				string[] nightSubactionComponents = nightSubactions[i].Trim().Split('(');
				string nightActionTypeAndOption = nightSubactionComponents[0];
				string[] disectedActionType = nightActionTypeAndOption.Split('_');
				bool isMandatory;
				if(disectedActionType[0] == "May") {
					isMandatory = false;
				} else if (disectedActionType[0] == "Must") {
					isMandatory = true;
				} else {
					Debug.LogError("Unexpected option string: " + disectedActionType[0]);
					continue;
				}
				List<SelectableObjectType> actionTargets = new List<SelectableObjectType>();
				string targetString = nightSubactionComponents[1].Remove(nightSubactionComponents[1].IndexOf(')'));
				if(targetString != "") {
					string[] stringTargets = targetString.Split(',');
					for(int j = 0; j < stringTargets.Length; j++) {
						try {
							SelectableObjectType objectType = (SelectableObjectType)Enum.Parse(typeof(SelectableObjectType), stringTargets[j].Trim());
							actionTargets.Add(objectType);
						} catch (Exception e) {
							Debug.LogError("Unable to parse selectable object from: " + hiddenActionSeries);
						}
					}
				}
				ActionType actionType = (ActionType)Enum.Parse(typeof(ActionType), disectedActionType[1]);
				hiddenActions.Add(new SubAction(actionType, actionTargets, isMandatory));
			}
		}
		return hiddenActions;
	}

	private static WinRequirement ParseWinRequirementSeries(string winRequirementSeries) { //Series is nested in a single win requirement, with subsequent ones treated as fallbacks
		List<Selector> subjects = new List<Selector>();
		List<WinPredicate> predicates = new List<WinPredicate>();

		if(winRequirementSeries != "") {
			string[] winRequirementStrings = winRequirementSeries.Split(';');
			for(int i = 0; i < winRequirementStrings.Length; i++) {
				string[] subjectPredicate = winRequirementStrings[i].Split('_');
				Selector subject;
				if(subjectPredicate[0].StartsWith("Role.")) {
					subject = new Selector((Role)Enum.Parse(typeof(Role), subjectPredicate[0].Substring(5)));
				} else if(subjectPredicate[0].StartsWith("Nature.")) {
					subject = new Selector((Nature)Enum.Parse(typeof(Nature), subjectPredicate[0].Substring(7)));
				} else if(subjectPredicate[0].StartsWith("Relation.")) {
					subject = new Selector((Relation)Enum.Parse(typeof(Relation), subjectPredicate[0].Substring(9)));
				} else {
					Debug.LogError("Unhandled subject type: " + subjectPredicate[0]);
					subject = Selector.None;
				}
				WinPredicate predicate = (WinPredicate)Enum.Parse(typeof(WinPredicate), subjectPredicate[1]);
				subjects.Add(subject);
				predicates.Add(predicate);
			}
		}

		WinRequirement lastWinRequirement = null;
		for(int i = subjects.Count - 1; i >= 0 ; i--) {
			if(i == subjects.Count - 1) {
				lastWinRequirement = new WinRequirement(subjects[i], predicates[i], null);
			} else {
				lastWinRequirement = new WinRequirement(subjects[i], predicates[i], new WinRequirement[] { lastWinRequirement });
			}
		}
		return lastWinRequirement;

	}
}
