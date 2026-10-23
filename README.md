# Network_TeamProject

## 1. 개요

| 게임명 | U.D.T.T Tower Company |
| --- | --- |
| 장르 | 멀티 플레이, 타워 디펜스 |
| 한 줄 정의 | 다인용 타워 디펜스(Multiplayer Tower Defence, 여러 사람이 함께 타워로 적을 막는 게임) |
| 동시 접속 인원 | 1 ~ 4 명 |
| 실행 환경 | 1920x1080 해상도 |
| GitHub 저장소 링크 | https://github.com/HaJaeHeon/DEVELROCKET_NETWORK_TeamProject.git |

## 2. 실행 방법

| 빌드 2개 실행 절차 |   1. 각 클라이언트가 서로 다른 Firebase 계정으로 로그인<br>  2. Firebase 에서 UID 획득<br>  3. Firebase DB에서 프로필 불러오기<br>  4. Firebase UID를 Photon UserId로 설정<br>  5. Photon 서버 접속<br>  6. Photon 로비 입장<br>  7. 방 생성 또는 입장 |
| --- | --- |
| Unity 버전 | 6000.3.6f1 |
| 조작 키 | 마우스 조작<br>  • 버튼 클릭 >> 프리셋 해제, 타워 구매 /<br>  • 드래그 >> 프리셋 설정, 타워 건설, 타워 회수)<br>  •  맵 제작  <br>키보드 조작(카메라 이동, 이모티콘(1~5)) |
| 설정 파일 위치 | Firebase - <br>`Assets/google-services.json` <br>Photon - `Assets/Imports/Photon/PhotonUnityNetworking/Resources/PhotonServerSettings.asset` |

## 3. 핵심 플레이 흐름 (로그인 → 로비 → 게임 → 결과 → 재대전)

<img width="914" height="1091" alt="ProjectFlow drawio" src="https://github.com/user-attachments/assets/6f3e7de7-ae2b-43eb-b783-707a950c7beb" />


1. 타이틀 씬에서 회원 가입 및 설정 후,  로그인 하면 로비로 입장
2. 현재 방 목록 중에서 원하는 방 입장하거나, 랜덤 룸 입장, 방 만들기를 통해 방에 입장, 이모티콘 프리셋 제작
3. 맵 에디터 씬으로 넘어가서 맵 제작 후 검증, 저장, 게시 후 다시 로비로 복귀
4. 방에서 모든 플레이어가 준비가 완료되면 ‘방장’이 게임 시작 클릭 시 게임을 시작함 >> 게임 Scene으로 이동
5. 게임 Scene에서 게임 진행
6. 결과 ( Clear / Lose ) 에 따라 DB에 게임 결과 내용이 저장됨 ( Clear 시에만 저장되며 nickname, stats{games : , wins, losses}, 반영 시점)
7. 결과 확인 후 로비로 돌아옴
8. 다시 방을 만들어 게임을 플레이

## 4. 주요 기능 목록

- **회원가입 및 로그인**
    - Firebase Authentication을 활용한 계정 생성, 이메일 인증, 로그인 기능
- **멀티플레이 로비 및 방 관리**
    - Photon 기반 방 생성, 검색, 비밀번호 방, 랜덤 입장 및 싱글 플레이 지원
- **게임 준비 및 시작**
    - 플레이어 준비 상태와 맵 선택을 동기화하고, 전원 준비 시 방장이 게임 시작
- **네트워크 타워 시스템**
    - 타워 구매·설치·회수·판매 요청을 방장이 검증하고 모든 플레이어에게 동기화
- **협동 타워 디펜스**
    - 적 생성과 웨이브 진행, 공동 체력, 플레이어별 재화를 네트워크로 공유
- **게임 종료 처리**
    - 모든 웨이브 제거 시 클리어, 공동 체력이 0이 되면 패배 결과 표시
- **사용자 맵 제작 (Map Editor)**
    - 타일 기반 맵 제작, 경로 유효성 검사, Firebase 저장 및 게시 기능

## 5. 프로젝트 구조 (폴더 · Scene · 주요 스크립트)

