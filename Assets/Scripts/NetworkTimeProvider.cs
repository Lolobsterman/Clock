using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkTimeProvider : MonoBehaviour, ITimeProvider
{
    private const string timeServerUrl = "http://worldtimeapi.org/api/timezone/Etc/UTC";
    private DateTime currentTime;
    private bool timeReceived;

    public DateTime GetCurrentTime()
    {
        if (!timeReceived)
        {
            // Если время ещё не получено, вернём текущее время устройства как запасной вариант
            return DateTime.Now;
        }

        return currentTime;
    }

    void Start()
    {
        StartCoroutine(GetTimeFromServer());
    }

    private IEnumerator GetTimeFromServer()
    {
        UnityWebRequest request = UnityWebRequest.Get(timeServerUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var json = request.downloadHandler.text;
            var timeData = JsonUtility.FromJson<TimeData>(json);
            currentTime = DateTime.Parse(timeData.datetime);
            timeReceived = true;
        }
        else
        {
            Debug.LogError("Ошибка получения времени с сервера: " + request.error);
        }
    }

    [Serializable]
    private class TimeData
    {
        public string datetime;
    }
}
