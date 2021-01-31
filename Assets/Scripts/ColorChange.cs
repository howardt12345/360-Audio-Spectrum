using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class ColorChange : MonoBehaviour
{
    public bool changeColor;
    public float yScale = 20f;
    public bool topOnly;

    public float value;

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
            value = Mathf.Clamp(
                this.gameObject.transform.localScale.y / yScale / (topOnly ? 1 : 2), 0f, 1f);
            _renderer.material.SetColor("_Color", gradient.Evaluate(value));
        }
    }
} 