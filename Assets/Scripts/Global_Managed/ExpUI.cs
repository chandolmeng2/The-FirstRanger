using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ExpUI : SingletonBehaviour<ExpUI>
{
    [Header("경험치 바")]
    [SerializeField] private Image fillImage;

    [SerializeField] private Text expText;

    [Header("랭크 아이콘")]
    [SerializeField] private Image rankIconImage;  // 실제 UI에 표시될 이미지
    [SerializeField] private Sprite[] rankSprites; // Inspector에서 설정

    private void Start()
    {
        UpdateExpBar();
        UpdateRankImage();
        ExpManager.Instance.OnAddExp += UpdateExpBar;
        ExpManager.Instance.OnRankUp += UpdateRankImage;
    }

    private void Update()
    {
        // null 체크 추가
        if (ExpManager.Instance == null) return;

        ChangeColor();
        ShowText();
    }

    private void OnDestroy()
    {
        // ExpManager 인스턴스가 존재하는지 확인 후 이벤트 구독 해제
        if (ExpManager.Instance != null)
        {
            ExpManager.Instance.OnAddExp -= UpdateExpBar;
            ExpManager.Instance.OnRankUp -= UpdateRankImage;
        }
    }

    private void UpdateExpBar()
    {
        int exp = ExpManager.Instance.GetExp();
        ExpManager.Rank rank = ExpManager.Instance.GetRank();

        int min = 0;
        int max = 0;

        switch (rank)
        {
            case ExpManager.Rank.rank1:
                min = 0;
                max = ExpData.RankValue1;
                
                break;
            case ExpManager.Rank.rank2:
                min = ExpData.RankValue1;
                max = ExpData.RankValue2;
                
                break;
            case ExpManager.Rank.rank3:
                min = ExpData.RankValue2;
                max = ExpData.RankValue3;
                
                break;
        }

        float ratio = Mathf.InverseLerp(min, max, exp);
        fillImage.fillAmount = ratio;
    }
    private void UpdateRankImage()
    {
        ExpManager.Rank rank = ExpManager.Instance.GetRank();
        int rankIndex = (int)rank;

        if (rankIndex < rankSprites.Length && rankIconImage != null)
        {
            rankIconImage.sprite = rankSprites[rankIndex];
        }
    }
    
    public void ChangeColor()
    {
        switch((int)ExpManager.Instance.GetRank()) {
            case 0:
                fillImage.color= new Color(239 / 255f, 114 / 255f, 36 / 255f);
                break;
            case 1:
                fillImage.color = new Color(249 / 255f, 177 / 255f, 49 / 255f);
                break;
            case 2:
                fillImage.color = new Color(127 / 255f, 170 / 255f, 171 / 255f);
                break;
        }

    }

    public void Show()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public void ShowText()
    {
        // null 체크 추가
        if (ExpManager.Instance == null || expText == null) return;

        expText.text = "현재 경험치: " + ExpManager.Instance.GetExp();
    }
}


