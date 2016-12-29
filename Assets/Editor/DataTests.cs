using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

public class DataTests {

	[Test]
	public void CardDataLoaded()
	{
		Assert.AreEqual(44, GameData.instance.cardData.Count);
	}

	[Test]
	public void CardPoolLoaded()
	{
		Assert.AreEqual(48, GameData.instance.cardPool.Count);
	}
}
