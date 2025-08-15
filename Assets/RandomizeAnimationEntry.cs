using System.Collections;
using UnityEngine;

public class RandomizeAnimationEntry : MonoBehaviour
{
    private Animator animator;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        StartCoroutine(DelayAnimationStart());
        print("Activated: " + gameObject.name);
    }

    IEnumerator DelayAnimationStart()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));
        animator.SetTrigger("Show");
        var state = animator.GetCurrentAnimatorStateInfo(0);
        animator.Play(state.fullPathHash, 0, Random.Range(0f, 0.5f));
    }


}
