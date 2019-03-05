using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Text timeText;
    public Text scoreText;

    public float timeBetweenShapes = 10f;
    public Shifter shifter;
    public Shifter targetShifter;

    private int score = 0;
    private float timeToNextShape;
    private State state;

    public static GameManager Instance { get; private set; }

    private void OnEnable()
    {
        shifter.OnShift += OnShift;
    }

    private void OnDisable()
    {
        shifter.OnShift -= OnShift;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
    }

    private void Start()
    {
        timeToNextShape = timeBetweenShapes;

        // initialize map
        GetNewTarget();
        UpdateScoreText();
        UpdateTimeText();
    }

    private void UpdateScoreText()
    {
        scoreText.text = score.ToString();
    }

    private void UpdateTimeText()
    {
        timeText.text = timeToNextShape.ToString("0.00");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GetNewTarget();
            Debug.LogFormat("generated new state:\n{0}", state);
        }

        timeToNextShape -= Time.deltaTime;
        if (timeToNextShape <= 0f)
        {
            if (!IsMatch())
            {
                // lose game
            }

            timeToNextShape += timeBetweenShapes;
        }

        UpdateTimeText();
        TouchManager.Update();
    }

    private void OnShift()
    {
        if (IsMatch())
        {
            ++score;
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
        targetShifter.State = state;
    }

    void OnGUI()
    {
    }
}