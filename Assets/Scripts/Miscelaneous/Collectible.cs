using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour {
    private Collider col;
    private PlayerActions pa;
    [Range (25, 500)][SerializeField] private int score;

    [SerializeField] private Type type;
    internal enum Type {
        Coin,
        Ammo,
        Health
    }

    private void LateUpdate () {
        transform.Rotate (new Vector3 (0, 105 * Time.deltaTime, 85 * Time.deltaTime));
    }

    private void OnTriggerEnter (Collider other) {
        pa = other.GetComponent<PlayerActions> ();

        if (pa != null) {

            switch (type) {
                case Type.Ammo:
                    var ps = pa.GetComponent<PlayerShoot> ();
                    if (ps.weapon.bullets < ps.weapon.maxBullets) ps.weapon.bullets = ps.weapon.maxBullets;
                    break;

                case Type.Health:
                    if (pa.ActualHealth < pa.GetMaxHealth) pa.LoadLife (pa.GetMaxHealth);
                    break;

                case Type.Coin:
                    ScoreManager.Instance.actualScore += score;
                    break;
            }
        }
        gameObject.SetActive (false);
    }
}
