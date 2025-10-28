using System;
using System.Collections;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using BC.Scripts.Utils;
using Newtonsoft.Json;

public class RealtimeAPIWrapper : MonoBehaviour
{
    private ClientWebSocket ws;
    private string apiKey = "YOUR_API_KEY"; // loaded from streamingAssets
    [TextArea (3,10)] public string promptInstruction;
    public AudioPlayer audioPlayer;
    private StringBuilder _messageBuffer = new StringBuilder();
    private StringBuilder _transcriptBuffer = new StringBuilder();
    private bool _isResponseInProgress;

    private string _currentConversationId;
    private string _currentVoice = "verse"; // default voice
    private int _conversationTurnCount = 0;
    public int maxTurnsBeforeReset = 1; // change as you like

    public static event Action OnWebSocketConnected;
    public static event Action OnWebSocketClosed;
    public static event Action OnSessionCreated;
    public static event Action OnConversationItemCreated;
    public static event Action OnResponseDone;
    public static event Action<string> OnTranscriptReceived;
    public static event Action OnResponseCreated;
    public static event Action OnResponseAudioDone;
    public static event Action OnResponseAudioTranscriptDone;
    public static event Action OnResponseContentPartDone;
    public static event Action OnResponseOutputItemDone;
    public static event Action OnRateLimitsUpdated;
    public static event Action OnResponseOutputItemAdded;
    public static event Action OnResponseContentPartAdded;
    public static event Action OnResponseCancelled;

    private void Start()
    {
        apiKey = BcUtils.LoadApiKey();

        AudioRecorder.OnAudioRecorded += SendAudioToAPI;
    } 
        
    private void OnApplicationQuit() => DisposeWebSocket();


    /// <summary>
    /// connects or disconnects websocket when button is pressed
    /// </summary>
    public async void ConnectWebSocketButton()
    {
        if (ws != null) DisposeWebSocket();
        else
        {
            ws = new ClientWebSocket();
            await ConnectWebSocket();
        }
    }

    /// <summary>
    /// establishes websocket connection to the api
    /// </summary>
    private async Task ConnectWebSocket()
    {
        try
        {
            var uri = new Uri("wss://api.openai.com/v1/realtime?model=gpt-4o-mini-realtime-preview-2024-12-17"); // lower cost variant
            ws.Options.SetRequestHeader("Authorization", "Bearer " + apiKey);
            ws.Options.SetRequestHeader("OpenAI-Beta", "realtime=v1");
            await ws.ConnectAsync(uri, CancellationToken.None);
            OnWebSocketConnected?.Invoke();
            _ = ReceiveMessages();
        }
        catch (Exception e)
        {
            Debug.LogError("websocket connection failed: " + e.Message);
        }
    }

    /// <summary>
    /// sends a cancel event to api if response is in progress
    /// </summary>
    private async void SendCancelEvent()
    {
        if (ws == null || ws.State != WebSocketState.Open)
            return;

        if (!_isResponseInProgress)
        {
            Debug.LogWarning("No active response to cancel.");
            return;
        }

        var cancelMessage = new { type = "response.cancel" };
        string jsonString = JsonConvert.SerializeObject(cancelMessage);
        byte[] messageBytes = Encoding.UTF8.GetBytes(jsonString);
        await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

        Debug.Log("### Sent response.cancel event");
        OnResponseCancelled?.Invoke();
        _isResponseInProgress = false;
    }


