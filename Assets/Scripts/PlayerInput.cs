using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Player))]
public class PlayerInput : MonoBehaviour {

	Player player;

	void Start () {
		player = GetComponent<Player> ();
	}

	void Update () {

		Vector2 directionalInput = new Vector2 (Input.GetAxisRaw ("Horizontal"), Input.GetAxisRaw ("Vertical"));
		player.SetDirectionalInput (directionalInput);

		if (Input.GetKeyDown (KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton1)) {
			player.OnJumpInputDown ();
		}
		if (Input.GetKeyUp (KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton1)) {
			player.OnJumpInputUp ();
		}
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.JoystickButton2)) {
            player.OnShootInputDown();
        }

    }
}
