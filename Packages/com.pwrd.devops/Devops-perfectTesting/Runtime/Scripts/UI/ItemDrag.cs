using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform rectTransform;
    public Action<ItemCase> MouseUp;
    public ItemCase itemCase;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        itemCase = GetComponent<ItemCase>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        //transform.SetAsLastSibling();

    }

    public void OnDrag(PointerEventData eventData)
    {

        Vector3 pos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventData.enterEventCamera, out pos);
        rectTransform.position = new Vector3(rectTransform.position.x, pos.y);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        MouseUp?.Invoke(itemCase);
    }
}
