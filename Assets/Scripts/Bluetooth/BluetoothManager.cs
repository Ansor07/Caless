using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
/// <summary>
/// Bluetooth мультиплеер для Android.
/// Использует Android Bluetooth API через AndroidJavaObject.
/// Поддерживает хостинг и подключение, а также чат.
/// </summary>
public class BluetoothManager : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;
 
    public bool IsConnected { get; private set; } = false;
    public bool IsHost { get; private set; } = false;
    public string StatusMessage { get; private set; } = "";
 
    // Чат
    public List<string> chatMessages = new List<string>();
 
    // Callback
    private Action<string> onMoveReceived;
    private Action<string> onChatReceived;
    private Action onConnected;
    private Action onDisconnected;
 
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject bluetoothAdapter;
    private AndroidJavaObject serverSocket;
    private AndroidJavaObject clientSocket;
    private AndroidJavaObject inputStream;
    private AndroidJavaObject outputStream;
#endif
 
    public void Initialize(Action<string> onMove, Action<string> onChat,
                           Action onConn, Action onDisconn)
    {
        onMoveReceived = onMove;
        onChatReceived = onChat;
        onConnected = onConn;
        onDisconnected = onDisconn;
    }
 
    public void StartHost()
    {
        IsHost = true;
        StatusMessage = "Ожидание подключения...";
 
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(HostCoroutine());
#else
        StatusMessage = "Bluetooth доступен только на Android";
#endif
    }
 
    public void StartClient()
    {
        IsHost = false;
        StatusMessage = "Поиск хоста...";
 
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(ClientCoroutine());
#else
        StatusMessage = "Bluetooth доступен только на Android";
#endif
    }
 
