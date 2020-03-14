using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    // オーディオの定義
    [SerializeField] private AudioClip gameMusic;

    // デリゲートの定義
    public delegate void GameDelegate();
    public static event GameDelegate OnGameStarted;
    public static event GameDelegate OnGameOverConfirmed;
    public static event GameDelegate BackToMenu;

    // ゲームマネージャーのインスタンスを定義
    public static GameManager Instance;

    // コンストの定義
    private const int STARTING_SCORE_T = 0;
    private const bool STARTING_GAMEOVER_STATE = false;
	private const bool PLAYER_DEAD_GAMEOVER_STATE = true;
	private const int TIMESCALE_PAUSE = 0;
	private const int TIMESCALE_RESUME = 1;

    // 各ページの定義
    public GameObject startPage;
    public GameObject gameOverPage;
    public GameObject countdownPage;
    public GameObject pausePage;

    // ポーズボタンの定義 
    public GameObject pauseButton;
    
    // 画面に表示されるスコア
    public Text scoreText;

    // ページステート
    enum PageState {
        None,
        Start,
        GameOver,
        Countdown,
        Pause
    }

    // 現在のスコア
    int score = STARTING_SCORE_T;
    // ゲームオーバーのboolean
    bool gameOver = true;

    public bool GameOver { get { return gameOver; } }
    // public int Score { get { return score; } }

    // Awake
    private void Awake() {
        Instance = this;
        // ポーズボタンをインアクティブ
        pauseButton.SetActive(false);
    }

    // 各デリゲートを足す
    private void OnEnable() {
        CountdownText.OnCountdownFinished += OnCountdownFinished;
        PlayerControl.OnPlayerDied += OnPlayerDied;
        PlayerControl.OnPlayerScored += OnPlayerScored;
    }

    // 各デリゲートを引く 
    private void OnDisable() {
        CountdownText.OnCountdownFinished -= OnCountdownFinished;
        PlayerControl.OnPlayerDied -= OnPlayerDied;
        PlayerControl.OnPlayerScored -= OnPlayerScored;
    }

    // カウントダウンが終わった時の処理
    void OnCountdownFinished() {
        SetPageState(PageState.None);							// ページステートを変える
        OnGameStarted();										// イベントは PlayerControl に送られる
        score = STARTING_SCORE_T;								// スコアの初期化
        gameOver = STARTING_GAMEOVER_STATE;						// ゲームオーバーの初期化
        pauseButton.SetActive(true);							// ポーズボタンをアクティブ
        AudioManager.Instance.PlayMusic(gameMusic);				// ゲーム音楽を始める
    }

	// プレーヤーが倒れた時の処理
    void OnPlayerDied() {
		gameOver = PLAYER_DEAD_GAMEOVER_STATE;					// ゲームオーバーをtrue
		pauseButton.SetActive(false);							// ポーズボタンをインアクティブ
		AudioManager.Instance.StopMusic(gameMusic);				// ゲーム音楽を止める

        int savedScore = PlayerPrefs.GetInt("HighScore");		// 保存されているハイスコアを一時的に代入
        if(score > savedScore) {								// 現在のスコアがハイスコアより大きい場合
            PlayerPrefs.SetInt("HighScore", score);				// 現在のスコアをハイスコアとして保存
        }
        SetPageState(PageState.GameOver);						// ページステートをゲームオーバーに変換
    }

	// プレーヤーのスコア加算の処理
    void OnPlayerScored() {
        score++;												// スコアの値を足す
        scoreText.text = score.ToString();						// 画面のスコアを更新
    }

	// Replayボタンが押された時の処理
    public void ConfirmGameOver() {
        OnGameOverConfirmed();									// イベントは PlayerControl に送られる
        scoreText.text = STARTING_SCORE_T.ToString();			// 画面のスコアの初期化
        SetPageState(PageState.Start);							// ページステートをスタートに変換
        pauseButton.SetActive(false);							// ポーズボタンをインアクティブ
	}

	// Playボタンが押された時の処理
    public void StartGame() {
        SetPageState(PageState.Countdown);						// ページステートをカウントダウンに変換
    }

	// Pauseボタンが押された時の処理
    public void PauseGame() {
        SetPageState(PageState.Pause);							// ページステートをポーズに変換
        Time.timeScale = TIMESCALE_PAUSE;						// タイムスケールを停止
        AudioManager.Instance.PauseMusic(gameMusic);			// 音楽をポーズされる
    }

	// Resumeボタンが押された時の処理
    public void ResumeGame() {
        SetPageState(PageState.None);							// ページステートを無しに変換
        Time.timeScale = TIMESCALE_RESUME;						// タイムスケールを再開
        AudioManager.Instance.PlayMusic(gameMusic);				// 音楽を再開
    }

	// ToMenuボタンが押された時の処理
    public void ToMenu() {
        ConfirmGameOver();                                      // Replayボタンが押された時の処理
		Time.timeScale = TIMESCALE_RESUME;						// タイムスケールを再開
		gameOver = PLAYER_DEAD_GAMEOVER_STATE;					// ゲームオーバー
        BackToMenu();											// イベントは PlayerControl に送られる
    }

	// ページステートの変換処理
	void SetPageState(PageState state) {
		switch (state) {
			case PageState.None:
				startPage.SetActive(false);
				gameOverPage.SetActive(false);
				countdownPage.SetActive(false);
				pausePage.SetActive(false);
				break;
			case PageState.Start:
				startPage.SetActive(true);
				gameOverPage.SetActive(false);
				countdownPage.SetActive(false);
				pausePage.SetActive(false);
				break;
			case PageState.GameOver:
				startPage.SetActive(false);
				gameOverPage.SetActive(true);
				countdownPage.SetActive(false);
				pausePage.SetActive(false);
				break;
			case PageState.Countdown:
				startPage.SetActive(false);
				gameOverPage.SetActive(false);
				countdownPage.SetActive(true);
				pausePage.SetActive(false);
				break;
			case PageState.Pause:
				startPage.SetActive(false);
				gameOverPage.SetActive(false);
				countdownPage.SetActive(false);
				pausePage.SetActive(true);
				break;
			default:
				break;
		}
	}
}
