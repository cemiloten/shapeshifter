using UnityEngine;

public class Block : MonoBehaviour
{
    private bool active;
    private Color color;
    private MaterialPropertyBlock props;
    private new Renderer renderer;

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
    public Color Color
    {
        get => color;
        set
        {
            color = value;
            if (renderer != null)
            {
                props.SetColor("_Color", color);
                renderer.SetPropertyBlock(props);
            }
        }
    }

    private void Start()
    {
        props = new MaterialPropertyBlock();
        renderer = GetComponent<Renderer>();
    }
}