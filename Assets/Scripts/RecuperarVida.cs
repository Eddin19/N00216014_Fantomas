using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecuperarVida : MonoBehaviour
{
    public PlayerController playerController;

    private void OnTriggerEnter2D(Collider2D other)
    {
        int vida = playerController.vidas;
        if (other.CompareTag("Player") && vida < 3)
        {
            playerController.PlayerRecuperarVida();
            Debug.Log("OnTrigger");
            Debug.Log(vida);
            Destroy(this.gameObject);
        }
    }

}
