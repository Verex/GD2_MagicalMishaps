using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkCharacter {

	[Command]
	private void RequestMove(Vector2 direction)
	{

	}

	new private void Start()
	{
		base.Start();

		
	}

	protected override IEnumerator UpdateCharacter()
	{

		yield return null;
	}

}
