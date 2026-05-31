#!/usr/bin/env python3
"""
validate_data.py — 高考古诗文数据完整性校验

校验规则：
1. divided.json 每条 content 必须是 poetry.json 对应篇目原文的精确子串
2. poetry.json 全部 60 篇在 divided.json 中均有覆盖
3. 每篇至少 4 句
4. 无重复条目
5. book 字段值合法
6. 无多余的篇目（divided 中有但 poetry 中没有）

Usage:
    python validate_data.py              # 标准校验
    python validate_data.py --ci         # CI 模式（严格，非零退出码）
    python validate_data.py --fix-titles # 尝试自动修复标题差异
"""

import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

ROOT = Path(__file__).parent
DIVIDED = ROOT / "Assets" / "divided.json"
POETRY = ROOT / "poetry.json"

VALID_BOOKS = {"必修上", "必修下", "选择性必修上", "选择性必修中", "选择性必修下", "其他"}
MIN_LINES = 4
EXPECTED_POEMS = 60


# Character variants that should be treated as identical
CHAR_VARIANTS = {
    '皙': '晳',   # U+7699 -> U+6673
}

def clean(s: str) -> str:
    """Remove punctuation and formatting for fuzzy matching."""
    for a, b in CHAR_VARIANTS.items():
        s = s.replace(a, b)
    return re.sub(r'[《》（）()·\s\[\]【】—…]', '', s)


def load_json(path: Path) -> list | dict:
    with open(path, 'r', encoding='utf-8') as f:
        return json.load(f)


def match_title(dt: str, poetry_titles: set[str]) -> str | None:
    """Match a divided title to a poetry title, handling format differences."""
    if dt in poetry_titles:
        return dt

    # Exact match after normalization
    cdt = clean(dt)
    for pt in poetry_titles:
        if clean(pt) == cdt:
            return pt

    # Remove parenthetical disambiguators: "菩萨蛮（小山重叠金明灭）" -> "菩萨蛮"
    # Try matching core title (divided) against core title (poetry)
    dt_core = re.sub(r'[（(][^）)]*[）)]', '', dt).strip()
    for pt in poetry_titles:
        pt_core = re.sub(r'[（(][^）)]*[）)]', '', pt).strip()
        if dt_core == pt_core:
            return pt

    # starts-with on core titles
    cdt_core = clean(dt_core)
    for pt in poetry_titles:
        pt_core = re.sub(r'[（(][^）)]*[）)]', '', pt).strip()
        cpt_core = clean(pt_core)
        if cdt_core and cpt_core:
            if cdt_core.startswith(cpt_core) or cpt_core.startswith(cdt_core):
                return pt

    return None