    /// <summary>
    /// sends recorded audio to the api
    /// </summary>
    /*private async void SendAudioToAPI(string base64AudioData)
    {
        if (isResponseInProgress)
            SendCancelEvent();

        if (ws != null && ws.State == WebSocketState.Open)
        {
            var eventMessage = new
            {
                type = "conversation.item.create",
                item = new
                {
                    type = "message",
                    role = "user",
                    content = new[]
                    {
                        new { type = "input_audio", audio = base64AudioData }
                    }
                }
            };
            /*var eventMessage = new
            {
                type = "conversation.item.create",
                item = new
                {
                    type = "message",
                    role = "user",
                    content = new[]
                    {
                        new { type = "input_text", text = "Say hello, please speak aloud!" }
                    }
                }
            };


            string jsonString = JsonConvert.SerializeObject(eventMessage);
            byte[] messageBytes = Encoding.UTF8.GetBytes(jsonString);
            await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);

            var responseMessage = new
            {
                type = "response.create",
                response = new
                {
                    modalities = new[] { "audio", "text" },
                    instructions = "Speak your reply aloud and include a transcript.",
                    voice = "verse",          // male, or alloy
                }
            };

            string responseJson = JsonConvert.SerializeObject(responseMessage);
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
            await ws.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
*/
    private async void SendAudioToAPI(string base64AudioData)
    {
        if (_isResponseInProgress)
            SendCancelEvent();

        if (ws == null || ws.State != WebSocketState.Open)
            return;

        // 1️⃣ Create user message
        var eventMessage = new
        {
            type = "conversation.item.create",
            item = new
            {
                type = "message",
                role = "user",
                content = new[]
                {
                    new { type = "input_audio", audio = base64AudioData }
                }
            }
        };
        await SendJson(eventMessage);

        // 2️⃣ Create response message
        var responseDict = new Dictionary<string, object>
        {
            { "modalities", new[] { "audio", "text" } },
            { "instructions", promptInstruction }
        };

        // ✅ Only include voice the very first time (new conversation)
        if (string.IsNullOrEmpty(_currentConversationId))
            responseDict["voice"] = _currentVoice;

        var responseMessage = new
        {
            type = "response.create",
            response = responseDict
        };
        await SendJson(responseMessage);
    }

    
    /// <summary>
    /// receives messages from websocket and handles them
    /// </summary>
    private async Task ReceiveMessages()
    {
        Debug.Log("### ReceiveMessages()");

        var buffer = new byte[1024 * 128];
        var messageHandlers = GetMessageHandlers();

        while (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            _messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

            if (ws.State == WebSocketState.CloseReceived)
            {
                Debug.Log("websocket close received, disposing current ws instance.");
                DisposeWebSocket();
                return;
            }

            if (result.EndOfMessage)
            {
                string fullMessage = _messageBuffer.ToString();
                _messageBuffer.Clear();
                Debug.Log("Raw message: " + fullMessage);

                if (!string.IsNullOrEmpty(fullMessage.Trim()))
                {
                    try
                    {
                        JObject eventMessage = JObject.Parse(fullMessage);
                        string messageType = eventMessage["type"]?.ToString();

                        if (messageHandlers.TryGetValue(messageType, out var handler)) handler(eventMessage);

                        // Track conversation_id if present
                        var convoId = eventMessage["response"]?["conversation_id"]?.ToString();
                        if (!string.IsNullOrEmpty(convoId))
                        {
                            _currentConversationId = convoId;
                            Debug.Log("### Updated currentConversationId: " + _currentConversationId);
                        }
                        
                        else Debug.Log("unhandled message type: " + messageType);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("error parsing json: " + ex.Message);
                    }
                }
            }
        }
    }

    /// <summary>
    /// returns dictionary of message handlers for different message types
    /// </summary>
    private Dictionary<string, Action<JObject>> GetMessageHandlers()
    {
        return new Dictionary<string, Action<JObject>>
        {
            { "response.audio.delta", HandleAudioDelta },
            { "response.audio_transcript.delta", HandleTranscriptDelta },
            { "conversation.item.created", _ => OnConversationItemCreated?.Invoke() },
            { "response.done", HandleResponseDone },
            { "response.created", HandleResponseCreated },
            { "session.created", _ => OnSessionCreated?.Invoke() },
            { "response.audio.done", _ => OnResponseAudioDone?.Invoke() },
            { "response.audio_transcript.done", _ => OnResponseAudioTranscriptDone?.Invoke() },
            { "response.content_part.done", _ => OnResponseContentPartDone?.Invoke() },
            { "response.output_item.done", _ => OnResponseOutputItemDone?.Invoke() },
            { "response.output_item.added", _ => OnResponseOutputItemAdded?.Invoke() },
            { "response.content_part.added", _ => OnResponseContentPartAdded?.Invoke() },
            { "rate_limits.updated", _ => OnRateLimitsUpdated?.Invoke() },
            { "error", HandleError }
        };
    }

