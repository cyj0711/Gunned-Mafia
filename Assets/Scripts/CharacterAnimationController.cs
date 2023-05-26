using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public Animator animator;
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetWalk(bool isWalking)
    {
        animator.SetBool("IsWalking", isWalking);
    }

    public void SetGhost(bool isGhost)
    {
        animator.SetBool("IsGhost", isGhost);
    }
}
