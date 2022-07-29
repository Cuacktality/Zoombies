using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ZombieController : MonoBehaviour, IDamage {
    private NavMeshAgent agent;
    private Collider col;

    [Header ("HUD")]
    [SerializeField] private GameObject HUD;
    [SerializeField] private Image healthBar;

    [Header ("Health")]
    [Range (0, 100)][SerializeField] private float maxHealth = 100;
    public float ActualHealth { get; private set; }
    public float GetMaxHealth { get { return maxHealth; } }
    public void LoadLife (float value) { ActualHealth = value; }
    public bool IsDead { get; private set; }
    private Vector3 startPos;

    [Range (25, 100)][SerializeField] private int score;
    public int Score { get { return score; } }

    [Header ("AI - Agent")]
    [Range (1, 15)] public float walkSpeed = 5f;
    [Range (1, 20)] public float runSpeed = 6.2f;

    [Header ("AI - Patrol")]
    [SerializeField] private bool rndPath = true;
    [SerializeField] private Transform[] waypoint;
    [Range (1, 5)][SerializeField] private float minWaitTime = 1;
    [Range (5, 15)][SerializeField] private float maxWaitTime = 5;
    private int pointIndx = 0;
    private bool waiting;
    private float wait;

    [Header ("AI - Player")]
    [SerializeField] private PlayerActions pa;
    private bool playerDetected;
    public float DistToPlayer { get { return Vector3.Distance (transform.position, pa.transform.position); } }

    [Header ("AI - Look")]
    [Range (45, 120)] public float viewAngle = 90f;
    [Range (5, 120)] public float viewDist = 15f;
    private List<Collider> seenObj = new List<Collider> ();
    private RaycastHit seenHit;
    [SerializeField] private LayerMask mask;
    private bool cleaning = false;

    [Header ("AI - Attack")]
    [Range (2, 120)] public float maxAttackDist = 8;
    [Range (2, 120)] public float minAttackDist = 4f;
    public bool Retreating { get; private set; }
    public bool Attacking { get; private set; }

    [Header ("Ammo")]
    [HideInInspector] private float lastShot;
    [SerializeField] private float damage, fireRate, projSp;
    [SerializeField] private Rigidbody projectile;
    [SerializeField] private Transform projExt;

    private void Start () {
        ActualHealth = maxHealth;

        agent = GetComponent<NavMeshAgent> ();
        agent = GetComponent<NavMeshAgent> ();
        agent.angularSpeed = 360f;
        agent.speed = walkSpeed;
        agent.autoBraking = false;
        agent.stoppingDistance = 1f;

        col = GetComponent<Collider> ();
        startPos = transform.position;
    }
    private void Update () {
        Seek ();
        HUD.SetActive (false);

        if (!IsDead && waypoint.Length != 0 && !waiting && !playerDetected && agent.remainingDistance <= 1) StartCoroutine (Patrolling ());

    }
    private void LateUpdate () {
        UpdateLife ();
    }

    public void Damage (float value) {
        ActualHealth -= value;
    }
    private void UpdateLife () {
        healthBar.fillAmount = Mathf.Lerp (healthBar.fillAmount, (ActualHealth / maxHealth), .5f);
        ActualHealth = Mathf.Clamp (ActualHealth, 0, maxHealth);

        if (ActualHealth <= 0) {
            ScoreManager.Instance.actualScore += score;

            HUD.SetActive (false);
            StopAllCoroutines ();
            agent.enabled = false;
            col.enabled = false;
            transform.position = startPos;
            this.enabled = false;
            gameObject.SetActive (false);
        }
    }

    #region AI - Patrol
    private IEnumerator Patrolling () {

        wait = Random.Range (minWaitTime, maxWaitTime);
        agent.stoppingDistance = 1f;
        waiting = true;

        yield return new WaitForSeconds (wait);

        if (rndPath) {
            agent.destination = waypoint[pointIndx].transform.position;
            int nextPos;

            do nextPos = Random.Range (0, waypoint.Length);
            while (nextPos == pointIndx);

            pointIndx = nextPos;
        } else {
            agent.destination = waypoint[pointIndx].transform.position;
            pointIndx = (pointIndx + 1) % waypoint.Length;
        }

        waiting = false;
    }
    private void ResetPatrol () {

        Attacking = false;
        agent.speed = walkSpeed;
        agent.stoppingDistance = 1f;

        GoToNext ();
    }
    private void GoToNext () {

        if (rndPath) {
            agent.destination = waypoint[pointIndx].transform.position;
            int nextPos;

            do nextPos = Random.Range (0, waypoint.Length);
            while (nextPos == pointIndx);
            pointIndx = nextPos;

        } else {
            agent.destination = waypoint[pointIndx].transform.position;
            pointIndx = (pointIndx + 1) % waypoint.Length;
        }
    }
    #endregion

    #region AI - Algo
    private void Seek () {
        Collider[] seenCol = Physics.OverlapSphere (transform.position, viewDist);
        playerDetected = false;
        Collider obj = null;

        for (int i = 0; i < seenCol.Length; i += 1) {
            GameObject target = seenCol[i].gameObject;
            Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
            if (Vector3.Angle (transform.forward, dirToTarget) < viewAngle / 2) {
                //Debug.DrawRay (transform.position, dirToTarget * viewDist, Color.blue);

                if (Physics.Raycast (transform.position, dirToTarget, out seenHit, viewDist)) {
                    seenObj.Add (seenHit.collider);

                    if (seenHit.collider.GetComponent<PlayerActions> ()) {
                        pa = seenHit.collider.GetComponent<PlayerActions> ();
                        Attack (pa);
                        obj = seenHit.collider;
                    }
                }
                if (!cleaning) StartCoroutine (ClearSeen ());
            }
        }
        if (obj != null && (Vector3.Distance (transform.position, obj.transform.position) > maxAttackDist)) ResetPatrol ();
    }
    private IEnumerator ClearSeen () {
        cleaning = true;
        yield return new WaitForSecondsRealtime (1.25f);
        seenObj.Clear ();
        cleaning = false;
    }
    #endregion

    #region Combat
    private void Attack (PlayerActions ply) {
        if (!IsDead) {
            Attacking = playerDetected = true;
            agent.destination = ply.transform.position;
            agent.stoppingDistance = minAttackDist;

            Vector3 noRot = Vector3.zero;

            transform.LookAt (ply.transform);
            Shoot (agent.remainingDistance);

            Vector3 pos = pa.transform.position;
            agent.destination = pos;
            if (agent.transform.position == pos && !playerDetected) StartCoroutine (Patrolling ());
        }
    }
    public void Shoot (float trgtDist) {
        if (lastShot + (fireRate + .02f) < Time.time) {

            RaycastHit hit;
            Ray ray = new Ray (projExt.position, projExt.forward);
            Vector3 trgt;

            Rigidbody bullet = Instantiate (projectile, projExt.position, Quaternion.LookRotation (projExt.forward));

            if (Physics.Raycast (ray, out hit)) trgt = hit.point - projExt.position;
            else trgt = ray.GetPoint (viewDist);

            Bullet blt = bullet.GetComponent<Bullet> ();
            blt.Dad = transform;
            blt.Damage = damage;

            bullet.velocity = projSp * projExt.forward;
            //bullet.AddForce ((projSp / trgtDist) * projExt.forward, ForceMode.VelocityChange);

            lastShot = Time.time;
        }
    }
    #endregion

    #region Feedback
    public void ShowHUD (Transform showTo) {
        if (HUD != null) {
            HUD.SetActive (true);
            HUD.transform.LookAt (showTo);
        }
    }
    public void ResetAll () {
        playerDetected = false;
        Attacking = false;
        transform.position = startPos;
        ActualHealth = maxHealth;
        if (!gameObject.activeSelf) gameObject.SetActive (true);
    }
    #endregion   
}