    /// <summary>
    /// handles incoming audio delta messages from api
    /// </summary>
    private void HandleAudioDelta(JObject eventMessage)
    {
        string base64AudioData = eventMessage["delta"]?.ToString();
        if (!string.IsNullOrEmpty(base64AudioData))
        {
            byte[] pcmAudioData = Convert.FromBase64String(base64AudioData);
            Debug.Log($"### Audio delta received: {pcmAudioData.Length} bytes");
            audioPlayer.EnqueueAudioData(pcmAudioData);
        }
        else 
            Debug.Log($"### Audio delta received: string is NULL or empty");
    }

    /// <summary>
    /// handles incoming transcript delta messages from api
    /// </summary>
    private void HandleTranscriptDelta(JObject eventMessage)
    {
        string transcriptPart = eventMessage["delta"]?.ToString();
        if (!string.IsNullOrEmpty(transcriptPart))
        {
            Debug.Log("### Transcript delta: " + transcriptPart);
            _transcriptBuffer.Append(transcriptPart);
            OnTranscriptReceived?.Invoke(transcriptPart);
        }
    }

    /// <summary>
    /// handles response.done message - checks if audio is still playing
    /// </summary>
    private void HandleResponseDone(JObject eventMessage)
    {
        LogResponseData(eventMessage);

        // Continue your normal handling
        StartCoroutine(WaitForAudioFinish());
    }

    private void LogResponseData(JObject eventMessage)
    {
        // Extract and log token usage from the event
        try
        {
            var usage = eventMessage["response"]?["usage"];
            if (usage != null)
            {
                int inputTokens = usage.Value<int?>("input_tokens") ?? 0;
                int outputTokens = usage.Value<int?>("output_tokens") ?? 0;
                int totalTokens = usage.Value<int?>("total_tokens") ?? (inputTokens + outputTokens);

                Debug.Log($"### Token usage: input={inputTokens}, output={outputTokens}, total={totalTokens}");

                // Optional: if you want to see if it accumulates
                if (inputTokens > 1000)
                    Debug.LogWarning("⚠️ Context growing large — consider ResetConversation soon.");
            }
            else
            {
                Debug.Log("### No token usage info found in response.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error parsing token usage: " + ex.Message);
        }
    }

    private IEnumerator WaitForAudioFinish()
    {
        // Wait until audioPlayer queue is empty
        while (audioPlayer.IsAudioPlaying())
            yield return null;

        _isResponseInProgress = false;
        OnResponseDone?.Invoke();
        
        _conversationTurnCount++;
        Debug.Log($"### Turn {_conversationTurnCount} completed.");

        if (_conversationTurnCount >= maxTurnsBeforeReset)
        {
            ResetConversation();
            _conversationTurnCount = 0;
            Debug.Log("### Conversation auto-reset after max turns reached.");
        }

        Debug.Log("### Audio fully finished.");
    }


    /// <summary>
    /// handles response.created message - resets transcript buffer
    /// </summary>
    private void HandleResponseCreated(JObject eventMessage)
    {
        _transcriptBuffer.Clear();
        _isResponseInProgress = true;
        OnResponseCreated?.Invoke();
    }

    /// <summary>
    /// handles error messages from api
    /// </summary>
    private void HandleError(JObject eventMessage)
    {
        string errorMessage = eventMessage["error"]?["message"]?.ToString();
        if (!string.IsNullOrEmpty(errorMessage))
        {
            Debug.LogError("openai error: " + errorMessage);
        }
    }

    /// <summary>
    /// Helper to send JSON
    /// </summary>
    /// <param name="message"></param>
    private async Task SendJson(object message)
    {
        string jsonString = JsonConvert.SerializeObject(message);
        byte[] messageBytes = Encoding.UTF8.GetBytes(jsonString);
        await ws.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
    }

    public void ResetConversation(string newVoice = null)
    {
        _currentConversationId = null;

        if (!string.IsNullOrEmpty(newVoice))
        {
            _currentVoice = newVoice;
            Debug.Log("### Switched voice to: " + newVoice);
        }

        Debug.Log("### Conversation reset.");
    }

    
    /// <summary>
    /// disposes the websocket connection
    /// </summary>
    private async void DisposeWebSocket()
    {
        if (ws != null && (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived))
        {
            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by user", CancellationToken.None);
            ws.Dispose();
            ws = null;
            OnWebSocketClosed?.Invoke();
        }
    }

}
