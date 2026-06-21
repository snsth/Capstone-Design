from pptx import Presentation
from pptx.util import Inches, Pt, Emu
from pptx.dml.color import RGBColor
from pptx.enum.text import PP_ALIGN
from pptx.util import Inches, Pt
import copy

# ── 색상 팔레트 ──────────────────────────────────────────────────────
BG_DARK   = RGBColor(0x1A, 0x1A, 0x2E)   # 짙은 남색 (배경)
BG_CARD   = RGBColor(0x16, 0x21, 0x3E)   # 카드 배경
ACCENT    = RGBColor(0xE9, 0x4F, 0x37)   # 강조 빨강 (공포 느낌)
ACCENT2   = RGBColor(0x39, 0x3E, 0x46)   # 중간 회색
WHITE     = RGBColor(0xFF, 0xFF, 0xFF)
GRAY_LIGHT= RGBColor(0xC8, 0xD0, 0xE0)
YELLOW    = RGBColor(0xFF, 0xD3, 0x60)

W = Inches(13.33)
H = Inches(7.5)

prs = Presentation()
prs.slide_width  = W
prs.slide_height = H

BLANK = prs.slide_layouts[6]  # 완전 빈 레이아웃

# ── 헬퍼 함수 ────────────────────────────────────────────────────────

def add_rect(slide, x, y, w, h, fill=BG_DARK, alpha=None):
    shape = slide.shapes.add_shape(1, x, y, w, h)
    shape.line.fill.background()
    shape.fill.solid()
    shape.fill.fore_color.rgb = fill
    return shape

def add_text(slide, text, x, y, w, h,
             size=18, bold=False, color=WHITE,
             align=PP_ALIGN.LEFT, wrap=True):
    txb = slide.shapes.add_textbox(x, y, w, h)
    txb.word_wrap = wrap
    tf = txb.text_frame
    tf.word_wrap = wrap
    p = tf.paragraphs[0]
    p.alignment = align
    run = p.add_run()
    run.text = text
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = color
    return txb

def slide_bg(slide, color=BG_DARK):
    bg = add_rect(slide, 0, 0, W, H, color)
    return bg

def title_bar(slide, title, subtitle=None):
    # 상단 강조 바
    add_rect(slide, 0, 0, W, Inches(1.3), BG_CARD)
    add_rect(slide, 0, Inches(1.3), W, Inches(0.07), ACCENT)

    add_text(slide, title,
             Inches(0.5), Inches(0.18), Inches(12), Inches(0.75),
             size=32, bold=True, color=WHITE)
    if subtitle:
        add_text(slide, subtitle,
                 Inches(0.5), Inches(0.88), Inches(12), Inches(0.35),
                 size=14, color=GRAY_LIGHT)

def bullet_box(slide, items, x, y, w, h, title=None, title_color=ACCENT):
    add_rect(slide, x, y, w, h, BG_CARD)
    # 왼쪽 강조 선
    add_rect(slide, x, y, Inches(0.06), h, ACCENT)

    ty = y
    if title:
        add_text(slide, title,
                 x + Inches(0.15), ty + Inches(0.1), w - Inches(0.2), Inches(0.4),
                 size=14, bold=True, color=title_color)
        ty += Inches(0.45)

    for item in items:
        icon = "▸ " if not item.startswith("  ") else "   · "
        add_text(slide, icon + item.lstrip(),
                 x + Inches(0.15), ty, w - Inches(0.25), Inches(0.38),
                 size=12, color=GRAY_LIGHT)
        ty += Inches(0.35)

def table_slide(slide, headers, rows, x, y, col_widths):
    cx = x
    # 헤더
    for i, (h_txt, cw) in enumerate(zip(headers, col_widths)):
        add_rect(slide, cx, y, cw, Inches(0.42), ACCENT)
        add_text(slide, h_txt, cx + Inches(0.05), y + Inches(0.05),
                 cw - Inches(0.1), Inches(0.32),
                 size=12, bold=True, color=WHITE)
        cx += cw

    for ri, row in enumerate(rows):
        cx = x
        bg = BG_CARD if ri % 2 == 0 else RGBColor(0x1F, 0x2D, 0x4A)
        for ci, (cell, cw) in enumerate(zip(row, col_widths)):
            add_rect(slide, cx, y + Inches(0.42) + ri * Inches(0.42), cw, Inches(0.42), bg)
            add_text(slide, cell,
                     cx + Inches(0.05),
                     y + Inches(0.42) + ri * Inches(0.42) + Inches(0.06),
                     cw - Inches(0.1), Inches(0.32),
                     size=11, color=GRAY_LIGHT)
            cx += cw