```
DEVELROCKET_NETWORK_TeamProject/
├── Assets/
│   ├── Scenes/                         # 공통 실행 장면
│   │   ├── StartScene.unity
│   │   └── GameScene.unity
│   │
│   ├── HaJaeHeon/                      # 사용자 조작·화면·건설물 거래·소리
│   │   ├── Scripts/
│   │   │   ├── PlayerUI/               # 화면 전환, 참여자 표현, 사전 구성
│   │   │   └── Tower/                  # 상점, 보관함, 설치·회수·판매
│   │   ├── Prefabs/                    # 재사용할 객체 원본(Prefab)
│   │   └── Test/                       # 기능 확인용 객체 원본
│   │
│   ├── abcdcode/                       # 전투 객체·진행 상태·공통 통신
│   │   ├── Scripts/
│   │   │   ├── BulletPlace/            # 탄환 이동과 적중 처리
│   │   │   ├── DB/                     # 건설물·적 등록 정보와 생성
│   │   │   ├── SOData/                 # 전투·단계별 설정 자료의 형식
│   │   │   ├── TowerAI/                # 공격 대상 탐색과 발사 판단
│   │   │   └── UI/                     # 진행 정보와 결과 화면
│   │   ├── SOData/                     # 설정 자료 객체
│   │   ├── EnemyPrefab/                # 적 객체 원본
│   │   ├── TowerPrefab/                # 건설물·탄환 객체 원본
│   │   ├── UIPrefab/                   # 화면 객체 원본
│   │   └── Scene/                     # 전투 기능 확인용 장면
│   │
│   ├── minsoo/                         # 계정·접속·방 관리·지도
│   │   ├── Scripts/
│   │   │   ├── Server/
│   │   │   │   └── UI/                # 접속·대기방·방 관련 화면
│   │   │   │       └── Panel/         # 화면별 입력과 표시 처리
│   │   │   └── Map/
│   │   │       └── MapEditor/         # 지도 제작·검사·저장·공개
│   │   ├── Scenes/
│   │   │   ├── EditorScene.unity      # 지도 제작 장면
│   │   │   └── Test*.unity            # 접속·방·지도 등의 확인용 장면
│   │   ├── Prefabs/
│   │   │   ├── Manager/               # 계정·통신·지도 관리자 원본
│   │   │   ├── Maps/                  # 지도 원본과 지도 등록 자료
│   │   │   └── Test/                  # 기능 확인용 객체 원본
│   │   └── Tilemap/                   # 격자 지도(Tilemap) 관련 자료
│   │
│   ├── Interface/                     # 여러 기능이 공유하는 규약
│   │   └── Example/                   # 공통 규약의 사용 예시
│   ├── Settings/                      # 화면 출력 등 실행 환경 설정
│   └── Imports/                       # 외부 자료 영역, 내부 내용 설명 제외
│
└── README.md                          # 실행 방법과 구현 설명
```

## 6. 동기화 설계표 최종본 (기능 / 동기화 대상 / 방식 / 판정 권한 / 실패 시)

