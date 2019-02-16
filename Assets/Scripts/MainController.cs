using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MainController : MonoBehaviour
{
    public TextMeshProUGUI tmpFPS;
    public TextMeshProUGUI tmpLives;
    public TextMeshProUGUI tmpMoney;
    public Button exitButton;
    public Spawn spawner;
    public CameraController camControls;
    public TowerBuilder towerBuilder;
    public GameObject welcome;
    public GameObject tutorial;
    public Button startButton;
    public Button tutorialButton;
    public GameObject endGameDialog;
    public Button quitButton;
    public Button newButton;
    public TextMeshProUGUI endTitleText;
    public TextMeshProUGUI endSubtitleText;

    private int money = 50;
    private bool gameStarted;
    private bool gameOver;

    public float Lives { get; private set; }

    // FPS
    private float timeLeft = 0.5f;
    private float timeCount;
    private int frames;

    public static MainController Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("Duplicate MainController on " + name);
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Application.targetFrameRate = 90;

        exitButton.onClick.AddListener(() =>
        {
            if (!Application.isEditor)
                System.Diagnostics.Process.GetCurrentProcess().Kill();
        });

        quitButton.onClick.AddListener(() =>
        {
            if (!Application.isEditor)
                System.Diagnostics.Process.GetCurrentProcess().Kill();
        });

        tutorialButton.onClick.AddListener(() =>
        {
            welcome.SetActive(false);
            tutorial.SetActive(true);
        });

        startButton.onClick.AddListener(() =>
        {
            welcome.SetActive(false);
            StartGame();
        });

        newButton.onClick.AddListener(() =>
        {
            spawner.Restart();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        });

        Lives = 30;
        tmpMoney.text = money.ToString("N0") + " $";

        if (PlayerPrefs.HasKey("tutorial") && PlayerPrefs.GetInt("tutorial") == 1)
            welcome.SetActive(true);
        else
            tutorial.SetActive(true);
    }

    void Update()
    {
        FPS();
    }

    private void LateUpdate()
    {
        if (gameStarted)
            tmpMoney.text = money.ToString("N0") + " $";

        if (gameOver)
        {
            gameOver = false;
            spawner.KillAll();
        }
    }

    void FPS()
    {
        timeLeft -= Time.deltaTime;
        timeCount += Time.deltaTime;
        ++frames;

        if (timeLeft <= 0)
        {
            tmpFPS.text = "FPS: " + (frames / timeCount).ToString("F1");
            frames = 0;
            timeLeft = 0.5f;
            timeCount = 0;
        }
    }

    public bool RoundEnded()
    {
        return spawner.RoundEnded();
    }

    public void LifeLost()
    {
        if (Lives > 0)
            tmpLives.text = "Lives: " + --Lives;

        if (Lives == 0)
        {
            Lives = -1;
            towerBuilder.Clear();
            towerBuilder.enabled = false;
            spawner.enabled = false;
            endTitleText.text = "Bad Luck";
            endSubtitleText.text = "You lost all your lives";
            endGameDialog.SetActive(true);
            gameOver = true;
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        spawner.gameObject.SetActive(true);
        camControls.enabled = true;
        towerBuilder.enabled = true;
    }

    public void Win()
    {
        towerBuilder.Clear();
        towerBuilder.enabled = false;
        endGameDialog.SetActive(true);
    }

    public int Money
    {
        get { return money; }
        set { money = value; }
    }
}
