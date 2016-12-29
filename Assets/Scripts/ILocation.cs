using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public interface ILocation
{
	int locationId { get; }
	RealCard currentCard { get; set; }
	string name { get; }
	//int currentMark { get; }
	//int currentArtifact
}