# ═══════════════════════════════════════════════════════════════════
# 슬라이드 1 — 표지
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)

# 장식 사각형
add_rect(slide, 0, 0, Inches(0.5), H, ACCENT)
add_rect(slide, W - Inches(0.5), 0, Inches(0.5), H, ACCENT)
add_rect(slide, 0, H - Inches(0.12), W, Inches(0.12), ACCENT)

add_text(slide, "캡스톤 디자인",
         Inches(1), Inches(1.5), Inches(11), Inches(1.2),
         size=48, bold=True, color=WHITE, align=PP_ALIGN.CENTER)

add_text(slide, "3D BackRoom 파트  |  작업 내역 정리",
         Inches(1), Inches(2.9), Inches(11), Inches(0.7),
         size=24, color=ACCENT, align=PP_ALIGN.CENTER)

add_rect(slide, Inches(4), Inches(3.7), Inches(5.33), Inches(0.04), ACCENT2)

info = [
    "담당자 :  GODOFPINGU (변우석)",
    "기  간 :  2026년 4월 26일 ~ 6월 22일",
    "엔  진 :  Unity 6  /  HDRP 17.4.0",
    "담  당 :  3D BackRoom 파트 전체",
]
for i, line in enumerate(info):
    add_text(slide, line,
             Inches(2.5), Inches(3.9) + i * Inches(0.52), Inches(9), Inches(0.45),
             size=16, color=GRAY_LIGHT, align=PP_ALIGN.CENTER)


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 2 — 목차
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "목차", "Contents")

items_left = [
    ("01", "초기 환경 설정",         "HDRP 세팅, 레벨 디자인, 플레이어 배치"),
    ("02", "BackRoom 씬 기초",        "플레이어 컨트롤러, 인벤토리, 수중 트리거"),
    ("03", "사운드 시스템",           "발소리, 배경음, 수중음, 형광등, 비명"),
    ("04", "형광등 시스템",           "개별 깜빡임, 전체 암전, Point Light 페어링"),
    ("05", "몬스터 AI",               "시야 감지, NavMesh 추격, 킬 거리"),
    ("06", "몬스터 스폰",             "랜덤 위치, 물속/끼임 방지"),
]
items_right = [
    ("07", "문 / 탈출 시스템",        "열쇠 문, 슬라이드, 씬 전환"),
    ("08", "수중 / 바다 시스템",      "수영, 침강, 산소, 언더워터 이펙트"),
    ("09", "죽음 연출",               "3단계 카메라 연출 + 슬로우모션"),
    ("10", "포스트 프로세싱",         "HDRP Volume, 사망 시 섬광/핏빛/색수차"),
    ("11", "UI 시스템",               "자막, 크로스헤어, 산소바, 인벤토리"),
    ("12", "버그 수정",               "점프, 동시킬, 스폰 끼임 등"),
]

for i, (num, ttl, desc) in enumerate(items_left):
    y = Inches(1.55) + i * Inches(0.88)
    add_rect(slide, Inches(0.4), y, Inches(6.0), Inches(0.75), BG_CARD)
    add_rect(slide, Inches(0.4), y, Inches(0.06), Inches(0.75), ACCENT)
    add_text(slide, num, Inches(0.55), y + Inches(0.06), Inches(0.5), Inches(0.35),
             size=20, bold=True, color=ACCENT)
    add_text(slide, ttl, Inches(1.1), y + Inches(0.04), Inches(2.5), Inches(0.35),
             size=14, bold=True, color=WHITE)
    add_text(slide, desc, Inches(1.1), y + Inches(0.38), Inches(5.0), Inches(0.3),
             size=11, color=GRAY_LIGHT)

for i, (num, ttl, desc) in enumerate(items_right):
    y = Inches(1.55) + i * Inches(0.88)
    add_rect(slide, Inches(6.9), y, Inches(6.0), Inches(0.75), BG_CARD)
    add_rect(slide, Inches(6.9), y, Inches(0.06), Inches(0.75), ACCENT)
    add_text(slide, num, Inches(7.05), y + Inches(0.06), Inches(0.5), Inches(0.35),
             size=20, bold=True, color=ACCENT)
    add_text(slide, ttl, Inches(7.6), y + Inches(0.04), Inches(2.5), Inches(0.35),
             size=14, bold=True, color=WHITE)
    add_text(slide, desc, Inches(7.6), y + Inches(0.38), Inches(5.0), Inches(0.3),
             size=11, color=GRAY_LIGHT)


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 3 — 초기 환경 설정
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "01  초기 환경 설정", "2026-04-26  |  hdrp 패키지 설치 · leve_design · 플레이어 집어넣기 · 임시저장 1차")

