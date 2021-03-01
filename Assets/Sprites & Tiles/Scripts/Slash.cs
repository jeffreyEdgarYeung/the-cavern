using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : MonoBehaviour
{
    [SerializeField] float duration = .5f;
    Player player;

    void Start()
    {
        player = GameObject.Find("/Player").GetComponent<Player>();
        Destroy(gameObject, duration);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Sword Hit");
        player.Knockback();
    }
}