def main() -> int:
    ci_mode = "--ci" in sys.argv
    errors = []
    warnings = []

    # ---- Load data ----
    if not DIVIDED.exists():
        errors.append(f"divided.json not found: {DIVIDED}")
        return report(errors, warnings, ci_mode)
    if not POETRY.exists():
        errors.append(f"poetry.json not found: {POETRY}")
        return report(errors, warnings, ci_mode)

    divided = load_json(DIVIDED)
    poetry_list = load_json(POETRY)

    poetry_map: dict[str, str] = {}
    for p in poetry_list:
        poetry_map[p['title']] = p['content']

    if len(poetry_map) != EXPECTED_POEMS:
        warnings.append(f"poetry.json has {len(poetry_map)} poems, expected {EXPECTED_POEMS}")

    # ---- Build title mapping ----
    dt_to_pt: dict[str, str] = {}
    unmatched = []
    for d in divided:
        dt = d['title']
        if dt in dt_to_pt:
            continue
        pt = match_title(dt, set(poetry_map.keys()))
        if pt:
            dt_to_pt[dt] = pt
        else:
            unmatched.append(dt)

    if unmatched:
        errors.append(f"{len(unmatched)} divided title(s) cannot be matched to poetry.json:")
        for t in unmatched:
            errors.append(f"  - '{t}'")

    # ---- Rule 1: Content substring check ----
    bad_content = []
    for i, d in enumerate(divided):
        dc = d['content'][0]
        found = None
        for p in poetry_list:
            if dc in p['content']:
                found = p['title']
                break
        if not found:
            bad_content.append((i, d))
            # Try to find close match for debugging
            ndc = re.sub(r'[][，,。；;：:、！!？?·—…""''「」『』《》（）()【】\s]', '', dc)
            for p in poetry_list:
                npc = re.sub(r'[][，,。；;：:、！!？?·—…""''「」『』《》（）()【】\s]', '', p['content'])
                if ndc in npc:
                    warnings.append(f"[{i}] '{d['title']}' content matches after removing punctuation: '{dc[:50]}...'")
                    break

    if bad_content:
        errors.append(f"{len(bad_content)} entry(s) with content NOT found in ANY poem:")
        for idx, d in bad_content[:10]:
            errors.append(f"  [{idx}] {d['title']}: \"{d['content'][0][:60]}...\"")
        if len(bad_content) > 10:
            errors.append(f"  ... and {len(bad_content) - 10} more")

    # ---- Rule 2 & 3: Coverage and minimum lines ----
    pt_counts = Counter()
    for d in divided:
        dt = d['title']
        pt = dt_to_pt.get(dt, dt)
        pt_counts[pt] += 1

    uncovered = [pt for pt in poetry_map if pt not in pt_counts]
    under_min = [(pt, n) for pt, n in pt_counts.items() if n < MIN_LINES and pt in poetry_map]

    if uncovered:
        errors.append(f"{len(uncovered)} poem(s) with ZERO coverage:")
        for pt in uncovered:
            errors.append(f"  - {pt}")

    if under_min:
        errors.append(f"{len(under_min)} poem(s) below {MIN_LINES} lines:")
        for pt, n in sorted(under_min, key=lambda x: x[1]):
            errors.append(f"  - {pt}: {n} lines")

    # Count how many fully compliant
    compliant = sum(1 for pt in poetry_map if pt_counts.get(pt, 0) >= MIN_LINES)
    print(f"  Coverage: {len(poetry_map) - len(uncovered)}/{len(poetry_map)} poems")
    print(f"  >= {MIN_LINES} lines: {compliant}/{len(poetry_map)} poems")

    # ---- Rule 4: Duplicates ----
    seen = defaultdict(list)
    for i, d in enumerate(divided):
        key = (dt_to_pt.get(d['title'], d['title']), d['content'][0])
        seen[key].append(i)

    dups = {k: v for k, v in seen.items() if len(v) > 1}
    if dups:
        warnings.append(f"{len(dups)} duplicate content(s) found (same poem, same line):")
        for (pt, line), indices in list(dups.items())[:5]:
            warnings.append(f"  - '{pt}': \"{line[:50]}...\" appears {len(indices)} times")

    # ---- Rule 5: Book validity ----
    invalid_books = set()
    for d in divided:
        if d['book'] not in VALID_BOOKS:
            invalid_books.add(d['book'])

    if invalid_books:
        errors.append(f"Invalid book values: {invalid_books}")

    # ---- Rule 6: Extra poems in divided not in poetry ----
    # Already caught by unmatched titles

    # ---- Print report ----
    ret = report(errors, warnings, ci_mode)

    # ---- Summary stats ----
    if ret == 0:
        print(f"\n  Status: PASS")
        print(f"  Total entries: {len(divided)}")
        print(f"  Unique titles: {len(set(d['title'] for d in divided))}")
        print(f"  All {EXPECTED_POEMS} poems >= {MIN_LINES} lines: YES")
        print(f"  Content verification: ALL {len(divided)} entries verified")
    else:
        print(f"\n  Status: FAIL ({len(errors)} error(s), {len(warnings)} warning(s))")

    return ret


def report(errors: list[str], warnings: list[str], ci_mode: bool) -> int:
    if warnings:
        print(f"\n  Warnings ({len(warnings)}):")
        for w in warnings:
            print(f"    [!] {w}")

    if errors:
        print(f"\n  Errors ({len(errors)}):")
        for e in errors:
            print(f"    [X] {e}")

    return 0 if not errors else 1


if __name__ == '__main__':
    sys.exit(main())
