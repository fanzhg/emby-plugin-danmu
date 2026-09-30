# Danmu 1.1.3.2 本地修复版

基于 https://github.com/fengymi/emby-plugin-danmu 提交
79d6ae2f4ec8cc5d002a76a2334a0dc36b5776e0。不是官方发行版，也未确认与旧 1.1.3.0 二进制的源码完全一致。

## 修复内容

- 搜索节目名时使用独立的 Episode/Season 对象，不修改媒体库对象的 Name 或 ProductionYear。
- 下载、匹配及强制下载只把弹幕源 ID 保存到插件独立缓存，取消所有 Emby UpdateToRepository 调用，不触发整份元数据/NFO写回。
- 新 ID 存放在 Emby DataPath 下的 danmu-provider-cache，当前容器通常对应 /config/data/danmu-provider-cache（实际位置由 DataPath 决定）。保留读取旧 NFO/数据库中弹幕源 ID 的兼容路径；缓存值优先。
- 缺集时按实际集号匹配来源列表；缺第8集不会让第9集使用第8集的弹幕。
- 计划任务读取插件缓存，不再只按 Emby 数据库的 ProviderId 筛选。
- 配置页面正确显示程序集版本 1.1.3.2。

## 安装

1. 在群晖 Container Manager 停止 Emby 容器。
2. 把原 /volume1/docker/emby/plugins/Emby.Plugin.Danmu.dll 复制到媒体库之外的备份目录。保留原插件配置，不必先卸载插件。
3. 将本包 Emby.Plugin.Danmu.dll 上传到 /volume1/docker/emby/plugins/，替换同名旧 DLL。只保留一份活动 DLL，旧版备份不要留在 plugins 目录中。
4. 启动 Emby 容器，在插件页确认版本为 1.1.3.2，检查启动日志有“danmu 插件加载完成”，没有程序集加载错误。
5. 先对一个已确认正确季号、集号的剧集手动下载腾讯弹幕，确认 ASS/XML 文件生成，节目名、季名、季号、集号、集数和 NFO 内容保持不变。
6. 重启一次容器，确认已匹配的弹幕 ID 可继续用于更新。单集验证通过后再恢复批量任务。

不需要执行“替换所有元数据”。本插件不会自动修复已经错误的 NFO 或改回第三季的旧名称；此前节目修复与本次预防修改是两件事。

## 回滚

停止容器，用备份 DLL 替换修复版，启动容器。插件自己的 danmu-provider-cache 可以保留，旧版本不会读取它；旧版本也无法获得仅保存于该缓存的新 ID。恢复旧版会重新引入元数据写回风险。

## 验证与限制

- Release 编译成功，12项回归检查全部通过：搜索隔离、保留编号、缺集匹配、特殊/越界集跳过、外部ID不变、缓存读取与持久化、缓存优先、旧ID兼容、并发保存、原子替换、无效ID拒绝。
- 检查插件源码，活动代码无 UpdateToRepository / UpdateToRepositoryAsync 调用。
- 已在用户 NAS 的 Emby 4.10.0.40 实装；第三季第一集中文搜索返回腾讯三季结果，选第三季后日志报告弹幕下载成功、强制任务完成，页面仍为 S3:E1。未逐文件比较 NFO 哈希，未验证全部来源或全部集。
- 上游有138个编译警告，包括可空注解、异步方法无await等上游警告；本次未扩大范围处理上游警告。
- 弹弹play 官方发布密钥未公开，本包不包含密钥。需要自行合法配置 DANDAN_API_ID / DANDAN_API_SECRET 环境变量，或关闭该来源。腾讯、Bilibili等其他来源不需要这组密钥。
- 来源列表的顺序仍需对应节目集号；综艺的上/下期或加更可能需要手动选择正确来源。本修复防止弹幕流程写坏元数据，不能保证第三方所有集目录都准确。
- 新弹幕ID不再写入 Emby 外部ID编辑界面；更换匹配请使用插件的手动弹幕搜索/下载。

DLL SHA256:
8C8A391E416276CBB4A19891D0A8F833CFDE0F46295BFBFE7B5F7B89EF030BFE

## NAS 实装补充（2026-09-30）

已在群晖 Docker / Emby 4.10.0.40 加载 1.1.3.2。使用 ILRepack 合并 Google.Protobuf，解决 Costura 嵌入依赖在 Emby 反射扫描时的加载失败。新增 scripts/package.ps1，发布工作流使用同一打包方式。中文搜索兼容 Emby 的 zh / zho 代码。电视媒体库须勾选 Danmu 下载器。搜索已返回腾讯节目三季结果，下载任务已触发；最终文件生成结果见 verification.md。

构建单 DLL：pwsh ./scripts/package.ps1。不要直接安装未合并的 bin 目录 DLL。
