using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Data_Manager;
using static UI_Inventory;
using static UI_Inventory_Slot;

public class UI_Inventory_Manager : MonoBehaviour
{
    public UI_Inventory inventory;
    public UI_Shop shop;
    public Image iconImage;

    bool onDrag, onCheck;
    Coroutine slotMoving, movingMoney;
    float moneyWallet;
    public TMPro.TMP_Text moneyText;

    public DragSlotType enterSlotType;
    UI_Inventory_Slot enterSlot;

    public DragSlotType selectSlotType;
    UI_Inventory_Slot selectSlot;

    ItemClass selectItemClass;
    ItemClass originItemClass;

    public void SetStart()
    {
        inventory.SetStart();
        shop.SetStart();
    }

    public void OpenCanvas(bool _open)
    {
        inventory.OpenCanvas(_open);
    }

    public void OpenShop(bool _open)
    {
        inventory.OpenCanvas(_open);
        shop.OpenCanvas(_open);
    }

    void Update()// ������ �߰� �׽�Ʈ
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            string randomID = "Fs_" + Random.Range(1, 4) + "001";
            FishStruct fishStruct = Singleton_Data.INSTANCE.Dict_Fish[randomID];
            FishStruct.RandomSize randomSize = fishStruct.GetRandom();
            AddItem(fishStruct.itemStruct);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            string randomID = "Eq_000" + Random.Range(1, 4);
            BuyItem(randomID);
            Debug.LogWarning("GetKeyDown2");
        }
    }

    public void AddItem(ItemStruct _itemStruct)
    {
        ItemClass itemClass = new ItemClass
        {
            item = _itemStruct,
            angle = 0,
            shape = _itemStruct.shape,
        };

        onDrag = true;
        selectItemClass = itemClass;
        DragSlot();
    }

    void BuyItem(string _id)
    {
        ItemStruct item = Singleton_Data.INSTANCE.GetItemStruct(_id);
        if (moneyWallet < item.price)
            return;

        if (inventory.BuyItem(item) == true)// �� ������ ������ ���Լ���
        {
            if (movingMoney != null)
                StopCoroutine(movingMoney);
            movingMoney = StartCoroutine(MoneyWallet(-item.price));
        }
    }

    void RemoveItem(UI_Inventory_Slot _slot)
    {
        UI_Inventory getInventory = GetInventory(enterSlotType);
        getInventory.SetEmpty(_slot);
    }

    void TradeItem()
    {
        if (selectSlotType == DragSlotType.None)
            return;

        if (enterSlotType != selectSlotType)
        {
            float price = selectItemClass.item.price;
            if (selectSlotType == DragSlotType.Shop)
                price *= -1f;

            if (movingMoney != null)
                StopCoroutine(movingMoney);
            movingMoney = StartCoroutine(MoneyWallet(price));
        }
    }

    //===========================================================================================================================
    // ��ǲ ��Ʈ��
    //===========================================================================================================================
    public void OnPointerLeftClick(UI_Inventory_Slot _slot)
    {
        // ������ üũ
        if (onDrag == true)// �巡�� ���� �� ���
        {
            if (onCheck == true)
            {
                UI_Inventory getInventory = GetInventory(enterSlotType);
                getInventory.SetSlot(enterSlot, selectItemClass);
                TradeItem();
            }
            else
            {
                if (selectSlot == null)
                {
                    // ���� ������ Ʈ���̵尡 �ƴ� ���?
                }
                else
                {
                    // ���� �� ���ٸ� ���� ��ġ�� ������
                    UI_Inventory getInventory = GetInventory(selectSlotType);
                    getInventory.SetSlot(selectSlot, originItemClass);
                }
            }
            originItemClass = null;
            OnTreshBox();
        }
        else// �巡�� ���� �ƴ� �� �Ⱦ�
        {
            if (_slot.empty == true)
                return;

            onDrag = true;
            selectSlot = _slot.GetLinkSlot;
            selectItemClass = selectSlot.itemClass;
            selectSlotType = enterSlotType;

            SetOriginItemClass();
            EmptySlot(selectSlotType);
            DragSlot();
        }
    }

    public void OnPointerRightClick(UI_Inventory_Slot _slot)
    {
        if (onDrag == true)// �巡�� ���� ��
        {
            SetDragRotate();
        }
        else
        {
            RemoveItem(_slot.GetLinkSlot);
        }
    }

    public void OnPointerEnter(UI_Inventory_Slot _slot, DragSlotType _dragSlotType)
    {
        enterSlotType = _dragSlotType;
        enterSlot = _slot;
        CheckSlot(enterSlotType);

        UI_Inventory getInventory = GetInventory(_dragSlotType);
        getInventory.SetInfomation(_slot);
    }

    public void OnPointerExit()
    {
        UI_Inventory getInventory = GetInventory(enterSlotType);// ���� �ִ� ���� ����
        getInventory.SetInfomation(null);

        enterSlotType = DragSlotType.None;
        enterSlot = null;
        CheckSlot(DragSlotType.None);
    }

    public void OnTreshBox()
    {
        selectSlot = null;
        selectItemClass = null;
        selectSlotType = DragSlotType.None;
        CheckSlot(DragSlotType.None);// üũ ����

        onDrag = false;
    }

    //===========================================================================================================================
    // �׼�
    //===========================================================================================================================

    void DragSlot()
    {
        CheckSlot(enterSlotType);

        if (slotMoving != null)
            StopCoroutine(slotMoving);
        slotMoving = StartCoroutine(DragingSlot());
    }

    IEnumerator DragingSlot()
    {
        iconImage.gameObject.SetActive(true);
        iconImage.sprite = selectItemClass.item.icon;
        while (onDrag == true)
        {
            iconImage.transform.position = Input.mousePosition;
            yield return null;
        }
        iconImage.gameObject.SetActive(false);
    }

    void SetDragRotate()
    {
        if (selectItemClass == null)
            return;

        selectItemClass.SetRotate(90f);
        CheckSlot(enterSlotType);
    }

    void EmptySlot(DragSlotType _dragSlotType)
    {
        UI_Inventory getInventory = GetInventory(_dragSlotType);
        getInventory.SetEmpty(selectSlot);
    }

    void SetOriginItemClass()
    {
        if (originItemClass == null)// ���� ��ġ ����
            originItemClass = new ItemClass();
        originItemClass.SetItemClass(selectItemClass);
    }

    UI_Inventory GetInventory(DragSlotType _dragSlotType)
    {
        switch (_dragSlotType)
        {
            case DragSlotType.None:
                return null;

            case DragSlotType.Inventory:
                return inventory;

            case DragSlotType.Shop:
                return shop;
        }
        return null;
    }

    IEnumerator MoneyWallet(float _money)
    {
        float prevMoney = moneyWallet;
        moneyWallet += _money;
        bool addMoney = (prevMoney < moneyWallet);
        bool moveMoney = true;
        while (moveMoney == true)
        {
            prevMoney = Mathf.Lerp(prevMoney, moneyWallet, 0.1f);
            moneyText.text = Mathf.Round(prevMoney).ToString();

            if (addMoney == true)// �Ǹ��� ���
            {
                if (prevMoney > moneyWallet)
                {
                    moveMoney = false;
                }
            }
            else if (prevMoney < moneyWallet)// ������ ���
            {
                moveMoney = false;
            }
            yield return null;
        }
    }

    //===========================================================================================================================
    // üũ �κ��丮
    //===========================================================================================================================

    void CheckSlot(DragSlotType _dragSlotType)
    {
        if (onDrag == false)
            return;

        UI_Inventory getInventory = GetInventory(_dragSlotType);
        if (getInventory == null)
        {
            // üũĭ ��� ����
            inventory.ClearCheckList();
            shop.ClearCheckList();
            return;
        }
        onCheck = getInventory.SetCheck(enterSlot, selectItemClass);
    }
}