| 기능 | 동기화 대상 | 방식 | 판정 권한 | 실패 시 |
| --- | --- | --- | --- | --- |
| 플레이어 준비 상태 | 각 플레이어 준비 상태 여부(Ready) | Player Custom Properties // 상태 변경 → 변경 통지 후 화면 갱신 | 개인 | 기존 상태 유지 / x |
| 지도 선택 | 지도 id, 제작 지도의 이름,크기, 셀 내용 | MasterClient가 Room Custom Properties 변경, 각자 변경된 지도 표시 | 개인 | 지도 조회 및 검사 실패 시 이전 선택으로 복귀 |
| 게임 시작 / 입장 차단 | 게임에 사용될 지도, Room 입장 가능 여부, 이동할 Scene | Room 내 플레이어들의 준비 상태 확인 후 사작시 LoadLevel과 AutomaticallySyncScene 사용 | MasterClient | 준비 미완료 및 지도 미준비면 시작 실패, 무작위 지도 속성 전송 실패 시 Room isOpen = true |
| 플레이어 색상 | PlayerColor | Player CustomProperties로 변경 | 개인 | 방 입장시 게시 / x |
| 플레이어 마우스 위치 | 각자의 화면 내부 좌표 | Photon Transform View | 해당 객체 제어자 | 플레이어 마우스 위치 변경 / x |
| 적 생성 | 종류, 생성 위치, 이동 경로 번호 | MasterClient가Photonnetwork.Instantiate 호출, 생성 정보로 각자 초기화 | MasterClient | SO data나 Prefab이 없으면 생성 생략 |
| 적 위치, 방향 | 적의 위치와 외형 회전 | Master만 이동 계산, Photon View Transform | MasterClient | 별도 좌표 재요청 |
| Tower 공격 대상 및 조준 | 대상 적 번호 | MasterClient가 대상을 선택하고 Rpc AllviaServer 전달, 각자 대상 위치로 조준 | 대상 선택은 MasterClient / 조준 표시는 각자 | 대상이 없거나 사거리 밖이면 조준 실패 |
| 탄환 적중 및 체력 감소 | bullet 번호, 적 번호, 피해량 | MasterClient가 충돌 판정 후 적중 Rpc를 전송, 이어서 피해량 Rpc를 보내 각자 체력 차감 | 적중 판정은 MasterClient, 체력 적용은 각자 | 탄환, 적 못찾으면 무시 |
| 발사 및 탄환 이동 | 발사 시점, 탄환의 초기 상태 | MasterClient가 발사  Rpc 전송, 각자 탄환 초기화, 이동 계산 | 발사 시점은 MasterClient / 이동 표시는 각자 | x |
| 적 처치, 제거 | 제거할 적 번호와 Photon View Object | MasterClient 가 Rpc 전송, MasterClient가 Object 제거, 각자 목록 정리 | MasterClient | 적이 이미 없다면 무시 |
| 타워 구매 | 구매 요청 번호, tower종류, 개인 재화 | MasterClient 에게 Rpc, 검증 후 재화, 구매 요청 번호 확인 >> Player Custom Properties 적용, 확인 후 결과 회신 | MasterClient | 중복 완료 요청은 기존 겨로가 재전달, properties 전송 실패시 보유 등록 취소 |
| 타워 건설 | 보유 항목 번호, 설치 칸, 타워 종류, 타워 Level | MasterClient 에게 Rpc, 요청 예약, 위치 검사, Photon View Object 생성 후 Inventory 에서 제거 | MasterClient | 거절 시 inventory 유지, 예약 해제, 생성 후 처리 실패시 object 삭제 |
| 타워 합성 | 보유 항목 번호, 설치 칸, 타워 종류, 타워 Level | MasterClient 에게 Rpc, 요청 예약, 위치 검사, Photon View Object 생성 후 Inventory 에서 제거 | MasterClient | 거절 시 inventory 유지, 예약 해제, 생성 후 처리 실패시 object 삭제 |
| 타워 철거 | 타워 Object, PhotonView Ownership | RPC // 타워 철거 요청 → 해당 플레이어 소유 검사 → 타워 철거 | MasterClient | 재전송 요청, 철거 된 상태면 오브젝트 반환, 철거 안된 상태면 현 상태 유지 |
| 타워 판매 | 보유 항목 번호, 판매 요청 번호, 개인 재화 | MasterClient 가 예약, 재화와 판매 확인 번호 기록, 변경 확인 후 결과 회신  | MasterClient | 전송 실패 시 소비한 항목 복원 시도, 복원까지 실패하면 보유 기록 관리 중단 |
| 상점 새로고침 | 웨이브 종료시 | Master가 Rpc All 전송 / 상점 내용은 각자가 무작위로 출현 | 갱신 시점은 MasterClient, 상품 내용은 개인 | 후보가 없으면 기존 상점 유지 |
| Life 감소 | 공동 Life (PlayerHP) | Room Custom Properties // 적이 목적지 도달 시 감소 | MasterClient | MasterClient 별도 내부값으로 처리 |
| 적 처치 보상 및 개인 재화 | 개인별 Gold | Room Custom Properties // 처치된 순간 각자가 본인 properties에 기록 | 보상 발생은 MasterClient / 전투 결과, 재화 사용은 각자 | x |
| 웨이브 진행 | 현재 Wave 진행 단계, 다음 웨이브까지 준비 시간 | MasterClient가 남은 적 없을 시 다음 웨이브까지 남은 시간 표기 후 다음 웨이브 진행 | MasterClient | 조회 실패 시 기본값 사용 |
| 이모티콘 출력 | Sprite 번호 | Rpc All로 전달, 각자 일정 시간 표시 | 개인 요청, 수신 측에서 오브젝트 소유자 확인 | 소유자 불일치, 그림 없음, 기존 표현 재생중이면 무시 |
| 이모티콘 프리셋 | 사전에 구성된 5칸의 그림 번호 | 본인이 번호 배열을 Custom Properties에 저장 | 개인 | 입력 검사 실패 시 저장 중단, 전송 실패시 경고 출력 |
| 게임 종료(승리, 패배) | 게임 결과 출력 | Master가 Rpc AllViaServer로 전달, 각자 종료 상태와 결과화면 반영 | MasterClient | 게임 종료 수신 확인, 일정 시간 후 방으로 귀환 |
| Client 이탈 | 해당 참여자의 Tower와 보유 기록 | MasterClient가 Tower 제거 요청 전파, Client의 보유 기록 삭제 | MasterClient | 없는 Object는 무시 |
| MasterClient 이탈 | 거래 대기 상ㅇ태, 보유 기록 관리 샅애, 방 참가 상태 | MasterClient 이탈 감지 시 Room 내의 모두 Room나가기 | 각자의 상태 처리 | MasterClient 승계 없음 |

