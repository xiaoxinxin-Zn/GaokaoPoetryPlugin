#!/usr/bin/env python3
"""
下载高考古诗文数据并添加分册信息。

Usage:
    python download_data.py              # 下载并打标签
    python download_data.py --validate   # 下载后自动运行数据校验

数据源: https://clover-yan.github.io/gaokao-poetry/ (CC BY-SA 4.0)
校验: 运行 validate_data.py 确保 divided.json 内容与 poetry.json 一致
"""
import urllib.request
import json
import os
import sys

URL = "https://clover-yan.github.io/gaokao-poetry/generated/divided.json"
OUTPUT = os.path.join(os.path.dirname(__file__), "Assets", "divided.json")

# 篇目标题 → 教材册别映射（需与 poetry.json 和 divided.json 中的标题保持一致）
BOOK_MAP = {
    "静女": "必修上", "涉江采芙蓉": "必修上", "短歌行": "必修上",
    "归园田居·其一": "必修上", "梦游天姥吟留别": "必修上", "登高": "必修上",
    "琵琶行": "必修上", "念奴娇·赤壁怀古": "必修上", "永遇乐·京口北固亭怀古": "必修上",
    "声声慢": "必修上", "劝学": "必修上", "师说": "必修上",
    "赤壁赋": "必修上", "登泰山记": "必修上", "虞美人·春花秋月何时了": "必修上",
    "鹊桥仙·纤云弄巧": "必修上",
    "子路、曾皙、冉有、公西华侍坐": "必修下", "谏太宗十思疏": "必修下",
    "答司马谏议书": "必修下", "阿房宫赋": "必修下", "六国论": "必修下",
    "登岳阳楼": "必修下", "桂枝香·金陵怀古": "必修下", "念奴娇·过洞庭": "必修下",
    "论语十二章": "选择性必修上", "无衣": "选择性必修上", "春江花月夜": "选择性必修上",
    "将进酒": "选择性必修上", "江城子·乙卯正月二十日夜记梦": "选择性必修上",
    "屈原列传": "选择性必修中", "过秦论（上）": "选择性必修中", "五代史伶官传序": "选择性必修中",
    "燕歌行": "选择性必修中", "李凭箜篌引": "选择性必修中", "锦瑟": "选择性必修中",
    "书愤": "选择性必修中",
    "离骚": "选择性必修下", "蜀道难": "选择性必修下", "蜀相": "选择性必修下",
    "望海潮": "选择性必修下", "扬州慢·淮左名都": "选择性必修下", "陈情表": "选择性必修下",
    "项脊轩志": "选择性必修下", "归去来兮辞（并序）": "选择性必修下", "种树郭橐驼传": "选择性必修下",
    "石钟山记": "选择性必修下", "拟行路难·其四": "选择性必修下", "客至": "选择性必修下",
    "登快阁": "选择性必修下", "临安春雨初霁": "选择性必修下",
    "报任安书": "其他", "礼运": "其他", "山居秋暝": "其他", "菩萨蛮": "其他",
    "苏幕遮·燎沉香": "其他", "青玉案·元夕": "其他", "贺新郎·国脉微如缕": "其他",
    "菩萨蛮·书江西造口壁": "其他", "长亭送别": "其他", "朝天子·咏喇叭": "其他",
}


def main():
    print(f"正在从 {URL} 下载数据...")
    with urllib.request.urlopen(URL) as response:
        data = json.loads(response.read().decode('utf-8'))

    unassigned = []
    for item in data:
        title = item.get("title", "")
        book = BOOK_MAP.get(title)
        if book:
            item["book"] = book
        else:
            item["book"] = "其他"
            unassigned.append(title)

    os.makedirs(os.path.dirname(OUTPUT), exist_ok=True)
    with open(OUTPUT, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    print(f"数据已保存到: {OUTPUT}（共 {len(data)} 条，已添加分册信息）")

    if unassigned:
        print(f"\n[!] 以下 {len(unassigned)} 个新篇目未在 BOOK_MAP 中，已标记为'其他'：")
        for t in sorted(set(unassigned)):
            print(f"    - {t}")
        print("    请更新 download_data.py 中的 BOOK_MAP。")

    if "--validate" in sys.argv:
        print("\n" + "=" * 50)
        print("运行数据校验...")
        import subprocess
        script = os.path.join(os.path.dirname(__file__), "validate_data.py")
        result = subprocess.run([sys.executable, script], capture_output=False)
        if result.returncode != 0:
            print("\n[!] 数据校验未通过，请检查并修复后重新构建。")
            sys.exit(1)
        else:
            print("[OK] 数据校验通过")


if __name__ == "__main__":
    main()
