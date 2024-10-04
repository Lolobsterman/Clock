using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClockManager : MonoBehaviour
{
    public ClockHandsAnimator clockHandsAnimator;   // �������� �������
    public DigitalClockDisplay digitalClockDisplay; // ����������� ����
    public ITimeProvider timeProvider;             // ��������� ������� (��������, ����)

    public TMP_InputField manualTimeInputField;        // ���� ��� ����� �������
    public Button setManualTimeButton;             // ������ ��� ��������� ������� �������
    public Button syncWithServerButton;            // ������ ��� �������� � ������������� � ��������

    public DateTime lastSyncedTime;               // ��������� ������������������ ��� �������� ������� �����
    public float timeSinceLastSync;               // �����, ��������� � ������� ��������� ������������� ��� �����
    private const float syncInterval = 3600f;      // �������� ������������� � �������� (1 ���)

    private bool isManualTimeMode = false;         // ���� ������� ������ �������

    void Start()
    {
        timeProvider = GetComponent<ITimeProvider>();

        // ���������� ������� � �������
        setManualTimeButton.onClick.AddListener(OnSetManualTime);
        syncWithServerButton.onClick.AddListener(OnSyncWithServer);

        // �������������� ������������� �������
        SyncTime();
    }

    void Update()
    {
        timeSinceLastSync += Time.deltaTime;

        if (!isManualTimeMode)
        {
            // ���� �� ������ �����, ���������, ����� �� ������������������
            if (timeSinceLastSync >= syncInterval)
            {
                SyncTime();
            }

            DateTime currentTime = lastSyncedTime.AddSeconds(timeSinceLastSync);

            // ��������� ���������� � ����������� ����
            clockHandsAnimator.UpdateClock(currentTime);
            digitalClockDisplay.UpdateClock(currentTime);
        }
        else
        {
            // � ������ ������ ���� ���������� ��������� �����
            DateTime currentTime = lastSyncedTime.AddSeconds(timeSinceLastSync);
            clockHandsAnimator.UpdateClock(currentTime);
            digitalClockDisplay.UpdateClock(currentTime);
        }

        // ���������, ���� ������������ ���������� �������
        if (Input.GetMouseButtonUp(0) && (clockHandsAnimator.isDraggingHourHand || clockHandsAnimator.isDraggingMinuteHand))
        {
            OnSetTimeByDraggingHands();
        }
    }

    private void SyncTime()
    {
        // ������������� � �������� � ����� ������� ������
        lastSyncedTime = timeProvider.GetCurrentTime();
        timeSinceLastSync = 0f;
        isManualTimeMode = false;

        // ��������� ����� �� ����� ��������� �������
        clockHandsAnimator.UpdateClock(lastSyncedTime);
        digitalClockDisplay.UpdateClock(lastSyncedTime);
    }

    private void OnSetManualTime()
    {
        // �������� �������� �����
        string manualTimeString = manualTimeInputField.text;

        // ������� ���������� �������� ����� � ������ DateTime
        if (DateTime.TryParse(manualTimeString, out DateTime manualTime))
        {
            // ������������� � ����� ������� �������
            lastSyncedTime = manualTime;
            timeSinceLastSync = 0f;
            isManualTimeMode = true;

            // ��������� ����������� ������� � ���������� ������
            clockHandsAnimator.UpdateClock(manualTime);
            digitalClockDisplay.UpdateClock(manualTime);
        }
        else
        {
            Debug.LogError("������������ ������ �������. ����������� ������ HH:mm:ss.");
        }
    }

    private void OnSetTimeByDraggingHands()
    {
        // ������������� �����, ����������� �� ��������� �������
        DateTime draggedTime = clockHandsAnimator.GetCurrentTimeFromHands();

        // ��������� lastSyncedTime �� ����� �����, ���������� �� ��������� �������
        lastSyncedTime = draggedTime;
        timeSinceLastSync = 0f; // ���������� ������
        isManualTimeMode = true; // ������������� ����� ������� �������

        // ��������� ���� � ������ �������
        clockHandsAnimator.UpdateClock(draggedTime);
        digitalClockDisplay.UpdateClock(draggedTime);
    }

    private void OnSyncWithServer()
    {
        // ������������ � ������������� � ��������
        SyncTime();
    }
}