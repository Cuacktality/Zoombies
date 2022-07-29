using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {
    private PlayerActions pa;
    private CharacterController cc;
    private CapsuleCollider cap;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform headBone;

    [Header ("Movement settings")]
    [Tooltip ("Velocidad frontal")][Range (0, 30)][SerializeField] private float frontSP = 6f;
    [Tooltip ("Velocidad de espalda")][Range (0, 30)][SerializeField] private float backSP = 4f;
    [Tooltip ("Velocidad lateral")][Range (0, 30)][SerializeField] private float sideSP = 3f;
    [Tooltip ("Fuerza de salto")][Range (0, 20)][SerializeField] private float jumpForce = 10;
    [SerializeField] private Vector2 sens;
    [HideInInspector] private float actualSP;
    [HideInInspector] private Vector3 move;
    [HideInInspector] private float xRot = 0;
    [HideInInspector] private float mouseX, mouseY;

    [Header ("Run settings")]
    [Tooltip ("Stamina")][Range (10, 100)][SerializeField] private float maxStamina = 100;
    [Tooltip ("Cantidad de disminución de la Stamima")][Range (0, 20)][SerializeField] private int decreaseStam = 10;
    [Tooltip ("Multiplicador de esprintar")][Range (0, 5)] public int runTimes = 2;
    [Tooltip ("Incremento de recarga de la stamina")][Range (1, 10)][SerializeField] private int stamIncrement;
    [HideInInspector] private float stamina;
    private WaitForSeconds staminaTick = new WaitForSeconds (.1f);
    [SerializeField] private Image staminaBar;
    private Coroutine stamining;

    [Header ("Advanced settings")]
    [Tooltip ("Gravedad")][Range (0, 20)][SerializeField] private float gravity = 9.8f;

    [Header ("Damage by fall")]
    [Range (1, 45)][SerializeField] private int fallTreshold = 20;
    [Range (1, 5)][SerializeField] private int fallMultiplier = 2;
    [HideInInspector] private float startPos, endPos;
    [HideInInspector] private bool firstCall, canDamage;
    [HideInInspector] private float fallSp = 0;

    [Header ("Slopes")]
    [Range (-15, 15f)][SerializeField] private float sVel;
    [Range (-15, 15f)][SerializeField] private float sForce;
    [HideInInspector] private bool slope = false;
    [HideInInspector] private Vector3 normal;

    private void Start () {
        pa = GetComponent<PlayerActions> ();
        cc = GetComponent<CharacterController> ();
        cap = GetComponent<CapsuleCollider> ();

        stamina = maxStamina;
        staminaBar.fillAmount = (stamina / maxStamina);
    }

    private void Update () {

    }

    #region Player Movement
    public void SetSpeed (float x, float y, bool run) {
        if ((x == 0) || (y == 0)) return;
        if (x > 0 || y < 0) actualSP = sideSP;

        if (y < 0) actualSP = backSP;
        if (y > 0) {
            actualSP = frontSP;
            if (run) {
                if (stamining != null) {
                    StopCoroutine (stamining);
                    stamining = null;
                }

                actualSP *= runTimes;
                stamina -= decreaseStam * Time.deltaTime;
            }
        }
        if (stamina < maxStamina) stamining = StartCoroutine (RegenStamina ());
        staminaBar.fillAmount = (stamina / maxStamina);
    }
    public void Rotate (float x, float y) {
        mouseX = x * sens.x;
        mouseY = y * sens.y;

        xRot -= mouseY;
        xRot = Mathf.Clamp (xRot, -40, 32f);
        transform.Rotate (Vector3.up, mouseX);
        cam.transform.localRotation = Quaternion.Euler (xRot, 0, 0);

    }
    public void Move (float x, float z, bool jump, bool run) {
        SetSpeed (x, z, run);

        if (cc.isGrounded) {
            move = transform.right * x + transform.forward * z;
            move *= actualSP;
            if (jump) fallSp = jumpForce - gravity * Time.deltaTime;

        } else fallSp -= (gravity * 1.5f) * Time.deltaTime;
        move.y = fallSp;

        Slopes ();
        FallDamage ();
        cc.Move (move * Time.deltaTime);
    }
    private void FallDamage () {
        if (!cc.isGrounded) {
            if (transform.position.y > startPos) firstCall = true;

            if (firstCall) {
                startPos = transform.position.y;
                firstCall = false;
                canDamage = true;
            }
        } else {
            endPos = transform.position.y;
            if (startPos - endPos > fallTreshold) {
                if (canDamage) {
                    float value = (-cc.velocity.y + (startPos - endPos)) * fallMultiplier;
                    pa.Damage (value);
                    canDamage = false;
                    startPos = endPos = 0;
                }
            }
        }
    }

    private void Slopes () {

        if (cc.collisionFlags != CollisionFlags.Below) return;

        slope = Vector3.Angle (Vector3.up, normal) >= cc.slopeLimit;
        if (slope) {
            move.x -= ((.0015f - normal.y) * normal.x) * sVel;
            move.z -= ((.0015f - normal.y) * normal.z) * sVel;
            move.y -= sForce + (gravity * 1.5f) * Time.deltaTime;
        }
    }
    #endregion 

    private void OnControllerColliderHit (ControllerColliderHit hit) {
        normal = hit.normal;
    }
    private IEnumerator RegenStamina () {
        yield return new WaitForSeconds (10);

        while (((int) stamina) < maxStamina) {
            stamina += stamIncrement;
            yield return staminaTick;
        }
        stamining = null;
    }
}
