using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameResultService
{
    private FirebaseAuth auth;
    private DatabaseReference dataRef;

    public GameResultService()
    {
        auth = FirebaseAuth.DefaultInstance;
        dataRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public string CurrentUserId => auth.CurrentUser?.UserId ?? string.Empty;

    public async Task<bool> SaveResultAsync(string roomName, int score, int playSeconds)
    {
        if (string.IsNullOrEmpty(CurrentUserId))
        {
            Debug.Log("User ID is Empty");
            return false;
        }

        try
        {
            var saveData = new Dictionary<string, object>
            {
                {$"scores/{CurrentUserId}/score", score},
                {$"scores/{CurrentUserId}/lastRoomaName", roomName},
                {$"scores/{CurrentUserId}/playSeconds", playSeconds},
                {$"scores/{CurrentUserId}/updateAt", DateTimeOffset.UtcNow.ToUnixTimeSeconds()}
            };
            await dataRef.UpdateChildrenAsync(saveData);
            Debug.Log("Save Success");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("Save Failed :" + ex.Message);
            return false;
        }

        
    }

    public async Task<bool> SaveWithRetryAsync(string roomName, int score, int playSeconds, int maxRetries = 3, float firstDelaySeconds = 2f)
    {
        float delay = firstDelaySeconds;
        for(int i = 0; i <= maxRetries; i++)
        {
            bool ok = await SaveResultAsync(roomName, score, playSeconds);
            if (ok)
            {
                Debug.Log($"Save Success Count.{i + 1}");
                return true;
            }
            if (i == maxRetries) break;

            Debug.LogWarning($"{delay}sec Retry {i + 1}/{maxRetries}");

            await Task.Delay(TimeSpan.FromSeconds(delay));
            delay *= 2f;

        }

        Debug.LogError("Save Failed");
        return false;
    }


}