add_text(slide, "HDRP 패키지 설치",
         Inches(0.4), Inches(1.55), Inches(5.9), Inches(0.42),
         size=16, bold=True, color=ACCENT)
bullet_box(slide, [
    "Unity 프로젝트를 URP → HDRP 17.4.0 으로 전환",
    "High Definition Render Pipeline 패키지 설치 및 셋업",
    "HDRP 전용 라이팅 / 카메라 설정 초기화",
    "이후 모든 3D BackRoom 파트는 HDRP 기반으로 개발",
], Inches(0.4), Inches(1.95), Inches(5.9), Inches(1.9))

add_text(slide, "레벨 디자인 초안",
         Inches(6.9), Inches(1.55), Inches(5.9), Inches(0.42),
         size=16, bold=True, color=ACCENT)
bullet_box(slide, [
    "BackRoom 특유의 좁고 반복적인 공간 배치",
    "열쇠가 있는 방, 탈출 문 위치 기획",
    "형광등 배치 계획 수립",
], Inches(6.9), Inches(1.95), Inches(5.9), Inches(1.4))

add_text(slide, "플레이어 씬 배치",
         Inches(0.4), Inches(4.05), Inches(5.9), Inches(0.42),
         size=16, bold=True, color=ACCENT)
bullet_box(slide, [
    "CharacterController 기반 플레이어 프리팹 씬 배치",
    "1인칭 카메라 설정",
    "기본 이동 스크립트 초안 작성",
], Inches(0.4), Inches(4.45), Inches(5.9), Inches(1.4))

add_text(slide, "머지 충돌 해결",
         Inches(6.9), Inches(3.55), Inches(5.9), Inches(0.42),
         size=16, bold=True, color=ACCENT)
bullet_box(slide, [
    "팀원과 develop 브랜치 충돌 발생",
    "Ours 전략으로 충돌 해소 (merge resolve ours)",
    "이후 3D 파트 작업 독립 진행",
], Inches(6.9), Inches(3.95), Inches(5.9), Inches(1.4))


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 4 — BackRoom 씬 기초 구현
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "02  BackRoom 씬 기초 구현", "2026-06-15  |  Add BackRoom scene, player, inventory, water systems and pool prefab")

# 플레이어 컨트롤러
add_text(slide, "플레이어 컨트롤러  (BR_PlayerController)",
         Inches(0.4), Inches(1.55), Inches(12.5), Inches(0.4),
         size=16, bold=True, color=ACCENT)
feats = [
    "WASD 이동 + 마우스 1인칭 시점 회전",
    "달리기 (Shift), 점프 (Space) — CharacterController 물리 기반",
    "헤드 밥 (Head Bob) — 걷는 리듬에 따라 카메라 자연스럽게 흔들림",
    "수영 모드 자동 전환 — 물 트리거 진입 시 SwimMove로 전환",
]
bullet_box(slide, feats, Inches(0.4), Inches(1.95), Inches(12.5), Inches(1.75))

# 두 박스
add_text(slide, "인벤토리  (BR_Inventory)",
         Inches(0.4), Inches(3.85), Inches(5.9), Inches(0.4),
         size=16, bold=True, color=ACCENT)
bullet_box(slide, [
    "E키로 아이템 줍기",
    "TAB키로 인벤토리 열기/닫기",
    "우클릭 컨텍스트 메뉴 — 장착 / 드롭 선택",
    "손전등 아이템 장착 연동",
], Inches(0.4), Inches(4.25), Inches(5.9), Inches(1.8))

add_text(slide, "수중 트리거  (BR_WaterZone / BR_OceanZone)",
         Inches(6.9), Inches(3.85), Inches(5.9), Inches(0.4),
         size=16, bold=True, color=ACCENT)
bullet_box(slide, [
    "BR_WaterZone — 일반 수영 구역 (콜라이더 트리거)",
    "BR_OceanZone — 바다 구역 (자동 침강 + 산소 고갈)",
    "수영장 프리팹 제작 및 씬 배치",
    "플레이어 진입/이탈 이벤트 연동",
], Inches(6.9), Inches(4.25), Inches(5.9), Inches(1.8))


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 5 — 사운드 시스템
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "03  사운드 시스템", "BR_SoundManager — 씬 전체 사운드 통합 관리 (싱글톤)")

