using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CookingAnimator : MonoBehaviour
{
    public List<GameObject> objects = new List<GameObject>();
    private Animator activeAnimator;
    private float lastTriggerTime;
    private float stopTime;

    void OnEnable()
    {
        GameManager.instance.cookRecipeEvent += OnCookRecipe;
    }

    void OnDisable()
    {
        GameManager.instance.cookRecipeEvent -= OnCookRecipe;
    }

    private void OnCookRecipe()
    {
        UpdateActiveObject();

        // Get animation duration dynamically
        float animationDuration = GetAnimationDuration("Cutting Animation");

        if (animationDuration > 0)
        {
            stopTime = animationDuration;
            lastTriggerTime = Time.time; // Store when animation was last triggered

            if (activeAnimator != null)
            {
                activeAnimator.SetTrigger("animate");
            }
        }
    }

    private void UpdateActiveObject()
    {
        GameObject activeObject = objects[0].activeSelf ? objects[0] : objects[1].activeSelf ? objects[1] : null;

        if (activeObject != null)
        {
            activeAnimator = activeObject.GetComponent<Animator>();

            if (activeAnimator == null)
            {
                Debug.LogWarning("CookingAnimator: Active object has no Animator component.");
            }
        }
    }

    void Update()
    {
        // Check if the animation should be stopped
        if (activeAnimator != null && Time.time >= lastTriggerTime + stopTime)
        {
            activeAnimator.ResetTrigger("animate");
        }
    }

    private float GetAnimationDuration(string animationName)
    {
        if (activeAnimator == null) return 0f;

        RuntimeAnimatorController controller = activeAnimator.runtimeAnimatorController;
        if (controller == null) return 0f;

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == animationName)
            {
                return clip.length;
            }
        }

        Debug.LogWarning($"CookingAnimator: Animation '{animationName}' not found in Animator.");
        return 0f;
    }
}
