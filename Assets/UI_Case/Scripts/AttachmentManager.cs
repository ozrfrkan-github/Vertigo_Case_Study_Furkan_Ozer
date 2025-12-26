using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum AttachmentType { Sight, Magazine, Tactical, Stock, Barrel }
public enum Rarity { Default, Common, Rare, Epic, Legendary }

[Serializable]
public class AttachmentItem
{
    public string id;                 
    public AttachmentType type;        
    public Rarity rarity;             
    public Sprite icon;               
    public GameObject meshObject;     
}

[Serializable]
public class IconSlot
{
  
    public GameObject root;

    public Button button;
    public Image image;               
}

[Serializable]
public class RarityRow
{
    public Rarity rarity;
    [Tooltip("Rarity Roots)")]
    public GameObject rowRoot;

    public List<IconSlot> slots = new();
}

public class AttachmentManager : MonoBehaviour


{
    [Header("DATA")]
    public List<AttachmentItem> items = new();

    [Header("UI")]
    public List<RarityRow> rarityRows = new();


    private AttachmentType _currentType;
    private AttachmentItem _currentSelected;


    private readonly Dictionary<Button, AttachmentItem> _buttonToItem = new();

    public GameObject attachmentSelectionRoot;

    private AttachmentItem _previewItem;


    private readonly Dictionary<AttachmentType, AttachmentItem> _equippedByType = new();

    private void Awake()
    {
        
        foreach(var row in rarityRows)
        {
            foreach(var slot in row.slots)
            {
                if(slot == null || slot.button == null) continue;

                
                slot.button.onClick.RemoveAllListeners();

                slot.button.onClick.AddListener(() =>
                {
                    if(_buttonToItem.TryGetValue(slot.button, out var item) && item != null)
                        SelectItem(item);
                });

            }
        }
    }

 
    
    public void OpenSight() => SelectType(AttachmentType.Sight);
    public void OpenMagazine() => SelectType(AttachmentType.Magazine);
    public void OpenTactical() => SelectType(AttachmentType.Tactical);
    public void OpenStock() => SelectType(AttachmentType.Stock);
    public void OpenBarrel() => SelectType(AttachmentType.Barrel);

    public void EquipCurrentPreview()
    {
        if(_previewItem == null) return;

        _equippedByType[_currentType] = _previewItem;
   
    }
    public void CloseAttachmentSelection()
    {

        ApplyAllEquippedToMeshes();

        if(attachmentSelectionRoot != null)
            attachmentSelectionRoot.SetActive(false);
    }

    private void ApplyAllEquippedToMeshes()
    {
     
        foreach(var it in items)
            if(it != null && it.meshObject != null)
                it.meshObject.SetActive(false);

       
        foreach(var kvp in _equippedByType)
        {
            var equipped = kvp.Value;
            if(equipped != null && equipped.meshObject != null)
                equipped.meshObject.SetActive(true);
        }
    }


    public void SelectType(AttachmentType type)
    {
        if(attachmentSelectionRoot != null && !attachmentSelectionRoot.activeSelf)
            attachmentSelectionRoot.SetActive(true);

    
        if(_equippedByType.TryGetValue(type, out var equipped) && equipped != null)
        {
            SelectItem(equipped);      
            _previewItem = equipped;   
        }
        else
        {
            _previewItem = null;
        }

        _currentType = type;

       
        _buttonToItem.Clear();
        ClearAllSlots();

       
        var grouped = new Dictionary<Rarity, List<AttachmentItem>>();
        foreach(var it in items)
        {
            if(it == null || it.type != type) continue;
            if(!grouped.TryGetValue(it.rarity, out var list))
            {
                list = new List<AttachmentItem>();
                grouped[it.rarity] = list;
            }
            list.Add(it);
        }

     
        foreach(var row in rarityRows)
        {
            if(row == null) continue;

            grouped.TryGetValue(row.rarity, out var list);
            list ??= new List<AttachmentItem>();

            bool anyItemInThisRow = list.Count > 0;
            if(row.rowRoot != null)
                row.rowRoot.SetActive(anyItemInThisRow);

            for(int i = 0; i < row.slots.Count; i++)
            {
                var slot = row.slots[i];
                if(slot == null || slot.button == null || slot.image == null) continue;

                bool hasItem = i < list.Count;
                if(hasItem)
                {
                    var item = list[i];
                    slot.image.sprite = item.icon;
                    slot.button.interactable = true;
                    _buttonToItem[slot.button] = item;

                    if(slot.root != null) slot.root.SetActive(true);
                    else slot.button.gameObject.SetActive(true);
                }
              
            }
        }

      
        AutoSelectFirstAvailable(type);
    }

    private void SelectItem(AttachmentItem item)
    {
        if(item == null) return;

   
        foreach(var it in items)
        {
            if(it == null || it.type != item.type) continue;
            if(it.meshObject != null) it.meshObject.SetActive(false);
        }

      
        if(item.meshObject != null) item.meshObject.SetActive(true);

        _previewItem = item;
        _currentSelected = item;
        
    }

    private void AutoSelectFirstAvailable(AttachmentType type)
    {
    
        var order = new[] { Rarity.Default, Rarity.Common, Rarity.Rare, Rarity.Epic, Rarity.Legendary };

        foreach(var r in order)
        {
            foreach(var it in items)
            {
                if(it != null && it.type == type && it.rarity == r)
                {
                    SelectItem(it);
                    return;
                }
            }
        }
    }
    private void Start()
    {
        if(attachmentSelectionRoot != null)
            attachmentSelectionRoot.SetActive(false);

        ApplyAllEquippedToMeshes();
    }

    private void ClearAllSlots()
    {
        foreach(var row in rarityRows)
        {
            if(row == null) continue;
            foreach(var slot in row.slots)
            {
                if(slot == null || slot.button == null || slot.image == null) continue;

                slot.image.sprite = null;
                slot.button.interactable = false;

             
            }
        }
    }
}
