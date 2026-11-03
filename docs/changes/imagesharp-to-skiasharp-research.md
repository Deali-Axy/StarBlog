# ImageSharp → SkiaSharp 替换研究

研究日期：2026-11-03。范围：当前 StarBlog API、相关离线工具，以及相邻 `starblog-publisher` 项目的现有实现。最新进展：用户选定 EasyImageSharp，API 已切换到 1.2.0，ImageSharp.Web 已移除。Release build/publish 与媒体回归通过；实施结果见文末。前文保留各候选调研证据，不代表当前仍使用 SkiaSharp 或等待选型。

## 结论

当前 API 的图片处理需求较小，最终采用纯托管 EasyImageSharp 1.2.0 覆盖摄影上传、等比例缩放、JPEG 编码和尺寸读取，无需添加原生图像库。ImageSharp.Web 已移除，未引入替代 Web 中间件。

这不是只改 NuGet 包名：需要适配图片处理代码、确认解码格式范围、显式处理方向和透明度，并验证正式 Linux 容器。现有图片路径、摄影 API、数据库结构可以保留，不需要批量重编码已有媒体。

## 原 ImageSharp 构建阻塞（已由替换核心库解除）

执行：

```sh
dotnet build apps/api/src/StarBlog.Api/StarBlog.Api.csproj -c Release --no-restore --nologo -v quiet
```

失败：`SixLabors.ImageSharp.targets(35,5): error : No Six Labors license found`。

