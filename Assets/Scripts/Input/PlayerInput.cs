using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour {

    public PlayerActions Actions;

    private void Start() {
       Actions = PlayerActions.CreateWithDefaultBindings();
    }

    private void Awake() {
        
    }
}
