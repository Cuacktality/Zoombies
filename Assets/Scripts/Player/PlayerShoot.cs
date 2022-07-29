 using System.Collections;
 using TMPro;
 using UnityEngine.UI;
 using UnityEngine;

 /// <summary>
 /// Player Shoot se encarga de gestionar el HUD con el conteo de las balas, los íconos del arma y demás información.
 /// Además permite al jugador disparar, recargar y cambiar el arma.
 /// </summary>

 [DisallowMultipleComponent]
 public class PlayerShoot : MonoBehaviour {

     private PlayerActions pa;
     private Camera cam;
     private AudioSource au;

     [Header ("Weapons")]
     public Weapon weapon;
     [Space]
     [SerializeField] Transform socket;
     public GameObject gunMesh { get; private set; }
     public int indx { get; set; } = 0;
     public bool Reloading { get; private set; } = false;

     [Header ("HUD")]
     [SerializeField] private GameObject[] slots = new GameObject[3];

     [Header ("Primary weapon")]
     [SerializeField] private TMP_Text pmryAmmo;
     [SerializeField] private TMP_Text pmryBullets;
     private Transform weaponHole;
     protected const string HOLE = "Hole";

     [Header ("Advanced")]
     [SerializeField] private LayerMask mask;
     [SerializeField] private bool AutoLoad = false;
     [HideInInspector] private int residual;
     [HideInInspector] private float lastShot, damg;
     private Coroutine reloading;

     private void Start () {
         pa = GetComponent<PlayerActions> ();
         cam = pa.Camera;
         if (gunMesh != null) Destroy (gunMesh);
         if (weapon != null) InstantiateGun (indx);

         weapon.ammo = weapon.maxMag;
         weapon.bullets = weapon.maxBullets;
     }
     private void LateUpdate () {
         UpdateWeapon ();

         if (!Reloading && weapon.ammo == 0 && weapon.bullets > 0 && AutoLoad) Invoke ("Reload", .55f);
     }

     public void Shot (bool shot) {
         if ((shot) && (weapon.ammo > 0) && lastShot + weapon.fireRate < Time.time && !Reloading) {
             ShootRifle ();
             weapon.ammo--;
             lastShot = Time.time;
         }
     }

     public void Reload () {
         if (!Reloading) StartCoroutine (ReloadWeapon ());
     }

     private IEnumerator ReloadWeapon () {
         if (weapon.bullets > 0) {
             if ((weapon.ammo + weapon.bullets) <= weapon.maxMag) {

                 Reloading = true;
                 yield return new WaitForSeconds (weapon.tReload);

                 weapon.ammo += weapon.bullets;
                 weapon.bullets -= weapon.bullets;
                 Reloading = false;

             } else if (weapon.ammo < weapon.maxMag) {
                 residual = weapon.maxMag - weapon.ammo;

                 Reloading = true;
                 yield return new WaitForSeconds (weapon.tReload);

                 weapon.ammo += residual;
                 weapon.bullets -= residual;
                 Reloading = false;
             }
         }
     }
     private void InstantiateGun (int ind) {
         if (gunMesh != null) Destroy (gunMesh);

         GameObject mesh = Instantiate (weapon.mesh, socket.position, Quaternion.Euler (socket.eulerAngles.x, socket.eulerAngles.y, socket.eulerAngles.z));
         mesh.transform.parent = socket;
         weaponHole = mesh.transform.Find (HOLE);
         gunMesh = mesh;

         socket.localRotation = Quaternion.Euler (Vector3.zero);

     }
     private void UpdateWeapon () {

         if (weapon != null) {
             slots[0].SetActive (true);

             socket.localRotation = Quaternion.Slerp (socket.localRotation, Quaternion.Euler (new Vector3 (0, 0, 0)), 5 * Time.deltaTime);
         }

         UpdateAmmo ();
     }

     #region Shooting
     private void ShootRifle () {

         RaycastHit hit;
         Vector3 trgt, noSpread, spread;

         Ray ray = cam.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0));
         Rigidbody proj = Instantiate (weapon.projectile, weaponHole.position, Quaternion.identity);

         Bullet blt = proj.GetComponent<Bullet> ();
         blt.Dad = transform;
         blt.Damage = weapon.damage;

         if (Physics.Raycast (ray, out hit)) trgt = hit.point - weaponHole.position;
         else trgt = ray.GetPoint (weapon.range);

         noSpread = trgt - weaponHole.position;

         spread.x = Random.Range (-weapon.spread, weapon.spread);
         spread.y = Random.Range (-weapon.spread, weapon.spread);
         spread.z = Random.Range (-weapon.spread, weapon.spread);

         spread += noSpread;

         proj.transform.forward = spread.normalized;
         proj.velocity = weapon.projSp * weaponHole.forward;

         //proj.AddForce (weapon.projSp * weaponHole.forward, ForceMode.Impulse);
     }
     #endregion

     #region HUD

     public void UpdateAmmo () {

         if (weapon != null) {
             pmryAmmo.text = "Ammo: " + weapon.ammo.ToString ();
             pmryAmmo.color = Color.white;

             pmryBullets.text = "Bullets: " + weapon.bullets.ToString ();
             pmryBullets.color = Color.white;

             if (weapon.ammo <= (weapon.maxMag / 3)) pmryAmmo.color = Color.Lerp (Color.white, Color.red, 100);
             if (weapon.bullets <= (weapon.maxMag)) pmryBullets.color = Color.Lerp (Color.white, Color.red, 100);
         }
     }
     #endregion
 }
