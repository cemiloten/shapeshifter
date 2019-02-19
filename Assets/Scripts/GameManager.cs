using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public float timeBetweenShapes = 10f;

    private float timeToNextShape;
    private int matchedShape = 0;
    private GUIStyle style = new GUIStyle();

    public static GameManager Instance { get; private set; }

    void OnEnable()
    {
        ShapeShifter.OnShift += CheckMatch;
    }

    private void OnDisable()
    {
        ShapeShifter.OnShift -= CheckMatch;
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
        Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -10f);
        timeToNextShape = timeBetweenShapes;
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
            // do wall stuff
        }
        else
        {
        }

        TouchController.Update();
    }

    private void CheckMatch()
    {
        if (IsMatching())
        {
            Debug.Log("match");
            // next shape to match
        }
        else
            Debug.Log("no match");
    }

    private bool IsMatching()
    {
        return false;

        // for shapeshifter blocks
            // for obstacle blocks
                // if active and active
                    // true
        // false

    }

    void OnGUI()
    {
        style.fontSize = 36;
        GUILayout.BeginArea(new Rect(20, 20, 800, 200));
        GUILayout.Label(string.Format("Time until next wall: {0:0.#}", timeToNextShape), style);
        GUILayout.EndArea();
    }
}