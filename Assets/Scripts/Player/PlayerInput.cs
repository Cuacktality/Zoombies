using UnityEngine;

public class PlayerInput : MonoBehaviour {

    [HideInInspector] private PlayerController pc;
    [HideInInspector] private PlayerActions pa;
    [HideInInspector] private CharacterController cc;
    [HideInInspector] private PlayerShoot st;

    protected const string HORIZONTAL = "Horizontal";
    protected const string VERTICAL = "Vertical";
    protected const string MOUSE_X = "Mouse X";
    protected const string MOUSE_Y = "Mouse Y";

    protected const string RELOAD = "Reload";

    protected const string JUMP = "Jump";
    protected const string RUN = "Run";
    protected const string SHOOT = "Shoot";

    [HideInInspector] protected Vector2 mov, mouse;
    [HideInInspector] protected bool jump, run, radar, tch, shoot, change, reload, active, nade;
    protected float stForce = 0;

    private void Start () {
        cc = GetComponent<CharacterController> ();
        pc = GetComponent<PlayerController> ();
        pa = GetComponent<PlayerActions> ();
        st = GetComponent<PlayerShoot> ();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update () {
        mouse.x = Input.GetAxis (MOUSE_X);
        mouse.y = Input.GetAxis (MOUSE_Y);
        pc.Rotate (mouse.x, mouse.y);

        mov.x = Input.GetAxis (HORIZONTAL);
        mov.y = Input.GetAxis (VERTICAL);
        jump = Input.GetButton (JUMP);
        run = Input.GetKey (KeyCode.LeftShift);

        pc.Move (mov.x, mov.y, jump, run);
        st.Shot (Input.GetMouseButton (0));
        if (Input.GetKeyDown (KeyCode.R)) st.Reload ();
    }
}
