using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonsController : MonoBehaviour
{
    public GameObject pausarPanelJuego;
    public GameObject botonPausar;
    public void PausarJuego()
    {
        Time.timeScale = 0;
        pausarPanelJuego.SetActive(true);
        botonPausar.SetActive(false);
    }

    public void ContinuarJuego()
    {
        Time.timeScale = 1;
        pausarPanelJuego.SetActive(false);
        botonPausar.SetActive(true);
    }

    public void RegresarMenuInicial()
    {
        SceneManager.LoadScene("MenuPrincipal");
    }
}
