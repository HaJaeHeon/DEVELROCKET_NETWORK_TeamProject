using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class UserMapRepository
{
    private readonly DatabaseReference root = FirebaseDatabase.DefaultInstance.RootReference;
    private string Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    // 처음 저장이면 새 id를 만들고, 이미 id가 있으면 그 자리에 덮어씀
    public async Task SaveAsync(UserMapData data)
    {
        if (Uid == null) throw new InvalidOperationException("Not signed in");

        bool isNew = string.IsNullOrEmpty(data.Id);
        if (isNew) data.Id = root.Child("UserMaps").Push().Key;

        // 필드 단위 경로로 넣어야 기존 createdAt 같은 값이 지워지지 않음
        var updates = new Dictionary<string, object>();
        foreach (var kv in data.ToDictionary(isNew))
            updates[$"UserMaps/{data.Id}/{kv.Key}"] = kv.Value;
        updates[$"Users/{Uid}/maps/{data.Id}"] = true;

        await root.UpdateChildrenAsync(updates);   // 두 경로를 한 번에 저장
    }

    // 맵 하나 불러오기 (없으면 null)
    public async Task<UserMapData> LoadAsync(string id)
    {
        var snap = await root.Child("UserMaps").Child(id).GetValueAsync();
        return UserMapData.FromSnapshot(snap);
    }

    // 내가 만든 맵 목록 (최근에 저장한 순서)
    public async Task<List<UserMapData>> ListMineAsync()
    {
        if (Uid == null) throw new InvalidOperationException("Not signed in");

        var ids = await root.Child("Users").Child(Uid).Child("maps").GetValueAsync();
        var maps = new List<UserMapData>();
        foreach (var child in ids.Children)          // child.Key = mapId
        {
            var map = await LoadAsync(child.Key);
            if (map != null) maps.Add(map);
        }
        maps.Sort((a, b) => b.UpdatedAt.CompareTo(a.UpdatedAt));
        return maps;
    }

    // 맵을 공개 목록에 올림 (저장된 맵이어야 함)
    public async Task PublishAsync(UserMapData data)
    {
        if (Uid == null) throw new InvalidOperationException("Not signed in");
        if (string.IsNullOrEmpty(data.Id)) throw new InvalidOperationException("Save first");

        var updates = new Dictionary<string, object>
    {
        { $"UserMaps/{data.Id}/published", true },
        { $"MapList/{data.Id}", data.ToSummary() },
    };
        await root.UpdateChildrenAsync(updates);   // 두 곳을 한 번에 → 반쯤 공개된 상태가 안 생김
    }
}