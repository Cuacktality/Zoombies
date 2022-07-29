 using System.Collections;
 using TMPro;
 using UnityEngine.UI;
 using UnityEngine;

 public class PlayerActions : MonoBehaviour, IDamage {
     [SerializeField] private Camera cam;
     private PlayerInput pi;
     public Camera Camera { get { return cam; } }

     [Header ("Health")]
     [Tooltip ("Vida máxima")][Range (0, 100)][SerializeField] private float maxHealth = 100;
     public float ActualHealth { get; private set; }
     public float GetMaxHealth { get { return maxHealth; } }
     public void LoadLife (float value) { ActualHealth = value; }
     public bool IsDead { get; private set; }
     private Vector3 startPos;

     [Header ("Health UI")]
     [SerializeField] private Image life;
     [SerializeField] private GameObject damage;

     [Header ("Score UI")]
     [SerializeField] private TMP_Text score;

     [Header ("Interact")]
     [SerializeField] private LayerMask mask;

     private void Start () {
         ActualHealth = maxHealth;
         pi = GetComponent<PlayerInput> ();
         startPos = transform.position;
     }

     private void LateUpdate () {
         UpdateHealth ();
         SendCast ();
         UpdateScore ();
     }

     public void Damage (float value) {
         ActualHealth -= value;
         StartCoroutine (DamageFeedback ());
     }
     private void UpdateHealth () {
         ActualHealth = Mathf.Clamp (ActualHealth, 0, maxHealth);
         life.fillAmount = Mathf.Lerp (life.fillAmount, (ActualHealth / maxHealth), .5f);

         damage.SetActive (false);
         if (ActualHealth < 35f) damage.SetActive (true);

         if (ActualHealth <= 0 && !IsDead) {
             IsDead = true;
             ScoreManager.Instance.ShowScore ();
             pi.enabled = false;
         }
     }
     private IEnumerator DamageFeedback () {
         damage.SetActive (true);
         yield return new WaitForSeconds (.4f);
         damage.SetActive (false);
     }

     private void SendCast () {
         RaycastHit enHit;

         if (Physics.Raycast (cam.transform.position, cam.transform.forward, out enHit, 25, mask)) {
             float targetDist = Vector3.Distance (transform.position, enHit.point);

             ZombieController en = enHit.collider.GetComponent<ZombieController> ();
             if (en != null) en.ShowHUD (transform);
         }
     }
     private void UpdateScore () {
         score.text = "Score: " + ScoreManager.Instance.actualScore.ToString ();
     }

     public void ResetAll () {
         pi.enabled = true;
         ActualHealth = maxHealth;
         transform.position = startPos;
     }
 }
