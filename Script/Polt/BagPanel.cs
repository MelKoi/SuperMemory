using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class BagPanel : MonoBehaviour
{
    [Header("背包面板")]
    public GameObject charactorPanel; //角色背包面板
    public GameObject weaponPanel; //武器背包面板
    public GameObject itemPanel; //道具背包面板
    [Header("预制体")]
    public GameObject characterPrefeb; //角色预制体 
    public GameObject weaponPrefeb; //武器预制体
    public GameObject itemPrefab;

    [Header("事件监听")]
    public VoidEventSO showBagPanelEvent;  //监听显示背包广播
    [Header("广播")]
    public ChangeFightCharactorSO changeFightCharactorEvent;

    public BagDataManager bagDataManager;
    public List<GameObject> charactorList;
    public List<GameObject> weaponList;
    private float currentPosY;
    private RectTransform rectTransform;
    public GameObject TextPanel;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        bagDataManager = GameObject.Find("Bag").GetComponent<BagDataManager>();
        currentPosY = rectTransform.anchoredPosition.y;
    }
    private void OnEnable()
    {
        showBagPanelEvent.OnEventRiased += ShowBagPanel;
    }
    private void OnDisable()
    {
        showBagPanelEvent.OnEventRiased -= ShowBagPanel;
    }
    private void Update()
    {
        UpDateCardChoose();
    }
    public void UpDateCardChoose()
    {
        foreach(GameObject charactor in charactorList)
        {
            Transform chooseTransform = charactor.transform.Find("isChoose");
            GameObject isChoose = chooseTransform.gameObject;
            isChoose.SetActive(charactor.GetComponent<CharacterCardManager>().charactorasset.Cardname.Equals(bagDataManager.fightCharactor.Cardname));
        }
        foreach (GameObject weapon in weaponList)
        {
            Transform chooseTransform = weapon.transform.Find("isChoose");
            GameObject isChoose = chooseTransform.gameObject;
            bool flag = false;
            foreach (WeaponAsset fightWeapon in bagDataManager.fightWeapons)
            {
                int weaponnum = 0;
                if(weapon.GetComponent<WeaponCardManager>().
                    weaponAsset.WeaponName.Equals(fightWeapon.WeaponName))
                {
                    flag = true;
                }
                weaponnum++;
            }
            isChoose.SetActive(flag);
            
        }
    }
    private void CreateCharacter(CharactorAsset character, Transform parent)
    {
        GameObject newCharacter = Instantiate(characterPrefeb, parent);
        newCharacter.GetComponent<CharacterCardManager>().charactorasset = character;
        newCharacter.GetComponent<CharacterCardManager>().ReadCardFromAsset(character);
        charactorList.Add(newCharacter);
    }
    private void CreateWeapon(WeaponAsset weapon, Transform parent)
    {
        GameObject newWeapon = Instantiate(weaponPrefeb, parent);
        
        newWeapon.GetComponent<WeaponCardManager>().weaponAsset = weapon;
        newWeapon.GetComponent<WeaponCardManager>().ReadCardFromAsset(weapon);
        weaponList.Add(newWeapon);
    }
    public void ShowBagPanel()
    {
        int charactorCount = 1;
        int weaponCount = 1;
        //生成角色卡
        for (int i = 0; bagDataManager.bagAsset.runtimeBagData.characterStates.Count > i; i++) {
            if (bagDataManager.bagAsset.runtimeBagData.characterStates[i].owned)
            {
                var charactor = bagDataManager.bagAsset.runtimeBagData.characterStates[i].asset;
                string place = "Charactor/CharacterCard" + charactorCount.ToString();
                CreateCharacter(charactor, transform.Find(place));
                Debug.Log("生成角色卡");
                charactorCount++;
            }
        }
        //生成武器卡
        for(int i = 0; bagDataManager.bagAsset.runtimeBagData.weaponStates.Count > i; i++) {
            if (bagDataManager.bagAsset.runtimeBagData.weaponStates[i].owned)
            {
                var weapon = bagDataManager.bagAsset.runtimeBagData.weaponStates[i].asset;
                string place = "Weapon/WeaponCard" + weaponCount.ToString();
                CreateWeapon(weapon, transform.Find(place));
                Debug.Log("生成武器卡");
                weaponCount++;
            }
        }

        //下放到屏幕内显示
        rectTransform.DOAnchorPosY(0, 1f);
    }
    public void CloseBagPanel()
    {
        rectTransform.DOAnchorPosY(currentPosY, 1f);
        TextPanel.SetActive(true);

    }
    public void ShowCharactorPanel()
    {   
        charactorPanel.transform.SetSiblingIndex(2);
        charactorPanel.SetActive(true);
        weaponPanel.SetActive(false);
        itemPanel.SetActive(false);
    }

    public void ShowWeaponPanel()
    {
        weaponPanel.transform.SetSiblingIndex(2);
        charactorPanel.SetActive(false);
        weaponPanel.SetActive(true);
        itemPanel.SetActive(false);
    }

    public void ShowItemPanel()
    {
        itemPanel.transform.SetSiblingIndex(2);
        charactorPanel.SetActive(false);
        weaponPanel.SetActive(false);
        itemPanel.SetActive(true);
    }
}
