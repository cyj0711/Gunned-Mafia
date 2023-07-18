using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NotificationItemController : MonoBehaviour
{
    [SerializeField] Text m_vNotificationText;
    [SerializeField] Image m_vNotificationImage;

    private bool isPlaying;
    private float m_fFadeTime = 1f;
    private float m_fImageAlphaCorrection = 0.7f;   // 배경이미지 투명값은 0~0.7이므로 0.7을 곱해 보정함

    public void SetText(string _strText)
    {
        m_vNotificationText.text = _strText;

        StartCoroutine(nameof(PlayFadeOut));
    }

    IEnumerator PlayFadeOut()
    {
        // 애니메이션 재생중.  
        isPlaying = true;

        float start = 0f;
        float end = 1f;
        float time = 0f;


        Color vImageColor = m_vNotificationImage.color;
        Color vTextColor = m_vNotificationText.color;
        vImageColor.a = Mathf.Lerp(start, end, time) * m_fImageAlphaCorrection;
        vTextColor.a = Mathf.Lerp(start, end, time);

        while (vTextColor.a < 1f)
        {
            // 경과 시간 계산.  
            // animTime 동안 재생될 수 있도록 animTime으로 나누기.  
            time += Time.deltaTime / m_fFadeTime;


            vImageColor.a = Mathf.Lerp(start, end, time) * m_fImageAlphaCorrection;
            vTextColor.a = Mathf.Lerp(start, end, time);

            m_vNotificationImage.color = vImageColor;
            m_vNotificationText.color = vTextColor;


            yield return null;
        }

        // 애니메이션 재생 완료.  
        isPlaying = false;

        yield return new WaitForSeconds(3f);
        StartCoroutine(nameof(PlayFadeIn));
    }

    IEnumerator PlayFadeIn()
    {
        // 애니메이션 재생중.  
        isPlaying = true;

        float start = 1f;
        float end = 0f;
        float time = 0f;


        Color vImageColor = m_vNotificationImage.color;
        Color vTextColor = m_vNotificationText.color;
        vImageColor.a = Mathf.Lerp(start, end, time) * m_fImageAlphaCorrection;
        vTextColor.a = Mathf.Lerp(start, end, time);

        while (vTextColor.a > 0f)
        {
            // 경과 시간 계산.  
            // animTime 동안 재생될 수 있도록 animTime으로 나누기.  
            time += Time.deltaTime / m_fFadeTime;


            vImageColor.a = Mathf.Lerp(start, end, time) * m_fImageAlphaCorrection;
            vTextColor.a = Mathf.Lerp(start, end, time);

            m_vNotificationImage.color = vImageColor;
            m_vNotificationText.color = vTextColor;

            yield return null;
        }

        // 애니메이션 재생 완료.  
        isPlaying = false;

        Destroy(gameObject);
    }
}
