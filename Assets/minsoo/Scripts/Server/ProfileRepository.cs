using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

public class ProfileRepository
{
    private FirebaseAuth auth;
    private DatabaseReference nickRef;
    private DatabaseReference userRef;

    public ProfileRepository()
    {
        auth = FirebaseAuth.DefaultInstance;
        nickRef = FirebaseDatabase.DefaultInstance.RootReference.Child("Nicknames");
        userRef = FirebaseDatabase.DefaultInstance.RootReference.Child("Users");
    }

    // 프로필 생성
    public async Task CreateProfileAsync(string name)
    {
        var user = auth.CurrentUser;
        string uid = user.UserId;

        if (string.IsNullOrWhiteSpace(uid))
            return;

        try
        {
            // 첫 DB 목록 생성
            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
            {
            {$"{uid}/name", name },
            {$"{uid}/rate", 0 },
            {$"{uid}/createdAt", ServerValue.Timestamp},
            {$"{uid}/stats",  new Dictionary<string, object>
                {
                    { "games", 0 },
                    { "wins", 0 },
                    { "losses", 0 }
                } },
            {$"PrivateUser/{uid}/email", user.Email }
            };

            await userRef.UpdateChildrenAsync(keyValuePairs);
        }
        catch (Exception e)
        {
            Debug.LogError($"Create Failed : {e}");
        }

    }

    // 닉네임 중복 검사
    public async Task<bool> NicknameExistAsync(string name)
    {
        DataSnapshot nickSnap = await nickRef.Child(name).GetValueAsync();
        bool isExist = nickSnap.Exists;

        return isExist;
    }


    // 프로필 갱신
    public void ProfileUpdate()
    {
        
    }


    // 프리셋 생성

    // 프리셋 갱신

    // 프리셋 삭제

    // 프리셋 불러오기
}
