using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour {
    #region Singleton
    public static ScoreManager Instance {
        get {
            if (!_this) {
                ScoreManager[] scRe = GameObject.FindObjectsOfType<ScoreManager> ();
                if (scRe.Length == 1) { _this = scRe[0]; }
            }
            return _this;
        }
    }
    private static ScoreManager _this;

    #endregion

    public int actualScore;
    public int prevScore;
    public int maxScore;
    [Space]
    [SerializeField] private GameObject hud;
    [SerializeField] private TMP_Text thisScore;
    [SerializeField] private TMP_Text pvScore;
    [SerializeField] private TMP_Text mxScore;
    [SerializeField] private List<GameObject> objs = new List<GameObject> ();

    public void ShowScore () {

        maxScore = actualScore;
        prevScore = actualScore;

        PlayerPrefs.SetInt ("MaxScore", maxScore);
        PlayerPrefs.SetInt ("PrevScore", prevScore);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        hud.SetActive (true);
        Time.timeScale = 0f;

        thisScore.text = actualScore.ToString ();
        pvScore.text = prevScore.ToString ();
        mxScore.text = maxScore.ToString ();
    }
    public void ResetGame () {

        foreach (var go in objs) {
            if (!go.activeSelf) go.SetActive (true);
            if (go.GetComponent<ZombieController> ()) go.GetComponent<ZombieController> ().ResetAll ();
            if (go.GetComponent<PlayerActions> ()) go.GetComponent<PlayerActions> ().ResetAll ();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        hud.SetActive (false);
        Time.timeScale = 1f;
    }
}
