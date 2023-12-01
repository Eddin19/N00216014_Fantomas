using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public GameObject vidaRecuperable1;
    public GameObject vidaRecuperable2;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 direccion;
    private CinemachineVirtualCamera cm;
    private Vector2 direccionMovimiento;
    private Vector2 direccionDaño;
    private bool bloqueado;
    private GrayCamera gc;
    private SpriteRenderer sprite;

    [Header("Estadisticas")]
    public float velocidadDemovimiento = 10;
    public float fuerzaDeSalto = 5;
    public float velocidadDash = 20;
    public float velocidadDeslizar;
    public int vidas;
    public float tiempoInmortalidad;

    [Header("Collisiones")]
    public LayerMask layerPiso;
    public float radioDeColision;
    public Vector2 abajo, derecha, izquierda;

    [Header("Booleanos")]
    public bool puedeMover = true;
    public bool enSuelo = true;
    public bool puedeDash;
    public bool haciendoDash;
    public bool tocadoPiso;
    public bool haciendoShake;
    public bool estaAtacando;
    public bool enMuro;
    //public bool muroDerecho;
    //public bool muroIzquierdo;
    public bool agarraese;
    //public bool saltarDeMuro;
    public bool esInmortal;
    private bool aplicarFuerza;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();//referencia
        anim = GetComponent<Animator>();
        cm = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
        gc = Camera.main.GetComponent<GrayCamera>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public void SetBloqueadoTrue()
    {
        bloqueado = true;
    }

    public void Morir()
    {
        if (vidas > 0) 
        {
            return;
        }else if (vidas == 0)
        {
            SceneManager.LoadScene("GameOver");
        }    

        this.enabled = false;
    }

    public void RecibirDaño()
    {
        StartCoroutine(ImpoctoDaño(Vector2.zero));
    }

    public void RecibirDaño(Vector2 direccionDaño)
    {
        StartCoroutine(ImpoctoDaño(direccionDaño));
    }

    private IEnumerator ImpoctoDaño(Vector2 direccionDaño)
    {
        if (!esInmortal)
        {
            StartCoroutine(Inmortalidad());
            vidas--;
            gc.enabled = true;
            float velocidadAuziliar = velocidadDemovimiento;
            this.direccionDaño = direccionDaño;
            aplicarFuerza = true;
            Time.timeScale = 0.4f;
            FindObjectOfType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));
            StartCoroutine(AgitarCamara());
            yield return new WaitForSeconds(0.2f);
            Time.timeScale = 1;
            gc.enabled = false;

            for (int i = GameManager.instance.vidasUI.transform.childCount - 1; i >= 0; i--)
            {
                if(GameManager.instance.vidasUI.transform.GetChild(i).gameObject.activeInHierarchy)
                {
                    GameManager.instance.vidasUI.transform.GetChild(i).gameObject.SetActive(false);

                    break;
                }
            }
            velocidadDemovimiento = velocidadAuziliar;
            Morir();

        }
    }

    private void FixedUpdate()
    {
        if (aplicarFuerza)
        {
            velocidadDemovimiento = 0;
            rb.velocity = Vector2.zero;
            rb.AddForce(-direccionDaño * 25, ForceMode2D.Impulse);
            aplicarFuerza = false;
        }
    }

    public void DarInmortalidad()
    {
        StartCoroutine(Inmortalidad());
    }

    private IEnumerator Inmortalidad()
    {
        esInmortal = true;

        float tiempoTranscurrido = 0;

        while (tiempoTranscurrido < tiempoInmortalidad)
        {
            sprite.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(tiempoInmortalidad/20);
            sprite.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(tiempoInmortalidad/20);
            tiempoTranscurrido += tiempoInmortalidad / 10;
        }
        esInmortal = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        vidas = 3;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Vidas(Update): " + vidas);

        Movimiento();
        Agarres();
        RecuperarV();
        ActivarRecuperarVida();
    }
    private void RecuperarV()
    {
        if (vidas == 3)
        {
            Debug.Log("activar vida 3");
            GameManager.instance.vidasUI.transform.GetChild(2).gameObject.SetActive(true);
        }
        else if (vidas == 2)
        {
            Debug.Log("activar vida 2");
            GameManager.instance.vidasUI.transform.GetChild(1).gameObject.SetActive(true);
        }
    }

    public void ActivarRecuperarVida()
    {
        if (vidas == 2)
        {
            Debug.Log("activar vida < 3");
            vidaRecuperable1.SetActive(true);
        }
        else if (vidas == 1)
        {
            Debug.Log("activar vida < 2");
            vidaRecuperable2.SetActive(true);
        }
    }

    public void PlayerDañoVida()
    {
        vidas--;
    }

    public void PlayerRecuperarVida()
    {
        Debug.Log("public void PlayerRecuperarVida(IncrementoVida)");
        vidas++;
    }

    private void Atacar(Vector2 direccion)
    {
        if (Input.GetKeyDown(KeyCode.Z))//ataque basico
        {
            if (!estaAtacando && !haciendoDash)//si no esta atacando y no esta haciendo dash
            {
                estaAtacando = true;
                anim.SetFloat("ataqueX", direccion.x);
                anim.SetFloat("ataqueY", direccion.y);

                anim.SetBool("atacar", true);
            }
        }
    }

    public void FinalizarAtaque()
    {
        anim.SetBool("atacar", false);
        bloqueado = false;
        estaAtacando = false;
    }

    private Vector2 DireccionAtaque(Vector2 direccionMovimiento, Vector2 direccion)//direccion de ataque
    {
        if (rb.velocity.x == 0 && direccion.y != 0)//si no se mueve en x y se mueve en y
            return new Vector2(0, direccion.y);//devuelve la direccion en y

        return new Vector2(direccionMovimiento.x, direccion.y);//devuelve la direccion en x
    }

    private IEnumerator AgitarCamara()
    {
        haciendoShake = true;//para que no se pueda hacer otro shake
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();//referencia
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 5;//amplitud del shake
        yield return new WaitForSeconds(0.3f);//tiempo de duracion del shake
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;//se vuelve a 0
        haciendoShake = false;
    }
    private IEnumerator AgitarCamara(float tiempo)//para el dash
    {
        haciendoShake = true;
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();//referencia
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 5;//amplitud del shake
        yield return new WaitForSeconds(tiempo);
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
        haciendoShake = false;
    }

    private void Dash(float x, float y)
    {
        anim.SetBool("dash", true);
        Vector3 posicionJugador = Camera.main.WorldToViewportPoint(transform.position);//posicion del jugador
        Camera.main.GetComponent<RippleEffect>().Emit(posicionJugador);//   efecto de onda
        StartCoroutine(AgitarCamara());

        puedeDash = true;
        rb.velocity = Vector2.zero;//para que no se mueva en el dash
        rb.velocity += new Vector2(x, y).normalized * velocidadDash;//velocidad del dash
        StartCoroutine(PrepararDash());//para que no pueda hacer dash en el aire
    }

    private IEnumerator PrepararDash()
    {
        StartCoroutine(DashSuelo());

        rb.gravityScale = 0;//para que no caiga
        haciendoDash = true;//para que no pueda hacer dash en el aire

        yield return new WaitForSeconds(0.3f);//tiempo de duracion del dash

        rb.gravityScale = 3;//para que caiga
        haciendoDash = false;//para que pueda hacer dash en el aire
        FinalizarDash();
    }

    private IEnumerator DashSuelo()
    {
        yield return new WaitForSeconds(0.15f);//tiempo de duracion del dash
        if (enSuelo)//si esta en el suelo
            puedeDash = false;
    }

    public void FinalizarDash()
    {
        anim.SetBool("dash", false);
    }

    private void TocarPiso()
    {
        puedeDash = false;
        haciendoDash = false;
        anim.SetBool("saltar", false);
    }

    private void Movimiento()
    {
        // los valores en GetAxis van de -1 a 1 
        float x = Input.GetAxis("Horizontal");//para que se mueva en x
        float y = Input.GetAxis("Vertical");//para que se mueva en y

        //los valores de GetAxisRaw van de -1 a 0 a 1
        float xRaw = Input.GetAxisRaw("Horizontal");//para que se mueva en x
        float yRaw = Input.GetAxisRaw("Vertical");//    para que se mueva en y

        direccion = new Vector2(x, y);
        Vector2 direccionRaw = new Vector2(xRaw, yRaw);//para el dash

        Caminar();
        Atacar(DireccionAtaque(direccionMovimiento, direccionRaw));//direccion de ataque

       /* if(enSuelo && !haciendoDash)
        {
            saltarDeMuro = false;
        }*/

        //agarraese = enMuro && Input.GetKey(KeyCode.LeftShift);

        if (agarraese && !enSuelo)
        {
            anim.SetBool("escalar", true);
            if (rb.velocity == Vector2.zero)
            {
                
                anim.SetFloat("velocidad", 0);
            }
            else
            {
                anim.SetFloat("velocidad", 1);
            }

        }
        else
        {
            anim.SetBool("escalar", false);
            anim.SetFloat("velocidad", 0);
        }
        
       /* if (agarraese && !haciendoDash)
        {
            rb.gravityScale = 0;
            if (x > 0.2f || x < -0.2f)
                rb.velocity = new Vector2(rb.velocity.x, 0);

            float modificadorDeVelocidad = y > 0 ? 0.5f : 1;
            rb.velocity = new Vector2(rb.velocity.x, y * (velocidadDemovimiento * modificadorDeVelocidad));

            if(muroIzquierdo && transform.localScale.x > 0)
            {
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }else if(muroDerecho && transform.localScale.x < 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }else
        {
            rb.gravityScale = 3;
        }

        if(enMuro &&  !enSuelo)
        {
            anim.SetBool("escalar", true);
            if (x != 0 &&  !agarraese)
                DeslizarPared();
        }*/
       
        MejorarSalto();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (enSuelo)
            {
                anim.SetBool("saltar", true);
                Saltar();
            }

           /* if (enMuro && !enSuelo)
            {
                anim.SetBool("escalar", false);
                anim.SetBool("saltar", true);
                SaltarDesdeMuro();
            }*/
        }

        if (Input.GetKeyDown(KeyCode.X) && !haciendoDash && !puedeDash)
        {
            if (xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (enSuelo && !tocadoPiso)
        {
            anim.SetBool("escalar", false);
            TocarPiso();
            tocadoPiso = true;
        }

        if (!enSuelo && tocadoPiso)
            tocadoPiso = false;

        float velocidad;
        if (rb.velocity.y > 0)
            velocidad = 1;
        else
            velocidad = -1;

        if (!enSuelo)
        {

            anim.SetFloat("velocidadVertical", velocidad);
        }
        else
        {
            if (velocidad == -1)
                FinalizarSalto();
        }

    }

    private void DeslizarPared()
    {
        if(puedeMover)
            rb.velocity = new Vector2(rb.velocity.x, -velocidadDeslizar);
    }

   /* private void SaltarDesdeMuro()
    {
        StopCoroutine(DeshabilitarMovimiento(0));
        StartCoroutine(DeshabilitarMovimiento(0.1f));

        //Vector2 direccionMuro = muroDerecho ? Vector2.left : Vector2.right;

        if (direccionMuro.x < 0 && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        }
        else if(direccionMuro.x > 0 && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        anim.SetBool("saltar", true);
        anim.SetBool("escalar", false);
        Saltar((Vector2.up + direccionMuro), true);

        saltarDeMuro = true;
    }*/

    private IEnumerator DeshabilitarMovimiento(float tiempo)
    {
        puedeMover = false;
        yield return new WaitForSeconds(tiempo);
        puedeMover = true;
    }


    public void FinalizarSalto()
    {
        anim.SetBool("saltar", false);
    }

    private void MejorarSalto()
    {
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;// cuadno esta 0
        }
        else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (2.0f - 1) * Time.deltaTime;
        }

    }

    private void Agarres()
    {
        enSuelo = Physics2D.OverlapCircle((Vector2)transform.position + abajo, radioDeColision, layerPiso);

        Collider2D collisionDerecha = Physics2D.OverlapCircle((Vector2)transform.position + derecha, radioDeColision, layerPiso);
        Collider2D collisionIzquierda = Physics2D.OverlapCircle((Vector2)transform.position + izquierda, radioDeColision, layerPiso);

        if (collisionDerecha != null)
        {
            enMuro = !collisionDerecha.CompareTag("Plataforma");

        }else if(collisionIzquierda != null)
        {
            enMuro = !collisionIzquierda.CompareTag("Plataforma");
        }
        else
        {
            enMuro = false;
        }

        /*muroDerecho = Physics2D.OverlapCircle((Vector2)transform.position + derecha, radioDeColision, layerPiso);
        muroIzquierdo = Physics2D.OverlapCircle((Vector2)transform.position + izquierda, radioDeColision, layerPiso);
        enMuro = muroDerecho || muroIzquierdo;*/
    }

    
    private void Saltar()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += Vector2.up * fuerzaDeSalto;
    }

    private void Saltar(Vector2 direccionSalto, bool muro)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += direccionSalto * fuerzaDeSalto;
    }


    private void Caminar()
    {
        if (puedeMover && !haciendoDash && !estaAtacando)
        {
           /* if (saltarDeMuro)
            {
                    rb.velocity =  Vector2.Lerp(rb.velocity, 
                        (new Vector2(direccionMovimiento.x * velocidadDemovimiento, rb.velocity.y)), Time.deltaTime / 2);
            }*/
            //else
          //  {

                if (direccion != Vector2.zero && !agarraese)
                {
                    if (!enSuelo)
                    {
                        anim.SetBool("saltar", true);
                    }
                    else
                    {
                        anim.SetBool("caminar", true);
                    }
                    rb.velocity = new Vector2(direccion.x * velocidadDemovimiento, rb.velocity.y);
                    if (direccion.x < 0 && transform.localScale.x > 0)
                    {
                        direccionMovimiento = DireccionAtaque(Vector2.left, direccion);//-1 hacia la izquierda
                        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                    }
                    else if (direccion.x > 0 && transform.localScale.x < 0)
                    {
                        direccionMovimiento = DireccionAtaque(Vector2.right, direccion);//-1 hacia la derecha
                        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
                    }
                }
                else
                {
                    if (direccion.y > 0 && direccion.x == 0)
                    {
                        direccionMovimiento = DireccionAtaque(direccion, Vector2.up);
                    }
                    anim.SetBool("caminar", false);
                }
            //}
        }
        else
        {
            if (bloqueado)
            {
                FinalizarAtaque();
            }
        }
    }
}