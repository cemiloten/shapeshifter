using UnityEngine;

public class Block : MonoBehaviour
{
    private bool active;

    public Vector2Int Position { get; set; }
    public bool Active
    {
        get => active;
        set
        {
            active = value;
            gameObject.SetActive(value);
        }
    }
}