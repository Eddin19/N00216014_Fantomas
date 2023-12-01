using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour
{

    private PlayerController player;
    private Rigidbody2D rb;
    private SpriteRenderer sp;
    private Animator anim;
    private CinemachineVirtualCamera cm;
    private bool aplicarFuerza;

    public float distanciaDeteccionJugador = 17;
    public float distanciaDeteccionFlecha = 11;
    public GameObject flecha;
    public float fuerzaLanzamiento = 5;
    public float velocidadMovimiento;
    public int vidas = 3;
    public bool lanzarFlecha;


    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        sp = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        cm = GameObject.FindGameObjectWithTag("VirtualCamera").GetComponent<CinemachineVirtualCamera>();
    }   

    // Start is called before the first frame update
    void Start()
    {
        gameObject.name = "Skeleton";
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direccion = (player.transform.position - transform.position).normalized * distanciaDeteccionFlecha;
        Debug.DrawRay(transform.position, direccion, Color.red);

        float distanciaActual = Vector2.Distance(transform.position, player.transform.position);

        if (distanciaActual <= distanciaDeteccionFlecha)
        {
            rb.velocity = Vector2.zero;
            anim.SetBool("caminando", false);

            Vector2 direccionNormalizada = direccion.normalized;
            //CambiarVista(direccionNormalizada.x);
            if(!lanzarFlecha)
                StartCoroutine(LanzarFlecha(direccion,distanciaActual));
        }
        else
        {
            if(distanciaActual<= distanciaDeteccionJugador)
            {
                Vector2 movimiento = new Vector2(direccion.x, 0);
                movimiento = movimiento.normalized;
                rb.velocity = movimiento * velocidadMovimiento;
                anim.SetBool("caminando", true);
                CambiarVista(movimiento.x);
            }
            else
            {
                anim.SetBool("caminando", false);
            }
        }
    }

    private void CambiarVista(float direccionX)
    {
        if (direccionX < 0 && transform.localScale.x > 0)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        } else if (direccionX > 0 && transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position,distanciaDeteccionJugador);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccionFlecha);
    }

    private IEnumerator LanzarFlecha(Vector2 direccionFlecha, float distancia)
    {
        lanzarFlecha = true;
        anim.SetBool("disparando", true);
        yield return new WaitForSeconds(1.42f);
        direccionFlecha = direccionFlecha.normalized;

        GameObject nuevaFlecha = Instantiate(flecha, transform.position, Quaternion.identity);
        nuevaFlecha.transform.GetComponent<Flecha>().direccionFlecha = direccionFlecha;
        nuevaFlecha.transform.GetComponent<Flecha>().esqueleto = this.gameObject;

        nuevaFlecha.transform.GetComponent<Rigidbody2D>().velocity = direccionFlecha * fuerzaLanzamiento;
        lanzarFlecha = false;
    }

    public void RecibirDaño()
    {
        if (vidas > 0)
        {
            StartCoroutine(EfectoDaño());
            StartCoroutine(AgitarCamara(0.1f));
            aplicarFuerza = true;
            vidas--;
        }
        else
        {
            StartCoroutine(AgitarCamara(0.1f));
            velocidadMovimiento = 0;
            rb.velocity = Vector2.zero;
            Destroy(gameObject, 0.2f);
        }
    }

    private IEnumerator AgitarCamara(float tiempo)
    {
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin = cm.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();//referencia
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 5;//amplitud del shake
        yield return new WaitForSeconds(tiempo);
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0;
    }

    private IEnumerator EfectoDaño()
    {
        sp.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        sp.color = Color.white;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            player.RecibirDaño((transform.position - player.transform.position).normalized);
        }
    }

    private void FixedUpdate()
    {
        if (aplicarFuerza)
        {
            rb.AddForce((transform.position - player.transform.position).normalized * 100, ForceMode2D.Impulse);
            aplicarFuerza = false;
        }
    }
    
}