table_slide(
    slide,
    headers=["사운드 종류", "재생 조건", "구현 세부 사항"],
    rows=[
        ["발소리 (Footstep)",  "걷는 중",            "헤드밥 2보마다 1회, 여러 클립 중 랜덤 선택"],
        ["물 진입음",          "물 첫 접촉 시",       "1회 재생 (중복 방지 플래그)"],
        ["배경음 (BGM)",       "씬 시작 ~ 씬 종료",  "루프 재생, 별도 오디오 소스"],
        ["수중 앰비언트",      "물속 진입 시에만",    "진입/이탈 시 자동 재생/정지"],
        ["형광등 지지직",      "랜덤 간격",           "1~3회 연속 버스트, 코루틴 기반"],
        ["좀비 비명",          "사망 연출 + 문 열림", "단발 재생 (사망) + 루프 재생 (추격씬)"],
    ],
    x=Inches(0.4), y=Inches(1.55),
    col_widths=[Inches(2.5), Inches(2.8), Inches(7.1)],
)

add_text(slide, "주요 수정 사항",
         Inches(0.4), Inches(5.0), Inches(12.5), Inches(0.38),
         size=14, bold=True, color=YELLOW)
bullet_box(slide, [
    "기존에 '배경음 - 항상 재생' 으로 잘못 표기되어 있던 수중 사운드 채널 분리 및 명칭 수정",
    "BGM 전용 채널 신규 추가 — 배경음 / 수중음 / 발소리 / 비명을 각각 독립 AudioSource로 분리",
    "PlayFootstep() — 헤드밥 타이머 기반 2보마다 1회 트리거 (너무 자주 나는 문제 개선)",
], Inches(0.4), Inches(5.42), Inches(12.5), Inches(1.65))


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 6 — 형광등 시스템
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "04  형광등 시스템", "BR_FluorescentLight — 개별 깜빡임 · 전체 암전 · Point Light 페어링 · 소리 연동")

bullet_box(slide, [
    "씬 내 형광등 오브젝트 자동 탐색 — 오브젝트 이름 대소문자 구분 없이 검색",
    "다중 Renderer 지원 — 형광등 메시가 여러 하위 오브젝트에 분산된 경우에도 정상 동작",
], Inches(0.4), Inches(1.55), Inches(12.5), Inches(1.05), title="공통 기반")

add_text(slide, "개별 깜빡임",
         Inches(0.4), Inches(2.82), Inches(4.0), Inches(0.38),
         size=14, bold=True, color=ACCENT)
bullet_box(slide, [
    "각 형광등마다 독립적인 랜덤 주기",
    "켜짐↔꺼짐 간격 랜덤화로 자연스러운 연출",
    "개별 코루틴으로 비동기 실행",
], Inches(0.4), Inches(3.22), Inches(4.0), Inches(1.4))

add_text(slide, "전체 암전",
         Inches(4.65), Inches(2.82), Inches(4.0), Inches(0.38),
         size=14, bold=True, color=ACCENT)
bullet_box(slide, [
    "3~5초 랜덤 간격으로 발동",
    "모든 형광등 + Point Light 동시 꺼짐",
    "일정 시간 후 전체 복구",
    "공포 연출 극대화",
], Inches(4.65), Inches(3.22), Inches(4.0), Inches(1.58))

add_text(slide, "Point Light 페어링",
         Inches(9.0), Inches(2.82), Inches(4.0), Inches(0.38),
         size=14, bold=True, color=ACCENT)
bullet_box(slide, [
    "형광등 오브젝트 하위 자식 자동 탐색",
    "Point Light 컴포넌트 있으면 자동 연동",
    "깜빡임/암전 시 함께 제어",
    "씬에서 별도 설정 불필요",
], Inches(9.0), Inches(3.22), Inches(4.0), Inches(1.58))

add_text(slide, "형광등 지지직 사운드 연동 — BR_SoundManager 통해 랜덤 간격으로 지지직 사운드 1~3회 연속 재생",
         Inches(0.4), Inches(5.0), Inches(12.5), Inches(0.5),
         size=12, color=GRAY_LIGHT)


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 7 — 몬스터 AI
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "05  몬스터 AI", "BR_Monster — NavMeshAgent 기반 추격 · 시야 감지 · 문 열림 후 전방위 추격")

add_text(slide, "NavMesh 추격 AI",
         Inches(0.4), Inches(1.55), Inches(4.1), Inches(0.38),
         size=14, bold=True, color=ACCENT)
