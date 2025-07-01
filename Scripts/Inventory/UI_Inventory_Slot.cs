using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Data_Manager;

public class UI_Inventory_Slot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool empty;
    public TMPro.TMP_Text m_Text;
    public int x, y;
    public Image iconImage, checkImage;
    public Sprite checkOn, checkOff;
    public UI_Inventory_Slot baseSlot;// ��ũ ���̽� - �� ���̰�

    public delegate void Dele_HelperSlot(UI_Inventory_Slot _slot);
    public Dele_HelperSlot dele_LeftClick, dele_RightClick;
    public Dele_HelperSlot dele_Enter;
    public Dele_HelperSlot dele_Begin;

    public delegate void Dele_Helper();
    public Dele_Helper dele_Drag;
    public Dele_Helper dele_End;
    public Dele_Helper dele_Exit;
    public ItemStruct item;

    public void SetStart(int _x, int _y)
    {
        x = _x;
        y = _y;
        m_Text.text = _x + "/" + _y;
        gameObject.name = m_Text.text;
        CheckOff();
    }

    void SetSlot(ItemStruct _item)
    {
        item = _item;
        empty = item.ID == null;
        if (!empty)
            iconImage.sprite = item.Icon;
        iconImage.gameObject.SetActive(!empty);
    }

    public void SetBase(ItemStruct _item)
    {
        baseSlot = this;
        SetSlot(_item);
    }

    public void SetLink(UI_Inventory_Slot _slot)
    {
        baseSlot = _slot;
        SetSlot(_slot.item);
    }

    public void SetEmpty()
    {
        baseSlot = null;
        SetSlot(default);
        iconImage.gameObject.SetActive(false);
    }

    public bool CheckSlot()
    {
        Sprite temp = (empty == true) ? checkOn : checkOff;
        {
            checkImage.sprite = temp;
        }
        checkImage.gameObject.SetActive(true);
        return (empty == true);
    }

    public void CheckOff()
    {
        checkImage.gameObject.SetActive(false);
    }















    public void OnBeginDrag(PointerEventData eventData)
    {
        dele_Begin?.Invoke(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        dele_Drag?.Invoke();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dele_End?.Invoke();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // �׼�
            dele_LeftClick?.Invoke(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Ȯ��
            dele_RightClick?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        dele_Enter?.Invoke(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        dele_Exit?.Invoke();
    }
}
