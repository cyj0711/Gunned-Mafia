using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMenuManager : Singleton<UIMenuManager>
{
    [SerializeField] GameObject m_vMenuPanelObject;

    bool m_bISMenuActive = false;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            m_bISMenuActive = !m_bISMenuActive;

            m_vMenuPanelObject.SetActive(m_bISMenuActive);
        }
    }
}
