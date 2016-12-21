using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEditor;

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

	public List<Card> cardData = new List<Card>();
	public List<Role> rolePool = new List<Role>();

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

		instance.cardData = new List<Card>();
		instance.rolePool = new List<Role>();
		foreach(Dictionary<string, string> dict in roleDicts) {
			Role cardRole = ((Role)Enum.Parse(typeof(Role), dict["Role"].Replace(" ", "")));
//			Team cardTeam = ((Team)Enum.Parse(typeof(Team), dict["Team"]) as Team);
//			Nature cardNature = ((Nature)Enum.Parse(typeof(Nature), dict["Nature"]));
			Role cardSeedRequirement = Role.None;
			try {
				cardSeedRequirement = ((Role)Enum.Parse(typeof(Role), dict["SeedRequirement"]));
			} catch (Exception e) {

			}
			int cardMaxQuantity = int.Parse(dict["MaxQuantity"]);
			Card card = new Card(cardRole) {
//				team = cardTeam,
//				nature = cardNature,
//				public virtual WinRequirement[] winRequirements { get { return team.winRequirements; } }
//				public Order order = Order.None;
//				public CohortType cohort = CohortType.None;
//				public Prompt promptIfCohort = null;
//				public Prompt prompt = null;
//				public NightAction[] nightActions = new NightAction[] { };
//				public NightAction[] nightActionsIfCohort = new NightAction[] { };
				seedRequirement = cardSeedRequirement,
				maxQuantity = cardMaxQuantity,
			};
			instance.cardData.Add(card);
			for(int i = 0; i < card.maxQuantity; i++) {
				instance.rolePool.Add(card.role);
			}
		}
	}
}
