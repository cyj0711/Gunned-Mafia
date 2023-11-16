using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    [SerializeField] private Animator m_vAanimator;

    [SerializeField] private SpriteRenderer m_vSpriteRendererHead;
    [SerializeField] private SpriteRenderer m_vSpriteRendererBody;
    [SerializeField] private SpriteRenderer m_vSpriteRendererLeg1;
    [SerializeField] private SpriteRenderer m_vSpriteRendererLeg2;
    void Awake()
    {
        //animator = GetComponent<Animator>();
    }

    public void SetWalk(bool isWalking)
    {
        m_vAanimator.SetBool("IsWalking", isWalking);
    }

    public void SetGhost(bool isGhost)
    {
        m_vAanimator.SetBool("IsGhost", isGhost);

        float fColorAlpha = isGhost ? 0.2f : 1f;

        m_vSpriteRendererHead.color = new Color(m_vSpriteRendererHead.color.r, m_vSpriteRendererHead.color.g, m_vSpriteRendererHead.color.b, fColorAlpha);
        m_vSpriteRendererBody.color = new Color(m_vSpriteRendererHead.color.r, m_vSpriteRendererHead.color.g, m_vSpriteRendererHead.color.b, fColorAlpha);
        m_vSpriteRendererLeg1.gameObject.SetActive(!isGhost);
        m_vSpriteRendererLeg2.gameObject.SetActive(!isGhost);
    }

    public void SetCharacterSprite(bool _bIsEnabled)
    {
        m_vSpriteRendererHead.enabled = _bIsEnabled;
        m_vSpriteRendererBody.enabled = _bIsEnabled;
        m_vSpriteRendererLeg1.enabled = _bIsEnabled;
        m_vSpriteRendererLeg2.enabled = _bIsEnabled;
    }

}
