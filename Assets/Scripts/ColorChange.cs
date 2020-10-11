
using UnityEngine;
using MyBox;

public class ColorChange : MonoBehaviour
{
    public bool changeColor;
    public float yScale = 20f;
    public bool topOnly;
    [ConditionalField(nameof(changeColor)), Min(0f)]
    public float shiftFactor = 1f;

    public Gradient gradient;

    private Renderer _renderer;

    private void Start()
    {
        _renderer = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (changeColor)
        {
            _renderer.material.SetColor("_Color", gradient.Evaluate(Mathf.Clamp((gameObject.transform.localScale.y/yScale/(topOnly ? 1 : 2))*shiftFactor, 0f, 1f)));
        }
    }
}