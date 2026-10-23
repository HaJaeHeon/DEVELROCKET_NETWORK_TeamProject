# ProjectInfo

| 항목 | 내용 |
| --- | --- |
| 팀명 / 팀장 | U.D.T.T <br>팀장: 하재헌 |
| 팀원 | 하재헌, 권민수, 권혁찬 |
| 프로젝트명 / 게임명 | **프로젝트명:** `DEVELROCKET_NETWORK_TeamProject`<br>게임명:U.D.T.T Tower Company |
| 게임 한 줄 정의 | **1~4명이 타워(Tower)를 구매·설치·합성하여 공동 체력을 지키고 모든 웨이브(Wave, 적이 출현하는 차례)를 완료하는 협동 타워 방어(Tower Defense) 게임.** |
| 적용한 네트워크 기술 | **PUN 2(Photon Unity Networking 2):** 방 생성·검색·입장, 원격 함수 호출(RPC, Remote Procedure Call), 참가자·방 사용자 정의 속성(Custom Properties) 동기화, 위치 전달(PhotonTransformView), 장면(Scene) 전환 동기화<br>**Firebase Authentication:** 계정 생성·인증<br>**Firebase Realtime Database:** 사용자 정보(Profile)와 사용자 제작 지도 저장·조회 |
| GitHub 저장소 링크 | [DEVELROCKET_NETWORK_TeamProject 저장소](https://github.com/HaJaeHeon/DEVELROCKET_NETWORK_TeamProject) |
| 조작 방법 | **마우스(Mouse) 왼쪽 버튼:** 화면 버튼(Button) 선택, 상점 타워 구매<br>**끌어놓기(Drag and Drop):** 인벤토리(Inventory, 보유한 타워 항목을 담는 공간)의 항목을 건설 가능 칸에 놓아 건설 요청. 합성 가능한 내 타워 위에 놓아 합성 요청<br>**회수:** 설치된 내 타워를 선택한 뒤 인벤토리 영역에서 버튼을 놓아 회수 요청<br>**판매:** 인벤토리 항목을 판매 영역에 끌어놓아 판매 요청<br>**키보드(Keyboard) 1~5:** 해당 선택 칸에 설정한 이모티콘(Emoticon) 표시 |
| 실행 방법 | 동일한 배포용 실행 파일(Build)을 두 기기에서 각각 실행하고, 서로 다른 계정으로 접속합니다. 한쪽에서 방을 만들고 다른 쪽에서 같은 방에 입장한 뒤, 전원 준비 완료 후 방장(Master Client)이 게임을 시작합니다. 자세한 순서는 아래 실행 절차를 따릅니다. |
| 실행 환경 | **개발 도구:** Unity `6000.3.6f1`<br>**운영체제:** Windows 10/11 기준 안내, 실제 호환성 확인 필요<br>**프로젝트 기본 해상도 설정:** `1920 × 1080`<br>**통신 환경:** Photon과 Firebase에 연결할 수 있는 인터넷 연결<br>**최소·권장 장치 사양:** 별도 측정 및 확인 필요 |
