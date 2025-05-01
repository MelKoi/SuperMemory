using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoltManager : MonoBehaviour
{
    [Header("事件监听")]
    public VoidEventSO givePaint;
    public VoidEventSO showPanalOption;

    public GameObject pictruePanel;
    public GameObject pictrueArea;
    public GameObject PanalOption;
    public VerticalLayoutGroup verticalLayoutGroup;
    public List<Sprite>  images;
    private int currentImage = 0;

    private void Awake()
    {
        verticalLayoutGroup = PanalOption.GetComponent<VerticalLayoutGroup>();
    }
    private void OnEnable()
    {
        givePaint.OnEventRiased += ShowPictrue;
        showPanalOption.OnEventRiased += ShowPanalOption;
    }
    private void OnDisable()
    {
        givePaint.OnEventRiased -= ShowPictrue;
        showPanalOption.OnEventRiased -= ShowPanalOption;
    }
    
    public void ShowPictrue()
    {
        pictruePanel.SetActive(true);
        if(currentImage < images.Count) 
            pictrueArea.GetComponent<Image>().sprite = images[currentImage];
        currentImage++;
    }
    public void ShowPanalOption()
    {
        if (verticalLayoutGroup.padding.top != -300)
            verticalLayoutGroup.padding.top = -300;
        else
            verticalLayoutGroup.padding.top = -600;
    }
}
