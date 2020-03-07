using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    [SerializeField] private AudioClip gameMusic;

    public delegate void GameDelegate();
    public static event GameDelegate OnGameStarted;
    public static event GameDelegate OnGameOverConfirmed;
    public static event GameDelegate BackToMenu;

    public static GameManager Instance;

    public GameObject startPage;
    public GameObject gameOverPage;
    public GameObject countdownPage;
    public GameObject pausePage;
    public GameObject pauseButton;
    public Text scoreText;


    enum PageState {
        None,
        Start,
        GameOver,
        Countdown,
        Pause
    }

    int score = 0;
    bool gameOver = true;

    public bool GameOver { get { return gameOver; } }
    // public int Score { get { return score; } }

    private void Awake() {
        Instance = this;
        pauseButton.SetActive(false);
    }

    private void OnEnable() {
        CountdownText.OnCountdownFinished += OnCountdownFinished;
        PlayerControl.OnPlayerDied += OnPlayerDied;
        PlayerControl.OnPlayerScored += OnPlayerScored;

    }
    private void OnDisable() {
        CountdownText.OnCountdownFinished -= OnCountdownFinished;
        PlayerControl.OnPlayerDied -= OnPlayerDied;
        PlayerControl.OnPlayerScored -= OnPlayerScored;

    }

    void OnCountdownFinished() {
        SetPageState(PageState.None);
        OnGameStarted(); // event sent to PlayerControl
        score = 0;
        gameOver = false;
        pauseButton.SetActive(true);
        AudioManager.Instance.PlayMusic(gameMusic);
    }

    void OnPlayerDied() {
        AudioManager.Instance.StopMusic(gameMusic);
        gameOver = true;
        pauseButton.SetActive(false);
        int savedScore = PlayerPrefs.GetInt("HighScore");
        if(score > savedScore) {
            PlayerPrefs.SetInt("HighScore", score);
        }
        SetPageState(PageState.GameOver);
    }

    void OnPlayerScored() {
        score++;
        scoreText.text = score.ToString();
    }

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

    public void ConfirmGameOver() {
        // activated when replay button is hit
        OnGameOverConfirmed(); // event sent to playerControl
        scoreText.text = "0";
        SetPageState(PageState.Start);
        pauseButton.SetActive(false);
    }

    public void StartGame() {
        // activated when play button is hit
        SetPageState(PageState.Countdown);
    }

    public void PauseGame() {
        SetPageState(PageState.Pause);
        // pause game
        Time.timeScale = 0;
        AudioManager.Instance.PauseMusic(gameMusic);
    }

    public void ResumeGame() {
        SetPageState(PageState.None);
        // resume game
        Time.timeScale = 1;
        AudioManager.Instance.PlayMusic(gameMusic);

    }

    public void ToMenu() {
        ConfirmGameOver();
        Time.timeScale = 1;
        gameOver = true;
        BackToMenu(); // event sent to PlayerControl
    }

}