#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator HostCoroutine()
    {
        try
        {
            using (AndroidJavaClass btClass = new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
            {
                bluetoothAdapter = btClass.CallStatic<AndroidJavaObject>("getDefaultAdapter");
                if (bluetoothAdapter == null)
                {
                    StatusMessage = "Bluetooth не поддерживается";
                    yield break;
                }
 
                if (!bluetoothAdapter.Call<bool>("isEnabled"))
                {
                    StatusMessage = "Включите Bluetooth";
                    yield break;
                }
 
                string uuid = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
                AndroidJavaObject uuidObj = new AndroidJavaClass("java.util.UUID")
                    .CallStatic<AndroidJavaObject>("fromString", uuid);
 
                serverSocket = bluetoothAdapter.Call<AndroidJavaObject>(
                    "listenUsingRfcommWithServiceRecord", "Caless", uuidObj);
 
                StatusMessage = "Ожидание игрока...";
            }
        }
        catch (Exception e)
        {
            StatusMessage = "Ошибка: " + e.Message;
        }
 
        yield return StartCoroutine(WaitForConnection());
    }
 
    private IEnumerator WaitForConnection()
    {
        bool connected = false;
        while (!connected)
        {
            try
            {
                if (serverSocket != null)
                {
                    clientSocket = serverSocket.Call<AndroidJavaObject>("accept", 1000);
                    if (clientSocket != null)
                    {
                        inputStream = clientSocket.Call<AndroidJavaObject>("getInputStream");
                        outputStream = clientSocket.Call<AndroidJavaObject>("getOutputStream");
                        connected = true;
                        IsConnected = true;
                        StatusMessage = "Подключено!";
                        onConnected?.Invoke();
                    }
                }
            }
            catch (Exception) { }
            yield return new WaitForSeconds(0.5f);
        }
 
        StartCoroutine(ReadLoop());
    }
 
    private IEnumerator ClientCoroutine()
    {
        try
        {
            using (AndroidJavaClass btClass = new AndroidJavaClass("android.bluetooth.BluetoothAdapter"))
            {
                bluetoothAdapter = btClass.CallStatic<AndroidJavaObject>("getDefaultAdapter");
                if (bluetoothAdapter == null || !bluetoothAdapter.Call<bool>("isEnabled"))
                {
                    StatusMessage = "Bluetooth недоступен";
                    yield break;
                }
 
                AndroidJavaObject bondedDevices = bluetoothAdapter.Call<AndroidJavaObject>("getBondedDevices");
                AndroidJavaObject[] devices = bondedDevices.Call<AndroidJavaObject[]>("toArray");
 
                if (devices == null || devices.Length == 0)
                {
                    StatusMessage = "Нет сопряжённых устройств";
                    yield break;
                }
 
                string uuid = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";
                AndroidJavaObject uuidObj = new AndroidJavaClass("java.util.UUID")
                    .CallStatic<AndroidJavaObject>("fromString", uuid);
 
                foreach (AndroidJavaObject device in devices)
                {
                    try
                    {
                        string name = device.Call<string>("getName");
                        StatusMessage = "Подключение к " + name + "...";
 
                        clientSocket = device.Call<AndroidJavaObject>(
                            "createRfcommSocketToServiceRecord", uuidObj);
                        clientSocket.Call("connect");
 
                        inputStream = clientSocket.Call<AndroidJavaObject>("getInputStream");
                        outputStream = clientSocket.Call<AndroidJavaObject>("getOutputStream");
 
                        IsConnected = true;
                        StatusMessage = "Подключено к " + name;
                        onConnected?.Invoke();
                        break;
                    }
                    catch (Exception) { continue; }
                }
 
                if (!IsConnected)
                    StatusMessage = "Не удалось подключиться";
            }
        }
        catch (Exception e)
        {
            StatusMessage = "Ошибка: " + e.Message;
        }
 
        if (IsConnected)
            StartCoroutine(ReadLoop());
    }
 
    private IEnumerator ReadLoop()
    {
        byte[] buffer = new byte[1024];
        while (IsConnected)
        {
            try
            {
                if (inputStream != null)
                {
                    int bytes = inputStream.Call<int>("read", buffer);
                    if (bytes > 0)
                    {
                        string message = System.Text.Encoding.UTF8.GetString(buffer, 0, bytes);
                        ProcessMessage(message);
                    }
                }
            }
            catch (Exception)
            {
                IsConnected = false;
                StatusMessage = "Соединение потеряно";
                onDisconnected?.Invoke();
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
#endif
 
    private void ProcessMessage(string message)
    {
        if (message.StartsWith("MOVE:"))
        {
            string moveData = message.Substring(5);
            onMoveReceived?.Invoke(moveData);
        }
        else if (message.StartsWith("CHAT:"))
        {
            string chatMsg = message.Substring(5);
            chatMessages.Add("Противник: " + chatMsg);
            onChatReceived?.Invoke(chatMsg);
        }
    }
 
    public void SendMove(Move move)
    {
        string data = "MOVE:" + move.from.x + "," + move.from.y + "," +
                       move.to.x + "," + move.to.y;
       if (move.isCastling)
        {
            data += ",CASTLE," + move.castlePieceFrom.x + "," + move.castlePieceFrom.y +
                    "," + move.castlePieceTo.x + "," + move.castlePieceTo.y;
        }
        if (move.isTeleport) data += ",TELEPORT";
        if (move.isDragonRanged) data += ",RANGED";
        SendData(data);
    }
 
    public void SendChat(string message)
    {
        chatMessages.Add("Вы: " + message);
        SendData("CHAT:" + message);
    }
 
    private void SendData(string data)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (outputStream != null && IsConnected)
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(data);
                outputStream.Call("write", bytes);
                outputStream.Call("flush");
            }
        }
        catch (Exception e)
        {
            StatusMessage = "Ошибка отправки: " + e.Message;
        }
#endif
    }
 
    public void Disconnect()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            if (clientSocket != null) clientSocket.Call("close");
            if (serverSocket != null) serverSocket.Call("close");
        }
        catch (Exception) { }
#endif
        IsConnected = false;
        StatusMessage = "Отключено";
    }
 
    void OnDestroy()
    {
        Disconnect();
    }
}