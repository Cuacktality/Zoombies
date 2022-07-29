using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthZone : MonoBehaviour {
    internal enum Type {
        Damage,
        Health
    }

    [SerializeField] private Type type;
    [SerializeField] private float points = 5;

    private void OnTriggerEnter (Collider other) {
        if (type == Type.Damage) {
            IDamage dmg = other.GetComponent<IDamage> ();
            if (dmg != null) dmg.Damage (points);
        }
        if (type == Type.Health) {
            var pa = other.GetComponent<PlayerActions> ();
            pa.LoadLife (points++);
        }
    }
    private void OnTriggerStay (Collider other) {
        if (type == Type.Damage) {
            IDamage dmg = other.GetComponent<IDamage> ();
            if (dmg != null) dmg.Damage (points);
        }
        if (type == Type.Health) {
            var pa = other.GetComponent<PlayerActions> ();
            pa.LoadLife (points++);
        }
    }
}
