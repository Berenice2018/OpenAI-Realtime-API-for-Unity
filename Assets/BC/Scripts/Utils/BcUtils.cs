using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BC.Scripts.Utils
{
    public class BcUtils : MonoBehaviour
    {
        
        public static async Task<string> EncodeImageToBase64Async(string fileName)
        {
            BCLogger.Log("EncodeImageToBase64Async ");
            byte[] imageBytes;
            string fullPathToImg = GetFullStreamingAssetsPath(fileName);
            if (fullPathToImg.StartsWith("file://"))
            {
                // Load the file using UnityWebRequest for platforms requiring file:// URI
                using (var request = UnityWebRequest.Get(fullPathToImg))
                {
                    await request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError("Failed to load image from StreamingAssets: " + request.error);
                        throw new IOException("Failed to load image: " + request.error);
                    }

                    imageBytes = request.downloadHandler.data;
                }
            }
            else
            {
                // For standalone platforms, load the file directly
                imageBytes = await File.ReadAllBytesAsync(fullPathToImg);
            }

            // Convert to Base64 string
            return Convert.ToBase64String(imageBytes);
        }
        
        
        
        // Construct the full path to the file in StreamingAssets
        public static string GetFullStreamingAssetsPath(string fileName)
        {
            string filePath = String.Empty;

#if UNITY_EDITOR || UNITY_STANDALONE
            filePath = Path.Combine(UnityEngine.Application.streamingAssetsPath, fileName);
#elif UNITY_ANDROID || UNITY_IOS
        filePath = Path.Combine("file://" + UnityEngine.Application.streamingAssetsPath, fileName);
#else
    throw new PlatformNotSupportedException("Platform not supported for StreamingAssets path.");
#endif
            return filePath;
        }
    }
}