## 7. 핵심 로직 설명 (코드 일부 인용 + 설명)

1. Firebase 인증 후 Photon 네트워크 연결
    
    인용 위치: [NetworkManager.cs (line 112)]
    
    ```
    var profile = await profileRepo.LoadProfileAsync();
    var auth = new AuthenticationValues(
        AuthManager.Instance.Auth.CurrentUser.UserId);
    
    PhotonNetwork.AuthValues = auth;
    PhotonNetwork.NickName = profile.Name;
    
    PhotonNetwork.ConnectUsingSettings();
    ```
    
    Firebase 로그인에 성공하면 사용자의 프로필을 불러오고, Firebase UID를 Photon 인증값으로 사용한다. 이후 닉네임과 준비 상태 등의 플레이어 정보를 설정하고 Photon 서버에 접속한다.
    
    Firebase는 계정과 영구 데이터를 담당하고, Photon은 로비·방·게임 동기화를 담당하도록 두 시스템의 역할을 분리했다.
    
2. 공동 체력과 게임오버 동기화
    
    인용 위치: [PlayerHP.cs (line 37)]
    
    ```
    if (!PhotonNetwork.IsMasterClient || s_hp <= 0)
        return;
    
    s_hp = System.Math.Max(0, s_hp - amount);
    Sync();
    
    if (s_hp <= 0)
        IPacketer.SendPacket(
            GameManager.Instance.MainGamePacketer.photonView, d);
    ```
    
    공동 체력은 Master Client만 변경할 수 있으며, 변경 결과는 Room Custom Properties의 `"PlayerHP"` 값으로 공유한다.
    
    체력이 0이 되면 게임오버 패킷을 모든 플레이어에게 전달한다. 각 플레이어는 동일한 패킷을 받아 게임 진행을 중단하고 패배 결과 화면을 표시한다.
    
    마지막 웨이브의 적을 모두 제거한 경우에도 같은 방식으로 클리어 패킷을 전달한다.
    
3. 방장 권한 기반 타워 처리
    인용 위치 :[SocketManager.cs (line 98)]

```
photonView.RPC(
    nameof(RpcRequestTowerPurchase),
    RpcTarget.MasterClient,
    requestId,
    towerId);
```

타워 구매·설치·판매는 클라이언트가 직접 처리하지 않고 Master Client에게 요청한다. 방장은 재화, 타워 데이터, 설치 위치와 소유권을 확인한 후 결과를 확정한다.

각 요청에 고유한 `requestId`를 사용하여 네트워크 지연으로 동일한 요청이 반복되더라도 중복 결제나 중복 생성이 발생하지 않도록 처리했다.

## 8. 실패 상황 처리 목록 (접속 끊김 · 방장 이탈 · 늦은 입장)