bullet_box(slide, [
    "NavMeshAgent 자동 경로 탐색",
    "chaseRange — 추격 시작 범위",
    "detectionRange — 시야 감지 범위",
    "killDistance — 플레이어 사망 거리",
    "설정값 Inspector에서 조절 가능",
], Inches(0.4), Inches(1.95), Inches(4.1), Inches(2.2))

add_text(slide, "시야 감지 시스템",
         Inches(4.7), Inches(1.55), Inches(4.1), Inches(0.38),
         size=14, bold=True, color=ACCENT)
bullet_box(slide, [
    "플레이어가 몬스터를 바라보면 동결",
    "3개 체크포인트 (발·허리·머리)",
    "Viewport 좌표로 화면 내 감지",
    "RaycastAll로 벽 관통 방지",
    "hit 오브젝트 필터 (플레이어/몬스터 제외)",
], Inches(4.7), Inches(1.95), Inches(4.1), Inches(2.2))

add_text(slide, "문 열림 후 추격씬",
         Inches(9.0), Inches(1.55), Inches(4.0), Inches(0.38),
         size=14, bold=True, color=ACCENT)
bullet_box(slide, [
    "BR_Monster.alwaysChase = true",
    "시야 감지 완전 해제",
    "모든 몬스터 즉시 추격 시작",
    "새 스폰 몬스터도 동일 상태 유지",
    "(Start() 에서 리셋 방지 처리)",
], Inches(9.0), Inches(1.95), Inches(4.0), Inches(2.2))

add_text(slide, "난이도 조절  (PlayerPrefs 기반)",
         Inches(0.4), Inches(4.35), Inches(12.5), Inches(0.38),
         size=14, bold=True, color=YELLOW)
bullet_box(slide, [
    "첫 플레이: 몬스터 이동속도 15  →  매우 빠름, 플레이어가 한 번은 죽게 됨",
    "사망 후 재시작: 몬스터 이동속도 4  →  PlayerPrefs.SetInt(\"BR_HasDied\", 1) 로 영구 기록",
    "씬 재시작 후에도 값이 유지되어 자동으로 난이도 감소 적용",
], Inches(0.4), Inches(4.78), Inches(12.5), Inches(1.5))


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 8 — 몬스터 스폰 + 문/탈출
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "06 · 07  몬스터 스폰  /  문 · 탈출 시스템", "BR_MonsterSpawner  /  BR_Door")

add_text(slide, "몬스터 스폰  (BR_MonsterSpawner)",
         Inches(0.4), Inches(1.55), Inches(6.0), Inches(0.38),
         size=16, bold=True, color=ACCENT)
bullet_box(slide, [
    "NavMesh 위 랜덤 위치 선택 후 스폰",
    "플레이어와 최소 이격 거리 보장 (너무 가까이 스폰 방지)",
    "물속 스폰 방지 — IsInWater() 체크",
    "좁은 공간 끼임 방지 — Physics.CheckCapsule() 로 스폰 전 공간 확인",
    "  └ 캡슐 반경 0.4m, 높이 1.8m 기준으로 장애물 체크",
    "Activate() — 문 열릴 때 대량 추가 스폰 트리거",
    "게임 시작 시 기본 스폰 유지 + 문 열림 시 추가 스폰 분리",
], Inches(0.4), Inches(1.95), Inches(6.0), Inches(2.9))

add_text(slide, "문 / 탈출 시스템  (BR_Door)",
         Inches(6.9), Inches(1.55), Inches(6.0), Inches(0.38),
         size=16, bold=True, color=ACCENT)
bullet_box(slide, [
    "열쇠 아이템 보유 시에만 열 수 있는 잠긴 문",
    "열릴 때 슬라이드 애니메이션 재생",
    "  └ 방향: Up / Down / Left / Right / Custom 설정 가능",
    "  └ 슬라이드 거리 자동 계산 또는 수동 설정",
    "탈출 문 (isExitDoor) — 열리면 다음 씬으로 전환",
    "문 열림 이벤트 시 연쇄 트리거:",
    "  └ 몬스터 대량 추가 스폰",
    "  └ alwaysChase = true (전방위 추격)",
    "  └ 좀비 비명 루프 재생 시작",
], Inches(6.9), Inches(1.95), Inches(6.0), Inches(3.5))


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 9 — 수중/바다 시스템
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "08  수중 / 바다 시스템", "수영 · 침강 · 산소 · 언더워터 이펙트 · UI 산소바")

