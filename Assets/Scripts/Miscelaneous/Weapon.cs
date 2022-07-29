using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Weapon es el Scriptable Object encargado de gestionar los parametros del arma, asi como de mostrar
/// información sobre la misma.
/// ¡El primer hijo de la jerarquia del prefab debe contener el sistema de particulas encargado del destello!
/// </summary>
[CreateAssetMenu (fileName = "Weapon", menuName = "Weapons", order = 0)]
public class Weapon : ScriptableObject {

    [Header ("Ammo")]
    public int maxMag; //Balas máximas por cartucho
    public int maxBullets; //Balas maximas del arma
    [HideInInspector] public int ammo;
    [HideInInspector] public int bullets;

    [Header ("Bullet")]
    public Rigidbody projectile;
    [Range (60f, 250f)] public float projSp = 60;

    [Header ("Statistics")]
    [Range (1f, 250f)] public float range; //Distancia del arma
    [Range (1f, 30f)] public float damage; //Daño del arma 
    [Range (0f, 1f)] public float fireRate; //Cadencia de disparo
    [Range (0f, 5f)] public float tReload; //Tiempo de recarga del cartucho      
    [Range (0.005f, 0.08f)] public float spread; //Desvío de la "Bala"

    [Header ("Graphics")]
    public GameObject mesh;
}
