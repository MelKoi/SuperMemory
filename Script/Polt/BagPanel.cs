using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public VoidEventSO bagPanelOpen;

    [HideInInspector]public BagDataManager bagDataManager;
    [HideInInspector] public BagAsset bagAsset;
    public List<GameObject> charactorList;
    public List<CharactorAsset> charactorAssetsList;
    public List<GameObject> weaponList;
    public List<WeaponAsset> weaponAssetsList;
    private int charactorCount = 1;
    private int weaponCount = 1;
    private float currentPosY;
    private RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        bagDataManager = GameObject.Find("Bag").GetComponent<BagDataManager>();
        currentPosY = rectTransform.anchoredPosition.y;
    }
    private void OnEnable()
    {
        showBagPanelEvent.OnEventRaised += ShowBagPanel;
    }
    private void OnDisable()
    {
        showBagPanelEvent.OnEventRaised -= ShowBagPanel;
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
    private void CreateCharactor(CharactorAsset charactor, Transform parent)
    {
        GameObject newCharactor = Instantiate(characterPrefeb, parent);
        newCharactor.GetComponent<CharacterCardManager>().charactorasset = charactor;
        newCharactor.GetComponent<CharacterCardManager>().ReadCardFromAsset(charactor);
        charactorList.Add(newCharactor);
        charactorAssetsList.Add(charactor);
    }
    private void CreateWeapon(WeaponAsset weapon, Transform parent)
    {
        GameObject newWeapon = Instantiate(weaponPrefeb, parent);
        
        newWeapon.GetComponent<WeaponCardManager>().weaponAsset = weapon;
        newWeapon.GetComponent<WeaponCardManager>().ReadCardFromAsset(weapon);
        weaponList.Add(newWeapon);
        weaponAssetsList.Add(weapon);
    }
    public void ShowBagPanel()
    {
       
        bagAsset = bagDataManager.bagAsset;
        //生成角色卡
        for (int i = 0; bagAsset.characters.Count > i; i++) {
            if (bagDataManager.HasCharactor(bagAsset.characters[i]))
            {
                var charactor = bagDataManager.bagAsset.characters[i];
                string place = "Charactor/CharacterCard" + charactorCount.ToString();
                if (!charactorAssetsList.Contains(charactor))
                {
                    CreateCharactor(charactor, transform.Find(place));
                    charactorCount++;
                }
                Debug.Log("生成角色卡");
            }
        }
        //生成武器卡
        for (int i = 0; bagAsset.weapons.Count > i; i++) {
            if (bagDataManager.HasWeapon(bagAsset.weapons[i]))
            {
                var weapon = bagDataManager.bagAsset.weapons[i];
                string place = "Weapon/WeaponCard" + weaponCount.ToString();
                if (!weaponAssetsList.Contains(weapon))
                {
                    CreateWeapon(weapon, transform.Find(place));
                    weaponCount++;
                }
                Debug.Log("生成武器卡"); 
            }
        }

        //下放到屏幕内显示
        rectTransform.DOAnchorPosY(0, 1f);
    }
    public void CloseBagPanel()
    {
        rectTransform.DOAnchorPosY(currentPosY, 1f);
        bagPanelOpen.RaiseEvent();
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
