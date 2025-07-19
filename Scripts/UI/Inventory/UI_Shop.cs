using System.Collections.Generic;
using UnityEngine;
using static Data_Manager;

public class UI_Shop : UI_Inventory
{

    //===========================================================================================================================
    // ����
    //===========================================================================================================================

    [Header("- Shop")]
    public Data_Shop shopItem;

    public override void SetStart()
    {
        base.SetStart();
        //SetFixedItem();
        //SetRandomItem();
    }
    public override void OpenCanvas(bool _open)
    {
        base.OpenCanvas(_open);
    }

    void SetFixedItem()
    {
        string[] setID = shopItem.fixedID;
        for (int i = 0; i < setID.Length; i++)
        {
            ItemStruct item = Singleton_Data.INSTANCE.GetItemStruct(setID[i]);
            //if (AddItem(item) == false)
            //{
            //    break;// ��ĭ�� ������ �׸�
            //}
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
            //if (AddItem(item) == false)
            //{
            //    break;// ��ĭ�� ������ �׸�
            //}
        }
    }
}
