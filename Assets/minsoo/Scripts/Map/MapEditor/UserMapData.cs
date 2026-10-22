using Firebase.Auth;
using Firebase.Database;
using System;
using System.Collections.Generic;

[Serializable]
public class UserMapData
{

    public string Id;          // Firebase Push().Key
    public string Name;        // 맵 이름
    public string AuthorUid;   // 만든 사람 uid (보안 규칙에서 검사)
    public string AuthorName;  // 만든 사람 닉네임 (목록 표시용)
    public int Width;
    public int Height;
    public string Cells;       // 칸당 문자 1개, 길이 = Width * Height
    public bool Published;
    public long CreatedAt;     // ms, 서버 시간
    public long UpdatedAt;

    // 에디터에서 새 맵 만들기
    public static UserMapData Create(string name, TileType[,] grid)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        return new UserMapData
        {
            Name = name,
            AuthorUid = user?.UserId,
            AuthorName = Photon.Pun.PhotonNetwork.NickName,
            Width = grid.GetLength(0),
            Height = grid.GetLength(1),
            Cells = MapCodec.Encode(grid),
        };
    }

    public TileType[,] ToGrid() => MapCodec.Decode(Cells, Width, Height);

    // RTDB 저장용 (UpdateChildrenAsync에 넣음)
    public Dictionary<string, object> ToDictionary(bool isNew)
    {
        var d = new Dictionary<string, object>
        {
            { "name", Name },
            { "authorUid", AuthorUid },
            { "authorName", AuthorName },
            { "width", Width },
            { "height", Height },
            { "cells", Cells },
            { "published", Published },
            { "updatedAt", ServerValue.Timestamp },
        };
        if (isNew) d["createdAt"] = ServerValue.Timestamp;
        return d;
    }

    // 목록(MapList)용 요약: cells를 빼서 가볍게
    public Dictionary<string, object> ToSummary() => new()
    {
        { "name", Name },
        { "authorUid", AuthorUid },
        { "authorName", AuthorName },
        { "width", Width },
        { "height", Height },
        { "updatedAt", ServerValue.Timestamp },
    };

    public static UserMapData FromSnapshot(DataSnapshot snap)
    {
        if (!snap.Exists) return null;
        return new UserMapData
        {
            Id = snap.Key,
            Name = snap.Child("name").Value as string,
            AuthorUid = snap.Child("authorUid").Value as string,
            AuthorName = snap.Child("authorName").Value as string,
            Width = ToInt(snap.Child("width").Value),
            Height = ToInt(snap.Child("height").Value),
            Cells = snap.Child("cells").Value as string,
            Published = snap.Child("published").Value is bool b && b,
            CreatedAt = ToLong(snap.Child("createdAt").Value),
            UpdatedAt = ToLong(snap.Child("updatedAt").Value),
        };
    }

    // RTDB 숫자는 long으로 옴
    private static int ToInt(object v) => v == null ? 0 : Convert.ToInt32(v);
    private static long ToLong(object v) => v == null ? 0 : Convert.ToInt64(v);
}