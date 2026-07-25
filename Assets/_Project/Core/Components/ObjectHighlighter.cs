using UnityEngine;
using UnityEngine.UI;

// Requires Quick Outline's component on the same GameObject
[RequireComponent(typeof(Outline))]
public class ObjectHighlighter : MonoBehaviour, IHighlightable
{
    [SerializeField] private Outline outline;

    void Awake()
    {
        InitializeOutline();
    }

    private void InitializeOutline()
    {
        if (outline == null) outline = GetComponent<Outline>();

        // Ensure outline is off by default
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    public void Highlight()
    {
        if (outline != null)
        {
            outline.enabled = true;
        }
    }

    public void Unhighlight()
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}