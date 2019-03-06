using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Text timeText;
    public Text scoreText;
    public Text totalTimeText;
    public Button newGameButton;

    public LevelManager levelManager;
    public GameObject shifterPrefab;
    public GameObject targetShifterPrefab;
    public GameObject swipeStartSprite;
    public GameObject potentialSwipeEndSprite;

    public float timeBetweenShapes = 10f;

    private int score = 0;
    private float timeToNextShape;
    private float totalTime;
    private State state;
    private Shifter shifter;
    private Shifter targetShifter;

    public static GameManager Instance { get; private set; }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            if (timeText != null)
                Instance.timeText = timeText;
            if (scoreText != null)
                Instance.scoreText = scoreText;
            if (totalTimeText != null)
                Instance.totalTimeText = totalTimeText;
            if (newGameButton != null)
                Instance.newGameButton = newGameButton;
            if (swipeStartSprite != null)
                Instance.swipeStartSprite = swipeStartSprite;
            if (potentialSwipeEndSprite != null)
                Instance.potentialSwipeEndSprite = potentialSwipeEndSprite;

            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        newGameButton.onClick.AddListener(InitializeGame);
    }

    public static void InitializeGame()
    {
        SceneManager.LoadScene("Game");
    }

    private void LoseGame()
    {
        if (shifter != null)
            shifter.OnShift -= OnShift;
        SceneManager.LoadScene("EndGame");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.LogFormat("loaded {0}", scene.name);

        if (scene.name == "Game")
        {
            Instantiate(levelManager);
            shifter = Instantiate(shifterPrefab).GetComponent<Shifter>() as Shifter;
            if (shifter != null)
                shifter.OnShift += OnShift;

            targetShifter = Instantiate(targetShifterPrefab).GetComponent<Shifter>() as Shifter;
            timeToNextShape = timeBetweenShapes;
            totalTime = 0f;
            score = 0;
            GetNewTarget();
            UpdateScoreText();
            UpdateTimeText();
        }
        else if (scene.name == "EndGame")
        {
            UpdateScoreText();
            UpdateTotalTimeText();
            newGameButton.onClick.AddListener(InitializeGame);
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    private void UpdateTimeText()
    {
        if (timeText != null)
            timeText.text = timeToNextShape.ToString("0.00");
    }

    private void UpdateTotalTimeText()
    {
        if (totalTimeText != null)
            totalTimeText.text = totalTime.ToString("0.00");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GetNewTarget();
            Debug.LogFormat("generated new state:\n{0}", state);
        }

        if (SceneManager.GetActiveScene().name == "Game")
        {
            timeToNextShape -= Time.deltaTime;
            totalTime += Time.deltaTime;
            if (timeToNextShape <= 0f)
            {
                if (!IsMatch())
                {
                    LoseGame();
                }

                timeToNextShape += timeBetweenShapes;
            }

            TouchManager.Update();
            UpdateTimeText();
        }
    }

    private void OnShift()
    {
        if (IsMatch())
        {
            ++score;
            timeToNextShape = timeBetweenShapes;
            GetNewTarget();
            UpdateScoreText();
        }
    }

    private bool IsMatch()
    {
        if (shifter == null)
        {
            return false;
        }
        if (state == null)
        {
            return false;
        }

        return state.Equals(shifter.State);
    }

    public void GetNewTarget()
    {
        state = LevelManager.Instance.GenerateState();
        if (targetShifter != null)
            targetShifter.State = state;
    }

    void OnGUI()
    {
    }
}