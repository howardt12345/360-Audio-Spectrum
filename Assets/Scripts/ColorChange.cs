using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ColorChange : MonoBehaviour
{
    public ColorState changeColor;
    public float yScale = 20f;
    public bool topOnly;
    [ConditionalField(nameof(changeColor), false, ColorState.Hue), Min(0)]
    public float startingHue = 0.67f;
    [ConditionalField(nameof(changeColor), false, ColorState.Hue)]
    public float shiftFactor = -4f;

    private Renderer _renderer;

    private void Start()
    {
        _renderer = gameObject.GetComponent<Renderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (changeColor == ColorState.Hue)
        {
            startingHue = Mathf.Clamp(startingHue, 0f, 1f);
            _renderer.material.SetColor("_Color", Color.HSVToRGB(Mathf.Clamp(startingHue+((this.gameObject.transform.localScale.y/yScale/(topOnly ? 2 : 4))*shiftFactor), 0f, 1f), 1, 1));
        }
    }
}