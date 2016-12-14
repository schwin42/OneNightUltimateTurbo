using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public interface ILocation
{
	int locationId { get; }
	RealCard currentCard { get; }
	string name { get; }
	//int currentMark { get; }
}








