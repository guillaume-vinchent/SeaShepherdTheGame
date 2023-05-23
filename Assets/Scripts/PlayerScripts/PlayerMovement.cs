using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Player Movement")]
    public float moveSpeed = 3f;
    private float currentSpeed;
    public float shiftSpeed = 4f;
    public Vector3 directionInput;
    private Vector3 movement;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float turnSmoothVelocity = 0.1f;

    [Header("Flotaison")]
    [Tooltip("Hauteur de flotaison")]
    [SerializeField] private float height = 0.1f;
    [Tooltip("Rapidit� d'une p�riode de flotaison")]
    [SerializeField] private float timer = 1;

    private Vector3 initialPos;
    private float offset;
    private float playerY;

    [Header("Boost")]
    [SerializeField] private float boostRegen = 5f;
    [SerializeField] private bool stopBoost;
    [SerializeField] private bool boostRegenAvailable;
    [SerializeField] private float timerBoost = 5f;

    [Header("Player Component")]
    public Camera cam;
    private PlayerInputManager playerInput;
    private Rigidbody rb;
    #endregion

    private void Awake()
    {
        playerInput = GetComponent<PlayerInputManager>();
        rb = GetComponent<Rigidbody>();

        initialPos = transform.position;

        offset = 1 - (Random.value * 2);
    }

    private void Start()
    {
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        Locomotion();

        BoostManager();
    }

    private void FixedUpdate()
    {
        rb.velocity = new Vector3(movement.x * 10, playerY, movement.z * 10);
    }

    /// <summary>
    /// Gere le deplacement du personnage avec le character controller
    /// </summary>
    public void Locomotion()
    {
        if (!playerInput) return;

        directionInput.Set(playerInput.MoveInput.x, 0, playerInput.MoveInput.y);

        // Joueur regarde dans la direction o� il se d�place
        if (directionInput.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(directionInput.x, directionInput.z) * Mathf.Rad2Deg +
                cam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,
                ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            directionInput = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
        else if (directionInput.magnitude < 0.1f)
        {
            turnSmoothVelocity = 0;
            rb.velocity = Vector3.zero;
            FloatingEffect();
        }


        if (!playerInput.CanShift)
        {
            // Je n'ai pas mon boost d'activ�
            movement = directionInput.normalized * (currentSpeed * Time.deltaTime);
        }
        else if (playerInput.CanShift)
        {
            // Tant que j'ai du boost je l'utilise
            if (!stopBoost) movement = directionInput.normalized * (shiftSpeed * Time.deltaTime);
            else if (stopBoost) movement = directionInput.normalized * (currentSpeed * Time.deltaTime);
        }
    }

    private void BoostManager()
    {
        if (playerInput.CanShift && !stopBoost)
        {
            timerBoost = 5f;

            // Boost Activ�
            if (boostRegen > 0) boostRegen -= Time.deltaTime;

            if (boostRegen <= 0)
            {
                // Si boost utilis� pendant 5 secondes plus de boost
                stopBoost = true;
            }
        }

        if (!playerInput.CanShift && boostRegen < 5)
        {
            // Si boost pas activ� on patiente 5 secondes
            if (timerBoost > 0) timerBoost -= Time.deltaTime;
            else if (timerBoost <= 0)
            {
                // Au bout des 5 secondes on redonne du boost au joueur
                boostRegen += Time.deltaTime * 1.25f;
                boostRegenAvailable = false;
                stopBoost = false;
            }
        }
    }

    /// <summary>
    /// Permet de simuler un effet de flotaison sur les objets
    /// </summary>
    private void FloatingEffect()
    {
        playerY = initialPos.y * Mathf.Sin((Time.time + offset) * timer) * height;
    }
}