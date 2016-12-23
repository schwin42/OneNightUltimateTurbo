﻿using UnityEngine;
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
		foreach(Dictionary<string, string> dict in roleDicts) {
			Role cardRole = ((Role)Enum.Parse(typeof(Role), dict["Role"].Replace(" ", "")));
//			Team cardTeam = ((Team)Enum.Parse(typeof(Team), dict["Team"]) as Team);
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
			int cardMaxQuantity = int.Parse(dict["MaxQuantity"]);
			CardData card = new CardData(cardRole) {
//				team = cardTeam,
				nature = cardNature,
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
				instance.cardPool.Add(card);
			}
		}
	}
}