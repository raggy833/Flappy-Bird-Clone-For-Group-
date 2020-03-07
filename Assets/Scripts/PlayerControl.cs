using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : MonoBehaviour {

    public delegate void PlayerDelegate();
    public static event PlayerDelegate OnPlayerDied;
    public static event PlayerDelegate OnPlayerScored;

    public float tapForce = 10f;
    public float tiltSmooth = 5f;
    public Vector3 startPos;

    public AudioSource flapAudio;
    public AudioSource dieAudio1, dieAudio2, dieAudio3, dieAudio4;

    Rigidbody2D rb;
    Quaternion downRotation;
    Quaternion forwardRotation;

    GameManager gameManager;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        downRotation = Quaternion.Euler(0, 0, -90);
        forwardRotation = Quaternion.Euler(0, 0, 35);
        gameManager = GameManager.Instance;
        rb.simulated = false;
    }

    private void OnEnable() {
        GameManager.OnGameStarted += OnGameStarted;
        GameManager.OnGameOverConfirmed += OnGameOverConfirmed;
        GameManager.BackToMenu += BackToMenu;
    }

    private void OnDisable() {
        GameManager.OnGameStarted -= OnGameStarted;
        GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;
        GameManager.BackToMenu -= BackToMenu;
    }

    void OnGameStarted() {
        rb.velocity = Vector3.zero;
        rb.simulated = true;
    }
    void OnGameOverConfirmed() {
        transform.localPosition = startPos;
        transform.rotation = Quaternion.identity;
    }

    void BackToMenu() {
        rb.simulated = false;
    }

    private bool IsPointerOverUIObject() {
        // TODO
        // Check how it works
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private void Update() {
        if (gameManager.GameOver) return;

        if (Input.GetMouseButtonDown(0)) {
            // Add eventSystem to detect UI click
            if (EventSystem.current.IsPointerOverGameObject() || IsPointerOverUIObject())
                return;

            flapAudio.Play();
            transform.rotation = forwardRotation;
            // To restart the velocity
            rb.velocity = Vector3.zero;
            rb.AddForce(Vector2.up * tapForce, ForceMode2D.Force);

        }
        transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D col) {
        if(col.gameObject.tag == "ScoreZone") {
            // Add score
            OnPlayerScored(); // event sent to gameManager
            // Play sound
        }

        if(col.gameObject.tag == "DeadZone") {
            rb.simulated = false;
            // dead event
            OnPlayerDied(); // event sent to gameManager
            // Create random num and play dieAudio
            int ran = Random.Range(1, 5);
            Debug.Log(ran);
            switch (ran) {
                case 1:
                    dieAudio1.Play();
                    break;
                case 2:
                    dieAudio2.Play();
                    break;
                case 3:
                    dieAudio3.Play();
                    break;
                case 4:
                    dieAudio4.Play();
                    break;
                default:
                    System.Console.WriteLine("Something went wrong");
                    break;
            }
        }
    }

}