- 접속 끊김 :
    - 재접속 시도 , n회 시도 후에도 접속 안되면 연결 끊김 알림 / 방에 재입장 불가 / 가지고 있던 건물은 모두 철거하고 남은 재화는 현재 방에 남아 있는 모든 플레이어에게 n등분 해서 나눠줌
- 방장 이탈 :
    - 가장 우선순위 높은(핑이 제일 좋다거나 들어온 순서가 가장 빠르다거나) 클라이언트에게 Host와 방장을 넘김 / 다른 클라이언트가 존재하지 않을 시 방 해체
- 클라이언트(방장이 아닌 다른 모든 플레이어) 이탈 :
    - 재접속 시도, n회 시도 후에도 접속 안되면 연결 끊김 알림 / 재입장 불가 / 가지고 있던 건물은 모두 철거하고 남은 재화는 현재 방에 남아 있는 모든 플레이어에게 n등분 해서 나눠줌
- 늦은 입장 :
    - 늦은 입장 불가

## 9. Firebase 데이터 구조와 저장 시점

```jsx
─── MapList          key ⇒ authorName(string), authorUid(string), height(int), name(string), updatedAt(int), width(int)
├── Nicknames    key ⇒ nickname[authorUid(string)]
├── PrivateUser   key ⇒ authorUid - {email(string)}
├──UserMaps      key ⇒ authorName(string), authorUid(string), cells(string), createdAt(int), height(int), name(string), pulbished(bool), updateAt(int), width(int)
└──Users              key ⇒ authorUid -{createAt(int), name(string), rate(int), stats - {games(int), losses(int), wins(int)}}
```

- 저장 시점 (인게임) :
    - 프리셋 설정 시 : 프리셋 설정 후 완료 버튼 클릭 시 DB에 저장
    - 게임 클리어 시 : authorUid - authorUid -{createAt(int), name(string), rate(int), stats - {games(int), losses(int), wins(int)}}  을 DB에 저장
- 저장 시점 (맵 에디터) :
    - Publish를 누르면 맵 내용을 stirng 으로 DB에 저장

## 10. 팀원별 책임 영역과 기여 내용

### 하재헌

- UI
    - Title [ 로그인, 회원가입, 설정, 나가기 ]
    - Lobby [ 로그아웃(Title로), 타워 프리셋 설정, Room List, Player List, 방 검색, 선택한 방 입장, 랜덤 방 입장, 방 만들기 ]
- 게임 시작 전 세팅 파트
    - 이모티콘 프리셋, 플레이어 색깔 선택
- 플레이어 상호 작용
    - 타워 구매, 설치, 회수, 판매
    - 웨이브 종료마다 상점 Reroll
    - Player 마우스 오브젝트, 프리셋 이모티콘 출력
    - 카메라 이동(맵 외부로 이동 제한)

### 권민수

- 타이틀
    - Photon으로 로그인(로비 입장), 회원 가입
    - Firebase DB로 회원 정보 등록, 회원 정보 일치하는지 확인
- 로비
    - Photon으로 로그아웃(Title로 나가기), 방 검색, 방 입장, 방 만들기
- 계정 (로그인, 회원가입, 프로필 정보- FIrebase DB)
- 맵 ( 전체적인 맵 생성, 타워 건설 가능 구역 설정, 적 이동 경로,  적 Spawn위치 설정, 적 목적지 설정)
- 맵 에디터 - 맵 그리기, 그린 맵을 DB의 저장 및 불러오기

### 권혁찬

- 타워
    - 행동 패턴(적 인식, 공격, 기타 행동)
    - 코스트, 사거리, 데미지 등 데이터 작성
    - 데이터 및 오브젝트 프리팹 컨테이너 클래스 작성
    - 타워 및 탄환 오브젝트들 네트워크 동기화
    
- 적
    - 행동 패턴(길찾기)
    - 체력, 이동속도, 제공 보상(골드) 등 데이터 작성
    - 데이터 및 오브젝트 프리팹 컨테이너 클래스 작성
    - 적 위치 및 행동 양식 네트워크 동기화
    
- 스테이지 관리자 (웨이브 정보, 게임 루프)
