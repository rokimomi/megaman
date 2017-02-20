using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegabusterBlastController : MonoBehaviour {

    public Vector2 direction = new Vector2(1, 0);
    public float speed = 0.2f;
    public int activeBusterShots;
    public Player player;

	void Start () {
        // ensure the object gets destroyed eventually, prevents leaks
        Destroy(gameObject, 20); 
    }

    void Update () {
        transform.Translate(direction * speed);
    }

    void OnBecameInvisible() {
        Destroy(gameObject);
    }

    void OnDestroy() {
        player.activeBusterShots--;
    }

}
