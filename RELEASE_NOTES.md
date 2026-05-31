# v1.2.0.0 更新说明

> **发布日期**: 2026-05-31  
> **下载**: [GaokaoPoetryPlugin.cipx](https://github.com/xiaoxinxin-Zn/GaokaoPoetryPlugin/releases/download/v1.2.0.0/GaokaoPoetryPlugin.cipx)

---

## 一、数据层重大更新

### 1.1 60 篇全覆盖，每篇 ≥4 句

上一版本部分篇目覆盖不足。本次将所有 60 篇课标必背古诗文全部补全至不少于 4 句，最终 **251 条名句**，分布如下：

| 教材册别 | 篇目数 | 名句数 | 代表篇目 |
|----------|--------|--------|----------|
| 必修上册 | 16 | 67 | 赤壁赋、登高、琵琶行、劝学、短歌行等 |
| 必修下册 | 8 | 32 | 阿房宫赋、六国论、谏太宗十思疏等 |
| 选择性必修上册 | 5 | 23 | 论语十二章、春江花月夜、将进酒等 |
| 选择性必修中册 | 7 | 29 | 屈原列传、过秦论、燕歌行等 |
| 选择性必修下册 | 14 | 60 | 离骚、蜀道难、陈情表、项脊轩志等 |
| 其他 | 10 | 40 | 报任安书、山居秋暝、青玉案·元夕等 |
| **合计** | **60** | **251** | |

### 1.2 内容精确对齐原文

- 所有 251 条名句的 `content` 字段均为 `poetry.json` 对应篇目全文的**精确子串**，杜绝错字漏字
- 修复了 35 处标点与原文不一致的问题（如 `；`→`。`、`，`→`。`）
- 移除了 25 条在原文中找不到对应内容的错误条目：
  - 《归园田居》误收《饮酒》诗句"采菊东篱下，悠然见南山"
  - 文字差异：`帖`/`贴`、`受`/`益`、`锁哪`/`唢呐`、`吴越`/`胡越` 等
  - 《论语十二章》原文选章不一致
  - 《离骚》只覆盖了前半段，后段名句缺失

### 1.3 名句选取策略

新增条目聚焦于**高考默写高频考点**，优先收录历年真题中出现过的名句和新教材必背篇目的核心句子。

---

## 二、新增特性

### 2.1 易错字数据库

新增 `Assets/typo_prone_chars.json`，收录 **22 组高考高频易错字**，每组包含正字、常见误写字、原文上下文和辨析注释：

| 正字 | 常见误写 | 篇目 | 辨析 |
|------|----------|------|------|
| 贴 | 帖 | 菩萨蛮 | 贴饰 vs 字帖 |
| 唢 | 锁 | 朝天子·咏喇叭 | 唢呐：乐器名 |
| 益 | 受 | 五代史伶官传序 | 谦得益：得到益处 |
| 采 | 彩 | 登泰山记 | 原文作"采"非"彩" |
| 赢 | 嬴 | 永遇乐·京口北固亭怀古 | 赢得 vs 嬴政 |
| 酹 | 捋 | 念奴娇·赤壁怀古 | 酹(lèi)：洒酒祭奠 |
| 蜉 | 浮 | 赤壁赋 | 蜉蝣(fú yóu) |
| 衿 | 襟 | 短歌行 | 衿：衣领 |
| 砌 | 彻 | 虞美人 | 玉砌：玉石台阶 |
| 藉 | 籍 | 赤壁赋 | 枕藉：互相枕着 |
| ... | ... | ... | 共 22 组，详见文件 |

> 后续版本计划在 UI 中对这些字做高亮标注。

### 2.2 数据校验脚本

新增 `validate_data.py`，每次构建自动运行，校验项包括：

- `divided.json` 每条 content 都是 `poetry.json` 原文的精确子串
- 60 篇全部覆盖，每篇 ≥4 句
- 无重复条目、无非法 book 值
- 无孤立/多余篇目

```bash
python validate_data.py              # 标准校验
python validate_data.py --ci         # CI 模式（失败→非零退出码）
python download_data.py --validate   # 下载后自动校验
```

### 2.3 构建时自动校验

`GaokaoPoetry.csproj` 添加了 `BeforeBuild` target，每次 `dotnet build -c Release` 自动运行数据校验。数据不一致时输出 MSBuild Warning，防止错误数据被打包发布。

---

## 三、Bug 修复

### 3.1 修复数据加载竞态条件

**问题**：嵌入资源加载失败后启动网络回退，但定时器同时开始空转，期间始终显示"正在加载..."且无法恢复。

**修复**：`OnLoaded` 改为异步方法，等数据就绪后才启动定时器和首次刷新，避免空转。

### 3.2 异常不再静默吞没

**问题**：`PoetryService` 中 `catch { }` 空代码块，加载失败时用户永远只看到"正在加载..."，无法排查原因。

**修复**：新增 `LastError` 属性区分"嵌入资源缺失""网络超时""JSON 格式错误"等场景，UI 显示具体错误信息。

---

## 四、代码优化

- `PoetryService` 注册到 DI 容器（`AddSingleton`），通过构造函数注入
- `PoetryComponentSettings` 用内部 `Dictionary<string, bool>` 统一管理 6 个册别布尔属性，消除重复代码
- `download_data.py` 新增 `--validate` 参数和更完善的日志输出

---

## 五、安装 & 升级

### 新安装
1. 下载 `GaokaoPoetryPlugin.cipx`
2. 拖入 ClassIsland 插件目录

### 从 v1.1.0.0 升级
直接覆盖安装即可，设置会自动保留，新版数据向后兼容。

---

## 六、数据来源 & 许可

- 古诗文数据来自 [gaokao-poetry](https://clover-yan.github.io/gaokao-poetry/) by Clover Yan（CC BY-SA 4.0）
- 插件代码基于 MIT License

---

## 完整文件清单

```
GaokaoPoetryPlugin/
├── Assets/
│   ├── divided.json              # 251 条名句（嵌入资源）
│   └── typo_prone_chars.json     # 22 组易错字（嵌入资源）★新增
├── Components/
│   ├── PoetryComponent.cs        # 主组件（async 加载 + DI 注入）
│   ├── PoetryComponent.xaml      # UI 布局
│   ├── PoetryComponentSettings.cs # 设置类
│   ├── PoetryComponentSettingsControl.xaml
│   └── PoetryComponentSettingsControl.xaml.cs
├── Models/
│   └── PoetryItem.cs             # 数据模型
├── Services/
│   └── PoetryService.cs          # 数据服务（LastError 错误报告）
├── Plugin.cs                     # 插件入口（DI 注册）
├── manifest.yml                  # 插件清单 v1.2.0.0
├── GaokaoPoetry.csproj           # 项目文件（BeforeBuild 自动校验）
├── validate_data.py              # 数据校验脚本 ★新增
├── download_data.py              # 数据下载脚本
├── poetry.json                   # 60 篇古诗文全文（开发参考）
├── README.md
├── CHANGELOG.md
├── RELEASE_NOTES.md
└── icon.png
```
