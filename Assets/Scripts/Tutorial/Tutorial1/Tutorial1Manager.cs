using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class Tutorial1Manager : MonoBehaviour
{
    private bool cutsceneMode;
    private bool isStart = true;
    private bool isCarMove = false;
    public GameObject[] cutScenes;
    public CanvasGroup fadePanel;
    private int count;
    private float escKey_count = 0.0f;
    public GameObject start_camera;
    public GameObject obj_camera1;
    public GameObject obj_camera2;
    public GameObject player;
    public GameObject crossHair;
    public Image radialGauge;
    public TextMeshProUGUI Text;
    private bool did = false;
    public Animator animator;

    private Transform currentTransform;
    private Vector3 originalPosition;

    void Start()
    {
        crossHair.SetActive(false);
        cutsceneMode = true;
        count = 0;
        StartFadeOut();
        radialGauge.enabled = false;

        currentTransform = cutScenes[1].transform;
        originalPosition = currentTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (isStart)
        {
            cutScenes[count].SetActive(true);
            cutScenes[count].transform.DOShakePosition(1.5f, 10f, 10, 90f, false, true);
            /*cutScenes[count].transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.5f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => {
                    cutScenes[count].transform.DOScale(Vector3.one, 0.3f);
                });*/
            isStart = false;

        }

        if (cutsceneMode)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                HideCutScene();
                // 사운드 출력
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ShowCutScene();
                // 사운드 출력
            }
            else if (Input.GetKey(KeyCode.Escape))
            {
                if (!did)
                {
                    radialGauge.enabled = true;
                    did = true;
                }              
                
                Debug.Log(escKey_count);
                escKey_count += Time.deltaTime;

                // 게이지바 채우기
                if (radialGauge != null)
                {
                    radialGauge.fillAmount = Mathf.Clamp01(escKey_count / 3.0f);
                }

                if (escKey_count > 3.0f)
                {
                    StartCoroutine(FadeIn());
                    for (int i = 0; i < 5; i++)
                    {
                        cutScenes[i].SetActive(false);
                    }
                    radialGauge.enabled = false;
                    Text.enabled = false;
                    cutsceneMode = false;
                    // 게이지 초기화
                    if (radialGauge != null)
                    {
                        radialGauge.fillAmount = 0.0f;
                    }
                }
            }
            else if (Input.GetKeyUp(KeyCode.Escape))
            {
                escKey_count = 0;
                // 게이지 초기화
                if (radialGauge != null)
                {
                    radialGauge.fillAmount = 0.0f;
                }
            }
        }
    }

    private void ShowCutScene()
    {

        if (count == 4)
        {
            return;
        }

        count++;
        cutScenes[count].SetActive(true);
        if (count == 1)
        {
            currentTransform.position = originalPosition;
            cutScenes[count].transform.DOMoveX(cutScenes[count].transform.position.x + 50f, 1.5f);
        }
        //애니메이션 올라오기
    }
    private void HideCutScene()
    {
        if (count == 0)
        {
            return;
        }
        cutScenes[count].SetActive(false);
        count--;
    }

    IEnumerator FadeIn()
    {
        if (fadePanel != null)
        {
            yield return fadePanel.DOFade(1f, 1f).WaitForCompletion();
            StartCoroutine(FadeOut());
            //StopCoroutine(FadeIn());
        }
    }

    IEnumerator FadeOut()
    {
        if (fadePanel != null)
        {
            StartCoroutine(CarMove());
            yield return fadePanel.DOFade(0f, 2f).WaitForCompletion();
            isCarMove = true;
            fadePanel.gameObject.SetActive(false);
            //StopCoroutine(FadeOut());
        }
    }

    IEnumerator CarMove()
    {
        start_camera.SetActive(false);
        obj_camera1.SetActive(true);

        // 자동차 소리 한 번 재생
        SoundManager.Instance.Play(SoundKey.Tutorial1_CarStart);

        animator.SetBool("isGo", true);
        yield return new WaitForSeconds(2.7f);
        obj_camera1.SetActive(false);
        obj_camera2.SetActive(true);
        yield return new WaitForSeconds(5.0f);
        StartCoroutine(LastFadeIn());
    }

    IEnumerator LastFadeIn()
    {
        if (fadePanel != null)
        {
            fadePanel.alpha = 0f;
            fadePanel.gameObject.SetActive(true);
            yield return fadePanel.DOFade(1f, 2f).WaitForCompletion();
            StartCoroutine(LastFadeOut());
        }
    }

    IEnumerator LastFadeOut()
    {
        if (fadePanel != null)
        {
            obj_camera2.SetActive(false);
            player.SetActive(true);
            crossHair.SetActive(true);
            yield return fadePanel.DOFade(0f, 2f).WaitForCompletion();
            //StopCoroutine(LastFadeOut());
            
        }
    }

    void StartFadeOut()
    {
        fadePanel.alpha = 1f;
        fadePanel.DOFade(0f, 2f);
    }
}
