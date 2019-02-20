using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float timeBetweenShapes = 10f;

    private int matchedShapes = 0;
    private float timeToNextShape;
    private State state;

    private GUIStyle style = new GUIStyle();

    public static GameManager Instance { get; private set; }

    void OnEnable()
    {
        ShapeShifter.OnShift += IsMatching;
    }

    private void OnDisable()
    {
        ShapeShifter.OnShift -= IsMatching;
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
        Camera.main.transform.position = new Vector3(
            (LevelManager.width - 1) / 2f,
            (LevelManager.height - 1) / 2f, -10f);

        // initialize map
        state = LevelManager.Instance.GetState();
        for (int i = 0; i < State.Size; ++i)
        {
            Debug.LogFormat("{0}, {1}", state.cellStates[i], State.IndexToPosition(i));
        }
    }

    private void NextState()
    {

    }

    private void Update()
    {
        timeToNextShape -= Time.deltaTime;
        if (timeToNextShape <= 0f)
        {
            if (IsMatching() == false)
            {
                // lose game
            }

            timeToNextShape += timeBetweenShapes;
        }
        else
        {
        }

        TouchController.Update();
    }

    private bool IsMatching()
    {
        return false;
        // return myShape == playerShape
    }

    void OnGUI()
    {
        style.fontSize = 36;
        GUILayout.BeginArea(new Rect(20, 20, 800, 200));
        GUILayout.Label(string.Format("Time until next wall: {0:0.#}", timeToNextShape), style);
        GUILayout.EndArea();
    }
}