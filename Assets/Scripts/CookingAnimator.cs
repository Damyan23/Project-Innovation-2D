using UnityEngine;
using System.Collections.Generic;

public class CookingAnimator : MonoBehaviour
{
    [SerializeField] private List<Animator> animators; // List of all animators

    private readonly string cookingTriggerName = "StartCooking";
    private readonly string idleParameterName = "GoIdle";

    private readonly string cookingAnimationName = "Cooking";  // Name of the cooking animation
    private readonly string idleAnimationName = "Idle";        // Name of the idle animation

    private Animator activeAnimator;
    private bool isIdleSet = false;
    private bool shouldLoopCooking = false;

    private void OnEnable()
    {
        GameManager.instance.cookRecipeEvent += OnCookRecipeEvent;
    }

    private void OnDisable()
    {
        GameManager.instance.cookRecipeEvent -= OnCookRecipeEvent;
    }

    private void OnCookRecipeEvent()
    {

        //if (string.IsNullOrEmpty(GameManager.instance.cookingOutputName) && string.IsNullOrEmpty (GameManager.instance.doneDishName)) return;
        // Find the active animator (the one whose parent is active)
        activeAnimator = GetActiveAnimator();

        if (activeAnimator == null) return; // No active animator found, do nothing

        AnimatorStateInfo currentState = activeAnimator.GetCurrentAnimatorStateInfo(0);

        if (currentState.IsName(cookingAnimationName) && currentState.normalizedTime < 1f)
        {
            // If the event is triggered while cooking, mark that we should loop after the animation ends
            shouldLoopCooking = true;
        }
        else
        {
            // If the animation is not running, start cooking normally
            StartCooking();
        }
    }

    private void StartCooking()
    {
        if (activeAnimator == null) return; // No active animator found, do nothing

        activeAnimator.SetTrigger(cookingTriggerName);
        isIdleSet = false;
        shouldLoopCooking = false; // Reset loop flag when starting fresh
    }

    private void Update()
    {
        if (activeAnimator == null) return; // No active animator to update

        AnimatorStateInfo currentState = activeAnimator.GetCurrentAnimatorStateInfo(0);

        // When the cooking animation ends
        if (currentState.IsName(cookingAnimationName) && currentState.normalizedTime >= 1f)
        {
            if (shouldLoopCooking)
            {
                // Restart the animation if it was triggered mid-animation
                activeAnimator.Play(cookingAnimationName, 0, 0f);
                shouldLoopCooking = false; // Reset flag after looping
            }
            else if (!isIdleSet)
            {
                // If no loop is needed, go idle
                activeAnimator.SetBool(idleParameterName, true);
                isIdleSet = true;
            }
        }

        // When transitioning to idle, reset GoIdle to false
        if (currentState.IsName(idleAnimationName) && activeAnimator.GetBool(idleParameterName))
        {
            activeAnimator.SetBool(idleParameterName, false);
        }
    }

    private Animator GetActiveAnimator()
    {
        foreach (Animator anim in animators)
        {
            if (anim.transform.parent != null && anim.transform.parent.gameObject.activeSelf)
            {
                return anim; // Return the first active animator found
            }
        }
        return null; // No active animator found
    }
}
