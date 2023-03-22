using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetWalk(bool isWalking)
    {
        animator.SetBool("IsWalking", isWalking);
    }
}
