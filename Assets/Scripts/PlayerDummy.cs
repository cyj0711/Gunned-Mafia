using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDummy : MonoBehaviour
{
    [SerializeField] Rigidbody2D m_vRigidBody;
    [SerializeField] GameObject m_vCharacterObject;

    [SerializeField] Collider2D m_vCharacterUIClickCollider;

    [SerializeField] Text m_vHealthText;

    private int m_iCurrentHealth;

    void Awake()
    {

    }

    private void Start()
    {
        m_iCurrentHealth = 100;
    }

    public void Hit(int _idamage, int _iShooterActorNumber, int _iWeaponID)
    {
        m_iCurrentHealth = m_iCurrentHealth - _idamage;
        m_vHealthText.text = m_iCurrentHealth.ToString();
    }

}
