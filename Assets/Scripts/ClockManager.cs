using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClockManager : MonoBehaviour
{
    public ClockHandsAnimator clockHandsAnimator;   // Аниматор стрелок
    public DigitalClockDisplay digitalClockDisplay; // Электронные часы
    public ITimeProvider timeProvider;             // Провайдер времени (например, сеть)

    public TMP_InputField manualTimeInputField;        // Поле для ввода времени
    public Button setManualTimeButton;             // Кнопка для установки времени вручную
    public Button syncWithServerButton;            // Кнопка для возврата к синхронизации с сервером

    public DateTime lastSyncedTime;               // Последнее синхронизированное или введённое вручную время
    public float timeSinceLastSync;               // Время, прошедшее с момента последней синхронизации или ввода
    private const float syncInterval = 3600f;      // Интервал синхронизации с сервером (1 час)

    private bool isManualTimeMode = false;         // Флаг ручного режима времени

    void Start()
    {
        timeProvider = GetComponent<ITimeProvider>();

        // Подключаем функции к кнопкам
        setManualTimeButton.onClick.AddListener(OnSetManualTime);
        syncWithServerButton.onClick.AddListener(OnSyncWithServer);

        // Первоначальная синхронизация времени
        SyncTime();
    }

    void Update()
    {
        timeSinceLastSync += Time.deltaTime;

        if (!isManualTimeMode)
        {
            // Если не ручной режим, проверяем, нужно ли синхронизироваться
            if (timeSinceLastSync >= syncInterval)
            {
                SyncTime();
            }

            DateTime currentTime = lastSyncedTime.AddSeconds(timeSinceLastSync);

            // Обновляем аналоговые и электронные часы
            clockHandsAnimator.UpdateClock(currentTime);
            digitalClockDisplay.UpdateClock(currentTime);
        }
        else
        {
            // В ручном режиме тоже продолжаем обновлять время
            DateTime currentTime = lastSyncedTime.AddSeconds(timeSinceLastSync);
            clockHandsAnimator.UpdateClock(currentTime);
            digitalClockDisplay.UpdateClock(currentTime);
        }

        // Проверяем, если пользователь перемещает стрелки
        if (Input.GetMouseButtonUp(0) && (clockHandsAnimator.isDraggingHourHand || clockHandsAnimator.isDraggingMinuteHand))
        {
            OnSetTimeByDraggingHands();
        }
    }

    private void SyncTime()
    {
        // Синхронизация с сервером и сброс ручного режима
        lastSyncedTime = timeProvider.GetCurrentTime();
        timeSinceLastSync = 0f;
        isManualTimeMode = false;

        // Обновляем сразу же после получения времени
        clockHandsAnimator.UpdateClock(lastSyncedTime);
        digitalClockDisplay.UpdateClock(lastSyncedTime);
    }

    private void OnSetManualTime()
    {
        // Получаем введённое время
        string manualTimeString = manualTimeInputField.text;

        // Попытка распарсить введённое время в формат DateTime
        if (DateTime.TryParse(manualTimeString, out DateTime manualTime))
        {
            // Переключаемся в режим ручного времени
            lastSyncedTime = manualTime;
            timeSinceLastSync = 0f;
            isManualTimeMode = true;

            // Обновляем отображение времени и продолжаем отсчёт
            clockHandsAnimator.UpdateClock(manualTime);
            digitalClockDisplay.UpdateClock(manualTime);
        }
        else
        {
            Debug.LogError("Неправильный формат времени. Используйте формат HH:mm:ss.");
        }
    }

    private void OnSetTimeByDraggingHands()
    {
        // Устанавливаем время, основываясь на положении стрелок
        DateTime draggedTime = clockHandsAnimator.GetCurrentTimeFromHands();

        // Обновляем lastSyncedTime на новое время, полученное из положения стрелок
        lastSyncedTime = draggedTime;
        timeSinceLastSync = 0f; // Сбрасываем таймер
        isManualTimeMode = true; // Устанавливаем режим ручного времени

        // Обновляем часы с нового времени
        clockHandsAnimator.UpdateClock(draggedTime);
        digitalClockDisplay.UpdateClock(draggedTime);
    }

    private void OnSyncWithServer()
    {
        // Возвращаемся к синхронизации с сервером
        SyncTime();
    }
}