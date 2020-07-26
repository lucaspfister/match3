using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Piece : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] RectTransform rectTransform;
    [SerializeField] Image imgSelected;
    [SerializeField] Image icon;

    public int value { get; set; }
    public Vector2Int boardCoordinate { get; set; }

    bool isSelected = false;
    CanvasGroup canvasGroup;

    Transform dragStartParent;
    Vector3 dragStartPos;
    bool dragSuccess = false;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Set(int value, Color color, Vector2Int boardCoordinate, Vector2 localPos)
    {
        this.value = value;
        icon.color = color;
        this.boardCoordinate = boardCoordinate;
        transform.localPosition = localPos;
        transform.localScale = Vector3.one;
    }

    void Select()
    {
        isSelected = true;
        imgSelected.color = Color.white;
        PieceManager.Instance.SelectPiece(this);
    }

    public void Deselect()
    {
        isSelected = false;
        imgSelected.color = Color.clear;
    }

    void ResetPosition()
    {
        transform.parent = dragStartParent;
        transform.position = dragStartPos;
    }

    public void DragSuccess()
    {
        dragSuccess = true;
        ResetPosition();
    }

    #region Events 

    public void OnPointerDown(PointerEventData eventData)
    {
        Select();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        imgSelected.color = Color.clear;
        dragSuccess = false;
        dragStartParent = transform.parent;
        dragStartPos = transform.position;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        Deselect();

        if (!dragSuccess)
        {
            ResetPosition();
            PieceManager.Instance.DeselectPiece();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        PieceManager.Instance.SelectPiece(this, true);
    }

    #endregion
}
