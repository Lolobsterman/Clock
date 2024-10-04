using System;
using UnityEngine;

public class ClockHandsAnimator : MonoBehaviour
{
    public ClockManager clockManager;
    public Transform hourHand;      // ��������� ��� ������� �������
    public Transform minuteHand;    // ��������� ��� �������� �������
    public Transform secondHand;    // ��������� ��� ��������� ������� (���� ���������)

    public bool isDraggingHourHand = false;     // ���� ��� �������������� ������� �������
    public bool isDraggingMinuteHand = false;   // ���� ��� �������������� �������� �������
    private bool isTimeUpdating = true;          // ���� ��� ����, ����� ������������� ���������� �������

    private Camera mainCamera;       // ������ ��� �������������� ���������
    private Vector3 clockCenter;     // ����� ����� ��� ���������� �����

    private DateTime currentTime;    // ������� �����

    void Start()
    {
        mainCamera = Camera.main;
        clockCenter = transform.position; // ����� ����� ����� ���������� �������, � �������� ���������� ��������
        currentTime = DateTime.Now;       // ������������� ������� �����
    }

    public void UpdateClock(DateTime newTime)
    {
        if (!isTimeUpdating) return; // ������������� ���������� �������, ���� ������������� �������

        // ��������� ������� �����
        currentTime = newTime;

        // ��������� ��������� ������� �� ������ �������� �������
        float hoursAngle = -((newTime.Hour % 12) + newTime.Minute / 60f) * 30f; // 12 ����� = 360 ��������
        float minutesAngle = -newTime.Minute * 6f;  // 60 ����� = 360 ��������
        hourHand.localRotation = Quaternion.Euler(0, 0, hoursAngle);
        minuteHand.localRotation = Quaternion.Euler(0, 0, minutesAngle);
        if (secondHand != null)
        {
            secondHand.localRotation = Quaternion.Euler(0, 0, -newTime.Second * 6f); // 60 ������ = 360 ��������
        }
    }

    void Update()
    {
        // ����������� ������� �� ������� � �� �����������
        if (Input.GetMouseButtonDown(0))
        {
            OnStartDragging();
        }
        else if (Input.GetMouseButton(0)) // ���������� ������� ���� ��� ����������� �������
        {
            if (isDraggingHourHand || isDraggingMinuteHand)
            {
                DragHand(); // �������������� �������, ���� ������
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnStopDragging();
        }
    }

    private void OnStartDragging()
    {
        // ���������, �������� �� ������ � ���� ������� (���������� Raycast ��� 2D)
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            // ���������, ����� ������ ��� ����� (������� ��� �������� �������)
            if (hit.collider.transform == hourHand)
            {
                isDraggingHourHand = true;
                isTimeUpdating = false; // ������������� ���������� ������� ��� ��������������
            }
            else if (hit.collider.transform == minuteHand)
            {
                isDraggingMinuteHand = true;
                isTimeUpdating = false; // ������������� ���������� ������� ��� ��������������
            }
        }
    }

    private void OnStopDragging()
    {
        // ������������� ��������������
        isDraggingHourHand = false;
        isDraggingMinuteHand = false;
        isTimeUpdating = true; // ������������ ���������� �������

        // ��������� ������� ����� �� ������ ��������� �������
        currentTime = GetCurrentTimeFromHands();

        // ��������� ������� �� ������ ������ ������� (����� ���������, ��� ��� ��������� �����������)
        clockManager.lastSyncedTime = currentTime;
        clockManager.timeSinceLastSync = 0;
    }

    private void DragHand()
    {
        // �������� ������� ���� � ������� �����������
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;  // ��� 2D

        // ��������� ���� ����� ������� ����� � ���������� ����
        Vector3 direction = mousePosition - clockCenter;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // -90 ��� ���������� ����������

        if (isDraggingHourHand)
        {
            // ����������� ���� ��� ������� ������� (12 ����� = 360�)
            float hoursAngle = angle % 360f;
            if (hoursAngle < 0) hoursAngle += 360f;
            hoursAngle = Mathf.Round(hoursAngle / 30f) * 30f; // �������� � ����� 30� (1 ���)
            hourHand.localRotation = Quaternion.Euler(0, 0, hoursAngle);
        }
        else if (isDraggingMinuteHand)
        {
            // ����������� ���� ��� �������� ������� (60 ����� = 360�)
            float minutesAngle = angle % 360f;
            if (minutesAngle < 0) minutesAngle += 360f;
            minutesAngle = Mathf.Round(minutesAngle / 6f) * 6f; // �������� � ����� 6� (1 ������)
            minuteHand.localRotation = Quaternion.Euler(0, 0, minutesAngle);
        }
    }

    public DateTime GetCurrentTimeFromHands()
    {
        // �������� ����� �� ������ ��������� �������
        float hourAngle = -hourHand.localRotation.eulerAngles.z;
        float minuteAngle = -minuteHand.localRotation.eulerAngles.z;

        int hours = Mathf.FloorToInt((hourAngle + 360f) / 30f) % 12;
        int minutes = Mathf.FloorToInt((minuteAngle + 360f) / 6f) % 60;

        // ���������� ������� ������� ������ (AM/PM) �� ������ �������� �������
        if (currentTime.Hour >= 12)
        {
            hours += 12; // ��������� 12 �����, ���� ������� ����� � ��������� PM
        }

        // ���������� ����� �����, ��������� ��� �� ������ ��������� �������
        return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hours, minutes, 0);
    }
}