已检查本机 ImageSharp 3.2.0 的构建 targets：许可检查发生在 `CoreCompile` 前，`ContinueOnError` 取决于配置名是否以 `Debug` 开头。因此 Debug 成功不代表可正式发布。[官方 3.2.0 发布说明](https://github.com/SixLabors/ImageSharp/releases/tag/v3.2.0)也明确列入了许可强制检查。

## 迁移范围

| 位置 | 现有用途 | 建议 |
| --- | --- | --- |
| `apps/api/src/StarBlog.Api/StarBlog.Api.csproj` | ImageSharp 3.2.0、ImageSharp.Web 3.2.0 | 移除两个直接依赖，添加同版本的 SkiaSharp 与 Linux 原生库包 |
| `apps/api/src/StarBlog.Api/Modules/Media/MediaOperations.cs` | 上传转 JPEG、最长边 1500、缩略图、写入后读取尺寸 | 抽取一个小型图片处理服务，使用 SkiaSharp；编码时返回尺寸，避免写入后再次打开图片 |
| `apps/api/src/StarBlog.Api/Hosting/WebFrameworkExtensions.cs` | `AddImageSharp` / `UseImageSharp` | 已按用户决定删除，同时移除 ImageSharp.Web 包引用 |
| `apps/web/lib/media.ts` | `/api/v1/photos/{id}/thumbnail?width=...` 与原图路径 | 保持现有 URL 和响应契约 |
| `tools/BlogImageOptimizer` | ImageSharp 3.1.11；独立批量优化程序 | 第二阶段迁移；它在 `StarBlog.slnx` 内，API 完成后这里仍会保留 ImageSharp |
| `tools/DataProc` | ImageSharp 3.1.11；历史数据处理/优化 | 第二阶段单独评估；不在当前 solution 中，且仍引用当前源码目录中不存在的旧模块工程 |
| `README.md`、`apps/web/lib/about.ts`、部署文档 | 技术栈与发布说明 | 实际迁移完成并验证后更新；不能提前把发布阻塞标记为解决 |

当前缩略图边界已限制 `width=1..2000`、`quality=1..100`，默认宽 300、质量 85。现有代码允许将小图放大到请求宽度；前端 `srcSet` 使用 320、480、640、800、1200、1600、2000。替换时应保留这些行为，避免改变响应尺寸和前端声明的对应关系。

ImageSharp.Web 不负责这里的缩略图生成；该工作由 `MediaOperations.GetThumbAsync` 直接执行。移除中间件不能等同于给缩略图增加或移除缓存。用户已明确决定移除，本次已执行；不再将历史动态缩图链接核实作为移除的前置条件。

## publisher 中可以借鉴的内容

`../starblog-publisher/StarBlogPublisher.Core/StarBlogPublisher.Core.csproj` 当前使用：

```xml
<PackageReference Include="SkiaSharp" Version="3.119.4" />
<PackageReference Include="SkiaSharp.NativeAssets.Linux" Version="3.119.4" />
```

`Services/Imaging/DecodedImage.cs` 已集中封装 `SKCodec` 解码、八种 EXIF 方向变换、`SKSamplingOptions(SKCubicResampler.Mitchell)` 缩放、编码和资源释放。`ImageCompressionService.cs` 使用一次计算的比例约束宽高，并在文章压缩时保留动态 GIF 原文件。这些处理方式可用于服务端。

不建议为复用图片处理而让 API 引用整个 publisher Core。可移植必要的小型封装；以后多个项目确有共同维护需求时再考虑共享库。

需要按服务端场景调整：

- publisher 解码允许 `SKCodecResult.IncompleteInput`；服务端上传建议明确是否拒绝截断图片，不能无意沿用桌面端容错策略。
- 当前 API 没有调用 ImageSharp 的 `AutoOrient`。采用 publisher 的 EXIF 归一化会改变部分新上传照片的方向与宽高，属于需要明确接受并测试的行为改进；不自动重写已有照片。
- publisher 创建 RGBA 像素缓冲时没有显式指定输出色彩空间。摄影服务应验证带 ICC 的照片，明确转为 sRGB 的路径；不能假设直接复制封装就保留了全部色彩/元数据。
- JPEG 不支持透明度，需要在不透明画布上明确合成背景。建议白底；直接编码透明像素在本次测试中得到黑底。
- Skia 编解码主要为同步原生操作。保留异步文件 I/O 和取消检查，不要把 `Task.CompletedTask` 当作异步编解码，也不应为每张图无界启动 `Task.Run`。
- 分配像素缓冲前限制输入字节数、像素数和并发；长宽有效不等于内存占用可接受。所有 bitmap、codec、image、data 应确定释放。

## 版本、许可与部署

截至研究日期，官方已发布稳定版 [SkiaSharp 4.154.0](https://github.com/mono/SkiaSharp/releases/tag/v4.154.0)；publisher 的 3.119.4 是已有项目的参考版本，不应误称为最新版本。建议新迁移优先评估并固定 4.154.0，托管包与原生包保持相同版本；本次临时实验使用的正是这一组合。

SkiaSharp 使用 [MIT 许可证](https://github.com/mono/SkiaSharp/blob/main/LICENSE.md)，发布时仍需保留许可和原生包随附的第三方声明。本次独立 Release 编译/运行不需要 Six Labors 许可。

API 只做图片处理，无字体发现需求，建议使用 [SkiaSharp.NativeAssets.Linux.NoDependencies 4.154.0](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux.NoDependencies/4.154.0)。实查 publisher 所用常规 Linux 3.119.4 原生库依赖 `libfontconfig.so.1`；NoDependencies 4.154.0 的 `ldd` 列表无 fontconfig，但仍包含 libc、libstdc++、libgcc 等系统运行库。它不是完全不依赖操作系统的纯托管库。

当前 Dockerfile 使用 `mcr.microsoft.com/dotnet/aspnet:10.0`，CI 只在 Windows 上跑 Release 测试。迁移验收必须补 Linux 容器内实际图片请求，避免只通过 Windows 构建。本次没有运行目标 ASP.NET 镜像，因此尚未证明容器部署完成。

官方 v4 发布说明包含原生依赖更新和破坏性变更；[4.154 发布说明](https://mono.github.io/SkiaSharp/docs/releases/4.154.0.html)还说明禁用了 RAW/DNG 解码。不应承诺它与 ImageSharp 的所有输入格式、元数据或编码质量逐项等价，也不能仅凭 NuGet 没有告警推断所有内嵌原生库都没有漏洞。

## 临时验证结果

在 `/tmp/starblog-skia-research` 创建独立 `net10.0` 程序，使用 `SkiaSharp` 和 `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.154.0。直接链接 publisher 的 `DecodedImage.cs` 与 `GifEncoder.cs`，仅为未使用的封面业务类型提供最小编译桩；没有编译整个 publisher，也没有修改其源码。宿主为 Debian 13 / linux-x64，SDK 10.0.400。

执行 `dotnet run --project /tmp/starblog-skia-research/SkiaProbe.csproj -c Release --no-restore --nologo`：

| 检查 | 结果 |
| --- | --- |
| JPEG、PNG、WebP 解码，缩放后编码 JPEG 并再次读取尺寸 | 通过 |
| 单帧 GIF 解码 | 通过 |
| 八种 EXIF 方向变换，使用非正方形像素图逐像素核对 | 全部通过 |
| 实际带 EXIF Orientation=6 的 JPEG 经封装解码 | 4×2 正确变为 2×4 |
| 3000×2000 上传样例最长边压缩 | 正确输出 1500×1000 |
| 现有 API 测试的 1×1 PNG 样例 | 可解码 |
| 非图片输入 | 封装抛出解码异常 |
| 全透明 PNG 直接编码 JPEG | 得到黑底；显式白底合成后通过检查 |
| 最小无压缩 RGB TIFF | `SKCodec.Create` 返回 null，不能沿用“任意可解码图片”这一宽泛描述 |
| 原生 GIF 编码 | 返回 null；publisher 自定义 GIF 编码器用于单帧输出 |
| 原封装在 v4 的编译情况 | 成功；`DrawBitmap` 无采样参数的重载出现 obsolete 警告，移植时需更新 |

这些结果证明核心处理链路可行，不是完整 API 回归或容器验收。尚未验证 ICC/CMYK、动画保留、损坏图片全集、并发性能、内存峰值及其他 CPU 架构。没有据此作出性能优于 ImageSharp 的结论。

## 建议实施顺序与验收

1. API 引入独立图片处理服务及同版本 Skia 包，替换摄影上传、缩略图和尺寸读取；保留存储失败时的文件补偿逻辑。上传统一 JPEG、最长边 1500，明确编码质量、背景色和 EXIF 处理。
2. ImageSharp.Web 已移除；选定核心处理库后再移除 ImageSharp 依赖。现有媒体目录保留，不批量改写已有文件。
3. 扩充媒体测试：真实 JPEG 输出/尺寸、横竖图、EXIF 旋转和镜像、透明 PNG、GIF 首帧、无效/超大输入、缩略图宽度和质量参数、存储/数据库失败补偿。补充摄影页面、灯箱和原图加载的回归。
4. 执行 API Release build/test/publish、solution Release 测试、目标 Linux Docker 构建，并在最终运行容器内上传及请求缩略图。依赖清单与漏洞审计检查直接和传递引用。通过前保留现有上线阻塞记录。
5. 第二阶段处理 BlogImageOptimizer 和历史 DataProc：明确动态 GIF 保留策略、输出格式和透明度语义后迁移，不能将单帧 GIF 自定义编码视为动画编码的替代。最终清理整个仓库的 SixLabors 引用并更新技术栈文案。

## 补充：Native 成本与纯 C# 路线

SkiaSharp 的 Native 依赖对当前 Linux 容器部署预计可控，但会增加一项需要随运行平台发布和验证的库：本次下载的 NoDependencies 4.154.0 的 linux-x64 `libSkiaSharp.so` 为 12,233,016 字节（约 12 MB，非整个发布包增量）。更换 CPU 架构或 glibc/musl 基础镜像时，必须选择对应资产并实测加载；NoDependencies 仍依赖系统 C/C++ 运行库。原生像素缓冲也需要确定释放，其占用不能只靠托管 GC 堆指标判断。

StarBlog 当前也不是完全没有原生库：API 的 EF Core SQLite 依赖已经引入 `SQLitePCLRaw.lib.e_sqlite3`，恢复资产中包含各平台 `libe_sqlite3.so`。这不消除 SkiaSharp 的新增成本，只说明 Native 运行时资产并非项目第一次遇到的问题。

在线业务中，ImageSharp 使用仍集中在摄影模块的解码、等比例缩放、JPEG 编码及尺寸读取。`ContentOperations.UploadImageAsync` 只是原样保存文章图片，不经过 ImageSharp；文章中的 WebP 原样保存与浏览器展示也不等同于服务端需要解码 WebP。离线优化工具另有 WebP/GIF 编码需求，应单独决策。

纯 C# 替换在限定需求下可行，候选是 stb 的 C# 移植系列：

- [StbImageSharp](https://github.com/StbSharp/StbImageSharp) 与 [StbImageWriteSharp](https://github.com/StbSharp/StbImageWriteSharp) 明确是 C# 移植而非 Native 包装。两个项目目前都推荐后继的 [StbImageNext](https://github.com/StbSharp/StbImageNext)。
- StbImageNext 文档提供 JPEG/PNG 编解码、GIF 解码、尺寸读取，许可为 Public Domain 或 MIT。其官方支持列表没有 WebP/TIFF，不能作为保持所有现有摄影输入格式的直接替代。
- 缩放还需配套纯 C# 实现；[StbImageResizeSharp](https://github.com/StbSharp/StbImageResizeSharp) 是现成候选，文档列为 Public Domain。EXIF、透明度合成、色彩处理和质量验收不能因“纯 C#”而省略。

以上 stb 路线本次仅完成官方文档与工程文件核查，没有完成包版本选择、图片实测或性能验证。也不建议自行重写 JPEG/PNG 解码器。[System.Drawing.Common](https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/system-drawing-common-windows-only) 是系统图形技术的包装且仅支持 Windows，不能解决当前 Linux 下的纯托管需求。

这一轮仅比较 stb 与 SkiaSharp。后续发现的完整纯托管库不具有相同的 WebP 限制，最新候选排序见下节。

## 补充：更现代的完整纯托管候选

| 候选 | 核心目标框架 / 许可 | 与 StarBlog 的关系 |
| --- | --- | --- |
| [EasyImageSharp](https://github.com/FarhanLodi/EasyImageSharp) | net8.0、net10.0 / MIT | `Image.Load`、`Mutate`、`Resize`、`SaveAsJpeg` 等 API 接近现有代码；核心提供完整编解码与处理管道，无需组合 stb 组件 |
| [OneImage / RecursiveAI.OneImage](https://github.com/recursive-ai-oss/oneimage-csharp) | net10.0 / MIT OR Apache-2.0 | 提供延迟求值、分块处理与缩略图入口；官方文档列出 ICC、EXIF 处理，值得作为摄影服务候选 |
| [Echostorm.SharpImage](https://github.com/Echostorm44/SharpImage) | net10.0 / Apache-2.0 | 声明提供纯托管 ImageMagick 风格处理及多种格式；本次只核查官方说明与 NuGet 元数据，未进行运行验证，列为备选 |

三个项目的官方说明都列有 JPEG、PNG、WebP 支持，核心 NuGet 页面均列为无包依赖。这里的纯托管是无需额外随应用部署 Skia、libwebp 等原生图像库；不意味着 .NET 运行时自身不调用系统原生代码。EasyImageSharp 的 ONNX、Skia、LittleCMS 等扩展是另一回事，本项目只需核心包。

### 本轮实际验证

临时 `net10.0` 项目：`/tmp/starblog-managed-imaging-research`。下载并核查的实际 NuGet 包：

- `EasyImageSharp` **1.2.0**：nuspec 为 MIT，net8.0/net10.0 两个依赖组为空。
- `RecursiveAI.OneImage` **1.10.4**：nuspec 为 MIT OR Apache-2.0，net10.0 依赖组为空。

独立 Release 程序读取由 SkiaSharp 生成的 1800×1200 非对称色块图片，分别以 JPEG、PNG、WebP 作为输入；每个候选等比例缩为 1500×1000，并以显式质量 85 输出 JPEG。六个组合全部成功，输出可以再次解码且尺寸正确。临时程序未向 StarBlog 添加任何候选依赖。

这证明可发布包的基础链路能够工作，不能外推为所有 JPEG/WebP 变体、摄影色彩和畸形输入均兼容；两者的相同 quality 数值也不代表相同文件大小或视觉质量。官方页面和 README 的版本描述存在缓存或更新不同步，本轮结论以实际下载包为准，不依赖尚未发布的主分支功能。

### 建议

按当前迁移成本，**优先验证 EasyImageSharp，OneImage 作为并行比较候选**。前者 API 接近当前代码；后者的摄影色彩和按需解码设计值得进一步检验。两个项目的公开生态与使用积累较小，不能因为目标框架新或功能清单长就视为与 ImageSharp 同等成熟。候选测试应覆盖实际摄影样本、ICC/CMYK、EXIF、透明度、渐进 JPEG、有损/无损 WebP、错误输入和负载；本次没有验证作者发布的性能对比结论。

ImageSharp.Web 删除后的验证：恢复 API 测试项目依赖、Debug 编译成功，现有 `MediaFeatureTests` 两项全部通过。ImageSharp 核心仍产生 Debug 许可警告，Release 许可阻塞尚未解决。

## EasyImageSharp 实施结果

API 已固定使用 `EasyImageSharp` 1.2.0，并移除全部 SixLabors 包引用与宿主中间件注册。旧离线工具仍保留原 ImageSharp 版本，本次没有改动其批量压缩流程。README 已同步说明在线 API 与旧离线工具使用不同图像库。

摄影上传与缩略图现统一解码为 RGBA，按 EXIF 归一化方向，透明部分合成白底，再输出 4:2:0 JPEG。上传质量显式为 75；缩略图沿用请求中的 quality（默认 85）、宽度范围与允许放大的现有行为。上传最长边 1500，小图不放大，极宽/极高图片至少保留一个像素。尺寸直接从处理结果写入记录，不再为读取宽高重新打开已保存文件。文件写入失败时的补偿逻辑保留。

解码上限：编码文件 32 MiB、单帧 5000 万像素、总帧像素合计 1 亿；无效图片或超过上限的上传返回 400，并在写入文件/数据库前失败。动画输入输出 JPEG 首帧。已有媒体文件和数据库结构无需变更。

验证结果：

- API Release build：成功，0 warning / 0 error。
- API Release publish：成功，输出 `/tmp/starblog-easyimagesharp-publish`；`StarBlog.Api.deps.json` 的图像依赖只有 `EasyImageSharp/1.2.0`，不含 SixLabors、SkiaSharp 或新的 Native 图像库。
- `MediaFeatureTests`：**15 / 15** 通过。包含 JPEG/PNG/WebP、带 EXIF 的纵向照片、透明 PNG 白底、极宽图片、GIF 首帧、实际 JPEG 输出及尺寸、缩略图放大、无效输入/超大像素限制且无持久化、参数边界。
- 完整 API Release 测试：**78 通过 / 1 失败**。失败的 `Source_does_not_reference_FreeSql` 对所有源码做字面量扫描，命中未修改的 `StarBlogDatabase.cs` 中旧库迁移提示和注释；与图片库替换无关。本次未顺带改动架构测试规则。

固定回归图片位于 `apps/api/tests/StarBlog.Api.Tests/Fixtures/Media`，由独立 Skia 编码器或手工构造产生，测试工程不依赖 Skia。尚未完成真实摄影库的 ICC/CMYK 色彩对照、最终 Docker 镜像验收或负载性能验证，不据此承诺视觉质量或性能与原库逐项一致。
