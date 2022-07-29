using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public Transform Dad { get; set; }
    public float Damage { get; set; }
    private bool collision;
    private float timer = 10;

    private void OnEnable () {
        StartCoroutine (DestroyBullet ());
    }

    private void OnCollisionEnter (Collision other) {
        if (other.collider.name == gameObject.name) return;
        collision = true;
        IDamage dmg = other.collider.GetComponentInParent<IDamage> ();
        if (dmg == null) dmg = other.collider.transform.root.GetComponent<IDamage> ();
        if (dmg == null && other.collider.attachedRigidbody) dmg = other.collider.attachedRigidbody.GetComponent<IDamage> ();
        if (dmg != null) dmg.Damage (Damage);

        Destroy (gameObject);
    }
    private IEnumerator DestroyBullet () {
        while (timer > 0) {
            yield return null;
            timer -= Time.deltaTime;
        }
        if (timer <= 0 && !collision) Destroy (gameObject);
    }
}