feats = [
    ("수영 모드 자동 전환",  "물 트리거 진입 시 SwimMove 로 자동 전환, 이탈 시 GroundMove 복귀"),
    ("자동 침강 (sinkSpeed)","바다(OceanZone) 에서 가만히 있으면 아래로 서서히 가라앉음"),
    ("산소 시스템",          "잠수 시 산소 감소, 수면 복귀 시 회복 — 고갈 시 씬 재시작 (사망 처리)"),
    ("언더워터 이펙트",      "물속 진입 시 수중 앰비언트 사운드 자동 재생 / 이탈 시 정지"),
    ("UI 산소바",            "잠수 시에만 화면 하단에 O2 퍼센트 표시, 평소에는 숨김"),
    ("지상 점프 복구 (수정)","물 트리거 안이라도 바닥에 서 있으면 정상 점프 가능하도록 버그 수정"),
]

for i, (ttl, desc) in enumerate(feats):
    y = Inches(1.55) + i * Inches(0.9)
    add_rect(slide, Inches(0.4), y, Inches(12.5), Inches(0.78), BG_CARD)
    add_rect(slide, Inches(0.4), y, Inches(0.06), Inches(0.78), ACCENT)
    add_text(slide, ttl,  Inches(0.6), y + Inches(0.05), Inches(3.2), Inches(0.35),
             size=13, bold=True, color=WHITE)
    add_text(slide, desc, Inches(3.9), y + Inches(0.2), Inches(9.0), Inches(0.45),
             size=12, color=GRAY_LIGHT)


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 10 — 죽음 연출
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "09  죽음 연출 시스템", "BR_Monster.KillPlayer() — 3단계 카메라 연출 + 슬로우모션 + 비명")

phases = [
    ("Phase 0",  "0.2초",   ACCENT,
     ["타임스케일 0.15× 슬로우모션 적용",
      "카메라 제자리에서 폭발적 흔들림",
      "흔들림 강도: shakeIntensity × 2.4",
      "회전 ±90° 격렬한 진동",
      "→ 순간 충격감 연출"]),
    ("Phase 1",  "0.4초",   RGBColor(0xF7, 0x8C, 0x3F),
     ["타임스케일 1.0 복원",
      "카메라가 몬스터 얼굴로 빠르게 돌진",
      "EaseIn 이징 (t^0.45) — 끝에 빠르게",
      "강한 흔들림 함께 적용",
      "방향: 플레이어→머리 벡터 계산"]),
    ("Phase 2",  "1.2초",   RGBColor(0x2E, 0x86, 0xAB),
     ["몬스터 얼굴 클로즈업 유지",
      "흔들림 서서히 감쇠 (Pow 1.5 커브)",
      "좀비 비명 사운드 재생",
      "포스트 프로세싱 효과 지속",
      "→ 씬 재시작"]),
]

for i, (phase, dur, col, bullets) in enumerate(phases):
    x = Inches(0.4) + i * Inches(4.3)
    add_rect(slide, x, Inches(1.55), Inches(4.1), Inches(4.9), BG_CARD)
    add_rect(slide, x, Inches(1.55), Inches(4.1), Inches(0.55), col)
    add_text(slide, phase, x + Inches(0.12), Inches(1.6), Inches(2.0), Inches(0.38),
             size=18, bold=True, color=WHITE)
    add_text(slide, dur, x + Inches(2.5), Inches(1.68), Inches(1.5), Inches(0.3),
             size=12, color=WHITE, align=PP_ALIGN.RIGHT)
    for j, b in enumerate(bullets):
        add_text(slide, "▸ " + b,
                 x + Inches(0.15), Inches(2.2) + j * Inches(0.44),
                 Inches(3.85), Inches(0.4),
                 size=11, color=GRAY_LIGHT)

add_text(slide, "카메라 방향 계산 핵심:  headPos - camDir × closeupDistance  →  항상 몬스터 얼굴이 정면에 보이도록 벡터 기반 배치",
         Inches(0.4), Inches(6.6), Inches(12.5), Inches(0.45),
         size=11, color=YELLOW)


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 11 — HDRP 포스트 프로세싱
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "10  HDRP 포스트 프로세싱", "BR_PostProcessing — 평소 비활성 / 사망 시에만 발동 (HDRP Volume 동적 생성)")

add_text(slide, "설계 원칙",
         Inches(0.4), Inches(1.55), Inches(12.5), Inches(0.38),
         size=14, bold=True, color=YELLOW)
