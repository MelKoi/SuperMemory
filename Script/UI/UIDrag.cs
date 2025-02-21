using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIDrag : MonoBehaviour//基于warldplace 画布的拖动
{
    bool startdrag;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (startdrag)
        {
            transform.position = Input.mousePosition;
        }
    }
    public void StartDragUI()
    {
        startdrag = true;
    }
    public void StopDrugUI()
    {
        startdrag = false;
    }

}
