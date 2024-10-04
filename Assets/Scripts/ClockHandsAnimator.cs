using System;
using UnityEngine;

public class ClockHandsAnimator : MonoBehaviour
{
    public ClockManager clockManager;
    public Transform hourHand;      // Трансформ для часовой стрелки
    public Transform minuteHand;    // Трансформ для минутной стрелки
    public Transform secondHand;    // Трансформ для секундной стрелки (если требуется)

    public bool isDraggingHourHand = false;     // Флаг для перетаскивания часовой стрелки
    public bool isDraggingMinuteHand = false;   // Флаг для перетаскивания минутной стрелки
    private bool isTimeUpdating = true;          // Флаг для того, чтобы останавливать обновление времени

    private Camera mainCamera;       // Камера для преобразования координат
    private Vector3 clockCenter;     // Центр часов для вычисления углов

    private DateTime currentTime;    // Текущее время

    void Start()
    {
        mainCamera = Camera.main;
        clockCenter = transform.position; // Центр часов будет положением объекта, к которому прикреплен аниматор
        currentTime = DateTime.Now;       // Устанавливаем текущее время
    }

    public void UpdateClock(DateTime newTime)
    {
        if (!isTimeUpdating) return; // Останавливаем обновление времени, если перетаскиваем стрелки

        // Обновляем текущее время
        currentTime = newTime;

        // Обновляем положение стрелок на основе текущего времени
        float hoursAngle = -((newTime.Hour % 12) + newTime.Minute / 60f) * 30f; // 12 часов = 360 градусов
        float minutesAngle = -newTime.Minute * 6f;  // 60 минут = 360 градусов
        hourHand.localRotation = Quaternion.Euler(0, 0, hoursAngle);
        minuteHand.localRotation = Quaternion.Euler(0, 0, minutesAngle);
        if (secondHand != null)
        {
            secondHand.localRotation = Quaternion.Euler(0, 0, -newTime.Second * 6f); // 60 секунд = 360 градусов
        }
    }

    void Update()
    {
        // Отслеживаем нажатие на стрелки и их перемещение
        if (Input.GetMouseButtonDown(0))
        {
            OnStartDragging();
        }
        else if (Input.GetMouseButton(0)) // Удерживаем нажатие мыши для перемещения стрелки
        {
            if (isDraggingHourHand || isDraggingMinuteHand)
            {
                DragHand(); // Перетаскивание стрелки, если начато
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnStopDragging();
        }
    }

    private void OnStartDragging()
    {
        // Проверяем, попадает ли курсор в зону стрелки (используем Raycast для 2D)
        Vector2 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            // Проверяем, какой объект был нажат (часовая или минутная стрелка)
            if (hit.collider.transform == hourHand)
            {
                isDraggingHourHand = true;
                isTimeUpdating = false; // Останавливаем обновление времени при перетаскивании
            }
            else if (hit.collider.transform == minuteHand)
            {
                isDraggingMinuteHand = true;
                isTimeUpdating = false; // Останавливаем обновление времени при перетаскивании
            }
        }
    }

    private void OnStopDragging()
    {
        // Останавливаем перетаскивание
        isDraggingHourHand = false;
        isDraggingMinuteHand = false;
        isTimeUpdating = true; // Возобновляем обновление времени

        // Обновляем текущее время на основе положения стрелок
        currentTime = GetCurrentTimeFromHands();

        // Обновляем стрелки на основе нового времени (чтобы убедиться, что они корректно установлены)
        clockManager.lastSyncedTime = currentTime;
        clockManager.timeSinceLastSync = 0;
    }

    private void DragHand()
    {
        // Получаем позицию мыши в мировых координатах
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;  // Для 2D

        // Вычисляем угол между центром часов и положением мыши
        Vector3 direction = mousePosition - clockCenter;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f; // -90 для корректной ориентации

        if (isDraggingHourHand)
        {
            // Преобразуем угол для часовой стрелки (12 часов = 360°)
            float hoursAngle = angle % 360f;
            if (hoursAngle < 0) hoursAngle += 360f;
            hoursAngle = Mathf.Round(hoursAngle / 30f) * 30f; // Привязка к шагам 30° (1 час)
            hourHand.localRotation = Quaternion.Euler(0, 0, hoursAngle);
        }
        else if (isDraggingMinuteHand)
        {
            // Преобразуем угол для минутной стрелки (60 минут = 360°)
            float minutesAngle = angle % 360f;
            if (minutesAngle < 0) minutesAngle += 360f;
            minutesAngle = Mathf.Round(minutesAngle / 6f) * 6f; // Привязка к шагам 6° (1 минута)
            minuteHand.localRotation = Quaternion.Euler(0, 0, minutesAngle);
        }
    }

    public DateTime GetCurrentTimeFromHands()
    {
        // Получаем время на основе положения стрелок
        float hourAngle = -hourHand.localRotation.eulerAngles.z;
        float minuteAngle = -minuteHand.localRotation.eulerAngles.z;

        int hours = Mathf.FloorToInt((hourAngle + 360f) / 30f) % 12;
        int minutes = Mathf.FloorToInt((minuteAngle + 360f) / 6f) % 60;

        // Определяем текущий часовой период (AM/PM) на основе текущего времени
        if (currentTime.Hour >= 12)
        {
            hours += 12; // Добавляем 12 часов, если текущее время в диапазоне PM
        }

        // Возвращаем новое время, установив его на основе положения стрелок
        return new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, hours, minutes, 0);
    }
}