bullet_box(slide, [
    "평소에는 포스트 프로세싱 완전 비활성 — 화면 그대로 유지 (너무 어두운 문제 해결)",
    "HDRP Volume을 코드로 런타임 동적 생성 — 씬에 컴포넌트 배치 불필요",
    "TriggerDeathEffect() 호출 시에만 아래 3단계 효과 순서대로 발동",
], Inches(0.4), Inches(1.95), Inches(12.5), Inches(1.3))

stages = [
    ("0단계  0.05초",  "흰 섬광",   "postExposure +2.8\n화면이 순간 하얗게 번쩍임"),
    ("1단계  0.25초",  "핏빛 전환", "흰색 → 붉은 색 필터\nLens Distortion 점점 강해짐"),
    ("2단계  1.5초",   "효과 감쇠", "ChromaticAberration 서서히 감소\nLens Distortion 0으로 복귀"),
]
for i, (step, title, desc) in enumerate(stages):
    x = Inches(0.4) + i * Inches(4.3)
    y = Inches(3.45)
    add_rect(slide, x, y, Inches(4.1), Inches(2.5), BG_CARD)
    add_rect(slide, x, y, Inches(4.1), Inches(0.08), ACCENT)
    add_text(slide, step,  x + Inches(0.12), y + Inches(0.12), Inches(3.9), Inches(0.3),
             size=11, color=GRAY_LIGHT)
    add_text(slide, title, x + Inches(0.12), y + Inches(0.42), Inches(3.9), Inches(0.42),
             size=16, bold=True, color=WHITE)
    add_text(slide, desc,  x + Inches(0.12), y + Inches(0.9), Inches(3.85), Inches(1.4),
             size=12, color=GRAY_LIGHT)

add_text(slide, "적용 효과 목록:  Vignette(비활성)  /  ColorAdjustments  /  ChromaticAberration  /  LensDistortion  /  FilmGrain",
         Inches(0.4), Inches(6.6), Inches(12.5), Inches(0.45),
         size=11, color=YELLOW)


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 12 — UI 시스템
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "11  UI 시스템", "BR_UIManager — IMGUI 기반 (Canvas 없음) · 크로스헤어 · 프롬프트 · 자막 · 산소바")

ui_items = [
    ("크로스헤어",       "항상 표시, 화면 정중앙에 + 모양"),
    ("상호작용 프롬프트","아이템/문 근처에서 [E] 줍기 · [잠김] · [열려있음] 등 표시"),
    ("인벤토리 (TAB)",   "보유 아이템 목록, 우클릭 컨텍스트 메뉴 (장착/드롭/취소)"),
    ("산소바 (O2)",      "잠수 시에만 화면 하단 표시, 평소 완전 숨김"),
    ("시작 자막",        "씬 시작 시 \"열쇠를 찾아 비밀문을 열고 탈출하세요.\" 페이드인/아웃"),
    ("손전등 장착 표시", "손전등 장착 상태 인벤토리에 강조 표시"),
]

for i, (name, desc) in enumerate(ui_items):
    y = Inches(1.55) + i * Inches(0.85)
    add_rect(slide, Inches(0.4), y, Inches(12.5), Inches(0.72), BG_CARD)
    add_rect(slide, Inches(0.4), y, Inches(0.06), Inches(0.72), ACCENT)
    add_text(slide, name, Inches(0.6), y + Inches(0.06), Inches(3.0), Inches(0.35),
             size=13, bold=True, color=WHITE)
    add_text(slide, desc, Inches(3.75), y + Inches(0.18), Inches(9.0), Inches(0.4),
             size=12, color=GRAY_LIGHT)

add_text(slide, "시작 자막 애니메이션:  페이드인 0.8초 → 유지 4초 → 페이드아웃 0.8초  /  ShowSubtitle(string) 으로 커스텀 자막도 표시 가능",
         Inches(0.4), Inches(6.65), Inches(12.5), Inches(0.42),
         size=11, color=YELLOW)


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 13 — 버그 수정
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "12  버그 수정 내역", "개발 과정에서 발생한 주요 버그와 해결 방법")

