# 제로시버트 한글패치
[언더테일 모드툴](https://github.com/krzys-h/UndertaleModTool) 기반으로 작성된 한글패쳐.  
이 방식으로 게임메이커로 만들어진 모든 게임의 번역을 시트로 관리할 수 있습니다.  
30분마다 번역파일 갱신후 릴리즈 됩니다.
<p align="center">
 <img src=https://github.com/shlifedev/zero-sievert-localization/actions/workflows/autodeploy.yml/badge.svg/> 
</p> 
<p align="center">
 <img width="600px" src=https://user-images.githubusercontent.com/49047211/211140923-dc7e57d7-4462-451f-b88d-28440b2fe79f.png> 
</p> 

# 적용법 
게임 설치폴더에 `압축 해제`후 `Patcher.exe` 실행 

# FAQ
## Q.시트에 있는 최신 번역파일을 수동으로 받고싶어요.
1. [이 URL](https://script.google.com/macros/s/AKfycbzCz_GcTuC8opbyIAXIwcufljtYRVZx2lHTfCNAIx0AtuZoNwNLnIw5hv9Ov56o8WjFsw/exec)로 접속하세요(현재 모든 시트내용을 json 형식으로 출력)
2. 모든 텍스트를 복사하세요.
3. 설치 폴더 내 `localization/data.json` 파일을 텍스트 편집기로 열고 복사한 텍스트를 모두 붙여넣으세요.

## Q.번역에 참여하고 싶어요 
- [시트](https://docs.google.com/spreadsheets/d/1w2JjxpPBwynLhu69edHGjCwjLX0muLK1cXzwzj8Sfrc/edit#gid=360281631)에 셀 캡션으로 달아주시면
일정 주기마다 취합하겠습니다. 

## Q.폰트를 변경하고 싶어요
[게임메이커 폰트 만들기](https://www.youtube.com/watch?v=QIfgwtgSl4s&ab_channel=1upIndie)
참고해서 폰트를 생성하고 `localization/font` 경로에 넣기만 하면 알아서 불러옵니다.  


ZeroSievert 에는 2023.1.09 기준 아래와 같은 폰트들이 있습니다. 파일명을 같게 만들면 됩니다.
예쁜 폰트를 적용하는데 성공하신경우 저에게 공유해주시면 릴리즈에 포함하겠습니다!
| 게임폰트 파일명  | font size(em) |
| ------------- | ------------- |
| font0  | 8  |
| font_munro_12px  | 12 |
| font_quest  | 8 |
| font_minuscolo_16px  | 8  |
| font_name_speaker  | 16  |
| font_main_menu  | 12  |
| font_death_screen | 24  |
| font_munro_24  | 24 |
| font_credits_big  | 16  |
| font_credits_small  | 8  |
  

## 주의사항
 **꼭! 세이브 백업을 잘 해두세용. 사용에따른 문제는 책임지지 않습니다. **

 
