using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashMeshRenderer : MonoBehaviour
{
    private MeshRenderer _meshRenderer;
    private SkinnedMeshRenderer _skinnedMeshRenderer;

    private List<Color> _defaultColors = new();

    public FlashMeshRenderer(MeshRenderer meshRenderer, SkinnedMeshRenderer skinnedMeshRenderer)
    {
        _meshRenderer = meshRenderer;
        _skinnedMeshRenderer = skinnedMeshRenderer;

        if (meshRenderer != null)
        {
            foreach (Material mat in meshRenderer.materials)
            {
                _defaultColors.Add(mat.color);
            }
        }
        if (skinnedMeshRenderer != null)
        {
            foreach (Material mat in skinnedMeshRenderer.materials)
            {
                _defaultColors.Add(mat.color);
            }
        }

    }

    public void ChangeColor(Color flashColor)
    {
        if (_meshRenderer != null)
        {
            for (int i = 0; i < _meshRenderer.materials.Length; i++)
            {
                _meshRenderer.materials[i].color = flashColor;
            }
        }
        if (_skinnedMeshRenderer != null)
        {
            for (int i = 0; i < _skinnedMeshRenderer.materials.Length; i++)
            {
                _skinnedMeshRenderer.materials[i].color = flashColor;
            }
        }

    }

    public void RestoreColor()
    {
        if (_meshRenderer != null)
        {
            for (int i = 0; i < _meshRenderer.materials.Length; i++)
            {
                _meshRenderer.materials[i].color = _defaultColors[i];
            }
        }

        if (_skinnedMeshRenderer != null)
        {
            for (int i = 0; i < _skinnedMeshRenderer.materials.Length; i++)
            {
                _skinnedMeshRenderer.materials[i].color = _defaultColors[i];
            }
        }
    }
}
