using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class IconDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GridLayoutGroup _gridLayoutGroup;
    [SerializeField] private RectTransform _gridTransform;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private InputActionReference _rotateAction;

    private Vector3 _initialPosition;
    private Vector3 _initialEulerAngles;
    private Vector3 _currentMousePosition;
    private Vector2 _mouseOffset;
    private Vector2 _selectedImageCellCenterOffset;
    private bool _isDragging;
    private float _cellSize => _gridLayoutGroup.cellSize.x;

    private void OnEnable() => BindInputActions();
    private void OnDisable() => UnbindInputActions();

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _initialPosition = _rectTransform.position;
        _initialEulerAngles = _rectTransform.eulerAngles;

        // 90도, 180도 회전 구분
        int rotation = Mathf.RoundToInt(_rectTransform.eulerAngles.z / 90);
        bool isSizeDeltaRotated = rotation % 2 != 0;

        float width = isSizeDeltaRotated ? _rectTransform.sizeDelta.y : _rectTransform.sizeDelta.x;
        float height = isSizeDeltaRotated ? _rectTransform.sizeDelta.x : _rectTransform.sizeDelta.y;

        // 이미지의 좌상단 위치
        float leftTopPosX = _rectTransform.position.x - width / 2;
        float leftTopPosY = _rectTransform.position.y + height / 2;

        Vector2 leftTopPosition = new Vector2(leftTopPosX, leftTopPosY);
        // 왼쪽 위 이미지 셀이 (0, 0)인 기준. (eventData.position이 아닌 pressPosition을 사용해야 함)
        Vector2Int mouseCoord = new Vector2Int(Mathf.FloorToInt((eventData.pressPosition.x - leftTopPosition.x) / _cellSize), Mathf.FloorToInt((leftTopPosition.y - eventData.pressPosition.y) / _cellSize));

        Debug.Log($"Mouse Coord {mouseCoord}");
        // 마우스가 집은 이미지의 칸 중앙 위치
        Vector2 _selectedImageCellCenter = new Vector2(leftTopPosition.x + mouseCoord.x * _cellSize + _cellSize / 2, leftTopPosition.y - mouseCoord.y * _cellSize - _cellSize / 2);

        float mouseOffsetX = eventData.pressPosition.x - _rectTransform.position.x;
        float mouseOffsetY = eventData.pressPosition.y - _rectTransform.position.y;

        float selectedImangeCellCenterOffsetX = _selectedImageCellCenter.x - _rectTransform.position.x;
        float selectedImangeCellCenterOffsetY = _selectedImageCellCenter.y - _rectTransform.position.y;

        _mouseOffset = new Vector2(mouseOffsetX, mouseOffsetY);
        _selectedImageCellCenterOffset = new Vector2(selectedImangeCellCenterOffsetX, selectedImangeCellCenterOffsetY);

        Debug.Log($"Drag Started : MouseOffset {_mouseOffset} SelectedImageCellCenterOffset {_selectedImageCellCenterOffset}");
    }

    public void OnDrag(PointerEventData eventData)
    {
        float posX = eventData.position.x - _mouseOffset.x;
        float posY = eventData.position.y - _mouseOffset.y;
        _currentMousePosition = eventData.position;

        _rectTransform.position = new Vector3(posX, posY, 0f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;

        // 실제 그리드의 좌상단 위치
        Vector2 leftTopGridPosition = new Vector2(
            Screen.width / 2 - _gridTransform.sizeDelta.x / 2 + _gridTransform.anchoredPosition.x,
            Screen.height / 2 + _gridTransform.sizeDelta.y / 2 + _gridTransform.anchoredPosition.y
            );

        // 마우스가 위치한 그리드 좌표
        Vector2Int currentCoord = new Vector2Int(Mathf.FloorToInt((eventData.position.x - leftTopGridPosition.x)/ _cellSize), Mathf.FloorToInt((leftTopGridPosition.y - eventData.position.y) / _cellSize));

        // 그리드 범위 밖으로 나가면 원래 상태로 복귀
        if (currentCoord.x < 0 || currentCoord.x >= (_gridTransform.sizeDelta.x / _cellSize)
            || currentCoord.y < 0 || currentCoord.y >= (_gridTransform.sizeDelta.y / _cellSize)) 
        {
            _rectTransform.position = _initialPosition;
            _rectTransform.eulerAngles = _initialEulerAngles;
            return;
        }

        float posX = leftTopGridPosition.x + _cellSize * currentCoord.x + _cellSize / 2;
        float posY = leftTopGridPosition.y - _cellSize * currentCoord.y - _cellSize / 2;

        _rectTransform.position = new Vector3(posX - _selectedImageCellCenterOffset.x, posY - _selectedImageCellCenterOffset.y, 0);

        Debug.Log($"Drage Finished : Current Coord {currentCoord} RectTransform Position {_rectTransform.position}");
    }

    private void Rotate(InputAction.CallbackContext context)
    {
        if (!_isDragging) return;

        // 시계 방향 90도 회전
        _rectTransform.Rotate(new Vector3(0f, 0f, -90f));

        // Offset 회전
        _mouseOffset = new Vector3(_mouseOffset.y, -_mouseOffset.x, 0f);
        _selectedImageCellCenterOffset = new Vector3(_selectedImageCellCenterOffset.y, -_selectedImageCellCenterOffset.x, 0f);

        float posX = _currentMousePosition.x - _mouseOffset.x;
        float posY = _currentMousePosition.y - _mouseOffset.y;
        _rectTransform.position = new Vector3(posX, posY, 0f);

        Debug.Log($"Rotated : MouseOffset {_mouseOffset} SelectedImageCellCenterOffset {_selectedImageCellCenterOffset}");
    }

    private void BindInputActions()
    {
        _rotateAction.action.Enable();
        _rotateAction.action.performed += Rotate;
    }

    private void UnbindInputActions()
    {
        _rotateAction.action.Disable();
        _rotateAction.action.performed -= Rotate;
    }
}