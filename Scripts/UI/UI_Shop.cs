using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Data_Manager;
using static UI_Inventory_Slot;
using static UI_Main;

public class UI_Shop : MonoBehaviour
{
    public CanvasStruct[] canvasStructs;
    public Button outButton;
    public GridLayoutGroup gridLayoutGroup;
    public Vector2Int inventorySize;
    public float slotSize;
    private UI_Inventory_Slot[,] allSlots;
    public UI_Inventory_Slot inventorySlot;

    public Data_Shop shopItem;

    public void SetStart()
    {
        outButton.onClick.AddListener(OutCanvas);
        OpenCanvas(false);

        SetInventory();
    }

    public void OpenCanvas(bool _open)
    {
        Game_Manager.current.inventory.OpenCanvas(_open);
        StartCoroutine(OpenCanvasMoving(canvasStructs, _open));

        if (_open == true)
            ChangeMode(UI_Inventory.BargainType.Shop);
        else
            ChangeMode(UI_Inventory.BargainType.None);
    }

    void ChangeMode(UI_Inventory.BargainType _mode)
    {
        Game_Manager.current.inventory.SetBargainType = _mode;
    }

    void OutCanvas()
    {
        OpenCanvas(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            SetFixedItem();
            SetRandomItem();
        }
    }

    void SetFixedItem()
    {
        string[] setID = shopItem.fixedID;
        for (int i = 0; i < setID.Length; i++)
        {
            ItemStruct item = Singleton_Data.INSTANCE.GetItemStruct(setID[i]);
            if (AddItem(item) == false)
            {
                break;// ��ĭ�� ������ �׸�
            }
        }
    }

    void SetRandomItem()
    {
        List<string> setID = new List<string>(shopItem.randomID);
        setID = P01_Utility.ShuffleList(setID, 0);

        // ������ �ݺ� ���� �ʰ� ����
        int amount = Random.Range(0, setID.Count);
        for (int i = 0; i < amount; i++)
        {
            ItemStruct item = Singleton_Data.INSTANCE.GetItemStruct(setID[i]);
            if (AddItem(item) == false)
            {
                break;// ��ĭ�� ������ �׸�
            }
        }
    }

    void SetInventory()
    {
        gridLayoutGroup.cellSize = Vector2.one * slotSize;
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = inventorySize.x;

        allSlots = new UI_Inventory_Slot[inventorySize.x, inventorySize.y];
        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                UI_Inventory_Slot inst = Instantiate(inventorySlot, gridLayoutGroup.transform);
                inst.SetStart(x, y);
                inst.SetEmpty();
                inst.dele_LeftClick = OnPointerLeftClick;
                inst.dele_RightClick = OnPointerRightClick;
                inst.dele_Enter = OnPointerEnter;
                inst.dele_Exit = OnPointerExit;
                allSlots[x, y] = inst;
            }
        }
    }

    bool AddItem(ItemStruct _item)
    {
        UI_Inventory_Slot slot = GetEmptySlot(_item);
        if (slot == null)
        {
            // �������� �� ���� ����
            Debug.LogWarning("�������� �� ���� ����");
            return false;
        }
        Debug.LogWarning("���� �̸� : " + slot.name);
        float setAngle = Random.Range(0, 4) * 90f;
        ItemClass itemClass = new ItemClass
        {
            item = _item,
            angle = setAngle,
            shape = _item.shape,
        };
        SetSlot(slot, itemClass);
        return true;
    }

    void SetEmpty(UI_Inventory_Slot _slot)// ����
    {
        Vector2Int[] shape = _slot.itemClass.shape;
        _slot.SetEmpty();// ���� ���� ����
        if (shape == null)
            return;
        // ������
        for (int i = 0; i < shape.Length; i++)
        {
            int slotX = _slot.slotNum.x + shape[i].x;
            int slotY = _slot.slotNum.y + shape[i].y;
            allSlots[slotX, slotY].SetEmpty();
        }
    }

    UI_Inventory_Slot GetEmptySlot(ItemStruct _item)
    {
        for (int y = 0; y < inventorySize.y; y++)
        {
            for (int x = 0; x < inventorySize.x; x++)
            {
                bool empty = true;
                UI_Inventory_Slot slot = allSlots[x, y];
                if (slot.empty == false)
                {
                    continue;
                }

                for (int i = 0; i < _item.shape.Length; i++)
                {
                    int slotX = slot.slotNum.x + _item.shape[i].x;
                    int slotY = slot.slotNum.y + _item.shape[i].y;
                    if (slotX < 0 || slotX >= inventorySize.x || slotY < 0 || slotY >= inventorySize.y)
                    {
                        empty = false;
                        break;
                    }
                    else
                    {
                        bool temp = allSlots[slotX, slotY].empty;
                        if (temp == false)
                        {
                            empty = false;
                            break;
                        }
                    }
                }
                if (empty == true)
                    return slot;
            }
        }
        return null;
    }

    void SetSlot(UI_Inventory_Slot _slot, ItemClass _itemClass)
    {
        _slot.SetBase(_itemClass);// ���� ����
        if (_itemClass == null)
            return;

        Vector2Int[] shape = _itemClass.shape;
        // ������
        for (int i = 0; i < shape.Length; i++)
        {
            int slotX = _slot.slotNum.x + shape[i].x;
            int slotY = _slot.slotNum.y + shape[i].y;
            allSlots[slotX, slotY].SetLink(_slot);
        }
    }

    public UI_Inventory_Slot prevSlot;
    public ItemClass prevItemClass;
    public void ReturnPrevPosition(bool _return)
    {
        if (_return == true)
            SetSlot(prevSlot, prevItemClass);// ���� ��ġ�� ������

        prevSlot = null;
        prevItemClass = null;
        Game_Manager.current.inventory.TryDragSlotType = UI_Inventory.DragSlotType.None;
    }

    void OnPointerLeftClick(UI_Inventory_Slot _slot)
    {
        UI_Inventory inventory = Game_Manager.current.inventory;
        if (inventory.TryDragSlotType == UI_Inventory.DragSlotType.Shop)//���� �������� ������ ���� ���
        {
            inventory.RemoveDragItem();
            ReturnPrevPosition(true);
            return;
        }

        if (inventory.TryDragSlotType == UI_Inventory.DragSlotType.Inventory)// �巡�׷� �Ǹ�
        {
            inventory.ShopDragSellItem();
            ReturnPrevPosition(false);
            return;
        }

        if (_slot.empty == true)
            return;

        // ���� ���� ���
        prevSlot = _slot.GetLinkSlot;
        prevItemClass = _slot.itemClass;

        ItemStruct item = _slot.itemClass.item;
        inventory.AddItem(item);
        inventory.TryDragSlotType = UI_Inventory.DragSlotType.Shop;
        SetEmpty(_slot.GetLinkSlot);
    }

    void OnPointerRightClick(UI_Inventory_Slot _slot)
    {
        if (_slot.empty == true || prevSlot != null)
            return;

        ItemStruct item = _slot.itemClass.item;
        if (Game_Manager.current.inventory.BuyItem(item) == true)// �Ǹ� ����
        {
            SetEmpty(_slot.GetLinkSlot);// ���� ����
        }
    }

    void OnPointerEnter(UI_Inventory_Slot _slot)
    {

    }

    void OnPointerExit()
    {

    }
}
