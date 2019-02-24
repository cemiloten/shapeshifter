using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float timeBetweenShapes = 10f;
    public Shifter shifter;
    public Shifter targetShifter;

    private int matchedShapes = 0;
    private float timeToNextShape;
    private State state;

    private GUIStyle style = new GUIStyle();

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
        // Camera.main.transform.position = new Vector3(
        //     (LevelManager.width - 1) / 2f,
        //     (LevelManager.height - 1) / 2f, -9f);

        // initialize map
        GetNewTarget();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.LogFormat("current state:\n{0}", state);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.LogFormat("current shifter state:\n{0}", shifter.State);
        }
        if (Input.GetKeyDown(KeyCode.Backspace))
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

        TouchManager.Update();
    }

    private void OnShift()
    {
        if (IsMatch())
        {
            GetNewTarget();
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
        style.fontSize = 36;
        GUILayout.BeginArea(new Rect(20, 20, 800, 200));
        GUILayout.Label(string.Format("Time until next target: {0:0.#}", timeToNextShape), style);
        GUILayout.Label(string.Format("Style: {0}", shifter.Style), style);
        GUILayout.EndArea();
    }
}