table_slide(
    slide,
    headers=["버그 증상", "원인", "해결 방법"],
    rows=[
        ["점프가 가끔 안 먹힘",
         "cc.isGrounded를 cc.Move() 이후에 체크 — 값이 달라짐",
         "프레임 시작 시 bool grounded = cc.isGrounded 미리 캐싱"],
        ["몬스터 여러 마리 동시 죽음 연출",
         "킬 거리 내에 여러 몬스터가 동시에 진입",
         "static bool anyKilling 플래그로 첫 번째만 실행"],
        ["문 연 후 새 몬스터가 다시 얌전해짐",
         "새 몬스터 Start()에서 alwaysChase = false 리셋",
         "alwaysChase 초기화 코드 제거, anyKilling만 리셋"],
        ["물속에 몬스터 스폰됨",
         "스폰 위치에 물속 여부 체크 없음",
         "IsInWater() 체크 추가"],
        ["좁은 공간에서 몬스터 끼임",
         "스폰 위치에 공간 체크 없음",
         "Physics.CheckCapsule() 로 스폰 전 공간 사전 확인"],
        ["죽음 연출 시 몬스터 얼굴 안 보임",
         "transform.forward 가 실제 시선과 달랐음",
         "카메라→머리 벡터 직접 계산하여 방향 결정"],
        ["지상 점프가 물 트리거 안에서 안 먹힘",
         "트리거 진입 시 수영 모드로 전환되며 점프 불가",
         "isGrounded 시 GroundMove 강제 적용"],
    ],
    x=Inches(0.4), y=Inches(1.55),
    col_widths=[Inches(3.2), Inches(4.5), Inches(5.0)],
)


# ═══════════════════════════════════════════════════════════════════
# 슬라이드 14 — 타임라인 + 마무리
# ═══════════════════════════════════════════════════════════════════
slide = prs.slides.add_slide(BLANK)
slide_bg(slide)
title_bar(slide, "개발 타임라인  /  주요 스크립트", "전체 작업 흐름 요약")

timeline = [
    ("2026-04-26", "환경 설정",   "HDRP 패키지 설치, 레벨 디자인 초안, 플레이어 씬 배치, 머지 충돌 해결"),
    ("2026-06-15", "씬 기초 구현","BackRoom 씬 구성, 플레이어 컨트롤러, 인벤토리, 수중 트리거, 수영장 프리팹"),
    ("2026-06-21", "핵심 기능",   "사운드·형광등·몬스터AI·스폰·수중바다·문탈출·죽음연출·점프버그 수정 (14커밋)"),
    ("2026-06-22", "완성도 향상", "HDRP 포스트 프로세싱, 시작 자막, 난이도 조절, 스폰 끼임 방지, 손전등 (6커밋)"),
]

for i, (date, phase, desc) in enumerate(timeline):
    y = Inches(1.55) + i * Inches(0.88)
    add_rect(slide, Inches(0.4), y, Inches(12.5), Inches(0.75), BG_CARD)
    add_rect(slide, Inches(0.4), y, Inches(0.06), Inches(0.75), ACCENT)
    add_text(slide, date,  Inches(0.6),  y + Inches(0.06), Inches(1.6), Inches(0.3),
             size=11, color=ACCENT)
    add_text(slide, phase, Inches(2.3),  y + Inches(0.04), Inches(2.0), Inches(0.35),
             size=13, bold=True, color=WHITE)
    add_text(slide, desc,  Inches(4.5),  y + Inches(0.2),  Inches(8.2), Inches(0.45),
             size=11, color=GRAY_LIGHT)

scripts = [
    "BR_PlayerController", "BR_Monster", "BR_MonsterSpawner", "BR_Door",
    "BR_Inventory", "BR_UIManager", "BR_SoundManager",
    "BR_PostProcessing", "BR_FlashlightController", "BR_FluorescentLight",
    "BR_WaterZone / BR_OceanZone",
]
add_text(slide, "작성 스크립트 목록",
         Inches(0.4), Inches(5.25), Inches(12.5), Inches(0.38),
         size=13, bold=True, color=YELLOW)
row1 = "    ".join(scripts[:6])
row2 = "    ".join(scripts[6:])
add_text(slide, row1, Inches(0.4), Inches(5.65), Inches(12.5), Inches(0.38), size=11, color=GRAY_LIGHT)
add_text(slide, row2, Inches(0.4), Inches(6.05), Inches(12.5), Inches(0.38), size=11, color=GRAY_LIGHT)

add_text(slide, "총  29개  커밋  (GODOFPINGU)  —  3D BackRoom 파트 전체 기획 · 구현 · 버그수정 담당",
         Inches(0.4), Inches(6.6), Inches(12.5), Inches(0.45),
         size=12, bold=True, color=WHITE, align=PP_ALIGN.CENTER)


# ── 저장 ─────────────────────────────────────────────────────────
out = r"C:\Users\wcm20\OneDrive\문서\GitHub\Capstone-Design\작업내역_정리.pptx"
prs.save(out)
print("저장 완료:", out)
