using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControl : MonoBehaviour {

	// コンストの定義
	private const float TAP_FORCE = 10f;
	private const float TILT_SMOOTH = 5f;
	private const bool SIMULATION_ON = true;
	private const bool SIMULATION_OFF = false;
	private const int RANDOM_NUM_MIN = 1;
	private const int RANDOM_NUM_MAX = 5;

	// デリゲートの定義
	public delegate void PlayerDelegate();
    public static event PlayerDelegate OnPlayerDied;
    public static event PlayerDelegate OnPlayerScored;

	// オーディオの定義
    public AudioSource flapAudio;
    public AudioSource dieAudio1, dieAudio2, dieAudio3, dieAudio4;

	// 設定の定義
    Rigidbody2D rb;
    Quaternion downRotation;
    Quaternion forwardRotation;
    GameManager gameManager;

	// キャラの設定
	public float tapForce = TAP_FORCE;
	public float tiltSmooth = TILT_SMOOTH;
	public Vector3 startPos;

	// Start
	private void Start() {
        rb = GetComponent<Rigidbody2D>();								// Rigidbody2D を取得
        downRotation = Quaternion.Euler(0, 0, -90);						// 下に向く角度を設定
        forwardRotation = Quaternion.Euler(0, 0, 35);					// 角度を設定
        gameManager = GameManager.Instance;								// GameManagerのインスタンスを設定
        rb.simulated = SIMULATION_OFF;									// Simulation をオフにする
    }

	// OnEnable時の処理
    private void OnEnable() {
        GameManager.OnGameStarted += OnGameStarted;						// Delegateを足す
        GameManager.OnGameOverConfirmed += OnGameOverConfirmed;			// Delegateを足す
		GameManager.BackToMenu += BackToMenu;							// Delegateを足す
	}
	
	// OnDisable時の処理
    private void OnDisable() {
        GameManager.OnGameStarted -= OnGameStarted;						// Delegateを引く
        GameManager.OnGameOverConfirmed -= OnGameOverConfirmed;			// Delegateを引く
        GameManager.BackToMenu -= BackToMenu;							// Delegateを引く
    }

	// GameStart時の処理
    void OnGameStarted() {
        rb.velocity = Vector3.zero;										// 重力をzeroにする
        rb.simulated = SIMULATION_ON;									// Simulationをオンにする
    }

	// GameOverConfirmed時の処理
    void OnGameOverConfirmed() {
        transform.localPosition = startPos;								// ポジションを設定する
        transform.rotation = Quaternion.identity;						// 角度を設定する
    }

	// BackToMenu時の処理
    void BackToMenu() {
        rb.simulated = SIMULATION_OFF;									// Simulationをオフにする
    }

	// 画面タッチがUI上なのか確認する処理
    private bool IsPointerOverUIObject() {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

	// Update
    private void Update() {
		// GameOverの場合、処理はやめる
        if (gameManager.GameOver) return;

		// 画面タッチした場合
        if (Input.GetMouseButtonDown(0)) {
			// タッチがUI上の場合
            if (EventSystem.current.IsPointerOverGameObject() || IsPointerOverUIObject())
                return;

            flapAudio.Play();												// Audioを再生する
            transform.rotation = forwardRotation;							// 角度を設定する
            rb.velocity = Vector3.zero;										// 速度ベクトルの設定
            rb.AddForce(Vector2.up * tapForce, ForceMode2D.Force);			// X 方面に力を与える

        }
		// 徐々にキャラの角度を下げる処理
        transform.rotation = Quaternion.Lerp(transform.rotation, downRotation, tiltSmooth * Time.deltaTime);
    }

	// OnTriggerEnter2D時の処理
    private void OnTriggerEnter2D(Collider2D col) {
		// ScoreZoneに当たった場合
        if(col.gameObject.tag == "ScoreZone") {
            OnPlayerScored(); // event sent to gameManager
			/* スコア処理を加える予定 */
			/* SEを足す予定 */
		}

		// DeadZoneに当たった場合
		if (col.gameObject.tag == "DeadZone") {
            rb.simulated = SIMULATION_OFF;									// simulationをオフにする
            OnPlayerDied();													// イベントを gameManager に送る
            int ran = Random.Range(RANDOM_NUM_MIN, RANDOM_NUM_MAX);			// ランダムな数字を生成する
			// ranによってランダムなSEを流す
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
