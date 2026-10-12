using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ProfileRepository2
{
    private readonly FirebaseAuth auth;
    private readonly DatabaseReference root;

    private DatabaseReference listeningRef;

    public event Action<UserProfile> ProfileChanged;

    public ProfileRepository2()
    {
        auth = FirebaseAuth.DefaultInstance;
        root = FirebaseDatabase.DefaultInstance.RootReference;
    }

    private string CurrentUid => auth.CurrentUser?.UserId;

    // ───────── 생성 ─────────
    public async Task<bool> CreateProfileAsync(string name)
    {
        var user = auth.CurrentUser;
        if (user == null) return false;
        string uid = user.UserId;
        string nickKey = ToNickKey(name);

        var updates = new Dictionary<string, object>
        {
            { $"Nicknames/{nickKey}", uid },                 // 닉네임 선점 (규칙으로 중복 차단)
            { $"Users/{uid}/name", name },
            { $"Users/{uid}/rate", 0 },
            { $"Users/{uid}/createdAt", ServerValue.Timestamp },
            { $"Users/{uid}/stats", new Dictionary<string, object>
                { { "games", 0 }, { "wins", 0 }, { "losses", 0 } } },
            { $"PrivateUser/{uid}/email", user.Email },
        };

        try
        {
            await root.UpdateChildrenAsync(updates);   // 전부 성공 or 전부 실패
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Create Failed : {e}");
            return false;   // 닉네임 중복이면 규칙에 막혀 여기로 옴
        }
    }

    // ───────── 닉네임 중복 검사 (UI 안내용) ─────────
    public async Task<bool> NicknameExistAsync(string name)
    {
        var snap = await root.Child("Nicknames").Child(ToNickKey(name)).GetValueAsync();
        return snap.Exists;
    }

    // ───────── 불러오기 ─────────
    public async Task<UserProfile> LoadProfileAsync(string uid = null)
    {
        uid ??= CurrentUid;
        if (uid == null) return null;

        var snap = await root.Child("Users").Child(uid).GetValueAsync();
        return UserProfile.FromSnapshot(snap);
    }

    // ───────── 갱신 1: 닉네임 변경 ─────────
    public async Task<bool> ChangeNameAsync(string oldName, string newName)
    {
        string uid = CurrentUid;
        if (uid == null) return false;

        var updates = new Dictionary<string, object>
        {
            { $"Nicknames/{ToNickKey(oldName)}", null },     // null = 삭제
            { $"Nicknames/{ToNickKey(newName)}", uid },
            { $"Users/{uid}/name", newName },
        };

        try
        {
            await root.UpdateChildrenAsync(updates);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"ChangeName Failed : {e}");
            return false;
        }
    }

    // ───────── 갱신 2: 게임 결과 반영 ─────────
    public async Task RecordGameResultAsync(bool isWin, int rateDelta)
    {
        string uid = CurrentUid;
        if (uid == null) return;

        // 여러 기기/요청이 동시에 바꿔도 값이 꼬이지 않도록 트랜잭션 사용
        await root.Child("Users").Child(uid).RunTransaction(data =>
        {
            if (data.Value == null) return TransactionResult.Success(data);

            var user = data.Value as Dictionary<string, object>;
            var stats = user["stats"] as Dictionary<string, object>;

            stats["games"] = Convert.ToInt64(stats["games"]) + 1;
            if (isWin) stats["wins"]   = Convert.ToInt64(stats["wins"]) + 1;
            else       stats["losses"] = Convert.ToInt64(stats["losses"]) + 1;
            user["rate"] = Convert.ToInt64(user["rate"]) + rateDelta;

            data.Value = user;
            return TransactionResult.Success(data);
        });
    }

    // ───────── 갱신 받기: 내 프로필 실시간 감지 ─────────
    public void StartListening()
    {
        StopListening();
        string uid = CurrentUid;
        if (uid == null) return;

        listeningRef = root.Child("Users").Child(uid);
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

    // 대소문자 구분 없이 중복 검사 ("Abc"와 "abc"를 같은 닉네임으로)
    private static string ToNickKey(string name) => name.Trim().ToLowerInvariant();
}