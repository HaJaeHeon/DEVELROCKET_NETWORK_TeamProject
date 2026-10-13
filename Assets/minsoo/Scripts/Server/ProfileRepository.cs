using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class UserProfile
{
    public string Name;
    public int Rate;
    public int Games;
    public int Wins;
    public int Losses;

    public static UserProfile FromSnapshot(DataSnapshot snap)
    {
        if (!snap.Exists) return null;
        return new UserProfile
        {
            Name = snap.Child("name").Value as string,
            Rate = ToInt(snap.Child("rate").Value),
            Games = ToInt(snap.Child("stats/games").Value),
            Wins = ToInt(snap.Child("stats/wins").Value),
            Losses = ToInt(snap.Child("stats/losses").Value),
        };
    }

    // RTDB 숫자는 long
    private static int ToInt(object v) => v == null ? 0 : Convert.ToInt32(v);
}

public class ProfileRepository
{
    private FirebaseAuth auth;
    private DatabaseReference userRef;
    public DatabaseReference UserRef { get { return userRef; } }

    private DatabaseReference listeningRef;
    public event Action<UserProfile> ProfileChanged;

    public ProfileRepository()
    {
        auth = FirebaseAuth.DefaultInstance;
        userRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private string CurrentUid => auth.CurrentUser?.UserId;

    //익명 프로필 생성
    public async Task<bool> CreateGeustProfileAsync(string name)
    {
        var user = auth.CurrentUser;
        if (user == null)
            return false;
        string uid = user.UserId;

        

        // 첫 DB 목록 생성
        Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
        {
            {$"Nicknames/{name.ToLower()}", uid},
            {$"Users/{uid}/name", name },
            {$"Users/{uid}/rate", 0 },
            {$"Users/{uid}/createdAt", ServerValue.Timestamp},
            {$"Users/{uid}/stats",  new Dictionary<string, object>
                {
                    { "games", 0 },
                    { "wins", 0 },
                    { "losses", 0 }
                } },
        };

        try
        {
            await userRef.UpdateChildrenAsync(keyValuePairs);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Create Failed : {e}");
            return false;
        }

    }

    // 프로필 생성
    public async Task<bool> CreateProfileAsync(string name)
    {
        var user = auth.CurrentUser;
        if (user == null)
            return false;
        string uid = user.UserId;

        // 첫 DB 목록 생성
        Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
        {
            {$"Nicknames/{name.ToLower()}", uid},
            {$"Users/{uid}/name", name },
            {$"Users/{uid}/rate", 0 },
            {$"Users/{uid}/createdAt", ServerValue.Timestamp},
            {$"Users/{uid}/stats",  new Dictionary<string, object>
                {
                    { "games", 0 },
                    { "wins", 0 },
                    { "losses", 0 }
                } },
            {$"PrivateUser/{uid}/email", user.Email }
        };

        try
        {
            await userRef.UpdateChildrenAsync(keyValuePairs);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Create Failed : {e}");
            return false;
        }

    }

    // 닉네임 중복 검사
    public async Task<bool> NicknameExistAsync(string name)
    {
        DataSnapshot nickSnap = await userRef.Child($"Nicknames/{name.ToLower()}").GetValueAsync();
        return nickSnap.Exists;
    }

    // 프로필 정보 가져오기
    public async Task<UserProfile> LoadProfileAsync(string uid = null)
    {
        uid ??= CurrentUid;
        if (uid == null) return null;

        var snap = await userRef.Child("Users").Child(uid).GetValueAsync();
        return UserProfile.FromSnapshot(snap);
    }


    public void StartListening()
    {
        StopListening();
        string uid = auth.CurrentUser?.UserId;
        if (uid == null) return;

        listeningRef = userRef.Child("Users").Child(uid);
        listeningRef.ValueChanged += OnProfileValueChanged;
    }

    public void StopListening()
    {
        if (listeningRef == null) return;
        listeningRef.ValueChanged -= OnProfileValueChanged;
        listeningRef = null;
    }

    private void OnProfileValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }
        ProfileChanged?.Invoke(UserProfile.FromSnapshot(args.Snapshot));
    }


    // 프리셋 생성

    // 프리셋 갱신

    // 프리셋 삭제

    // 프리셋 불러오기
}
