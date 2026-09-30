# 本地修复验证

基线：fengymi/emby-plugin-danmu 79d6ae2f4ec8cc5d002a76a2334a0dc36b5776e0。
输出版本：1.1.3.2，目标netstandard2.0，SDK8.0.425。

问题证据：搜索直接覆盖Episode.Name；新增季搜索覆盖Season.Name；季批量更新对GetEpisodes返回对象执行MetadataEdit保存；批量下载按列表下标而不是实际集号匹配来源。
这些问题已修复。节目历史错误NFO的全部因果链未获得日志证实。

下载ID改为独立文件缓存，取消数据库/NFO写回；搜索使用独立对象；集号匹配函数由批量和单集路径共用；计划任务兼容独立缓存。

构建：Release成功，0错误，上游138警告。
回归：tests/RegressionTests.csproj，12检查全部通过。
测试使用真实MediaBrowser类型及生产缓存、搜索代理、集号匹配代码。
覆盖：元数据隔离、编号保持、缺集/特殊/越界集、TVDB不变、持久化、旧ID读取、优先级、并发和原子写入。
首次缓存原子替换测试受Windows沙箱权限限制失败，沙箱外同一测试通过。

NAS实装：Emby4.10.0.40加载1.1.3.2成功，反射类型加载错误消失。修复中文搜索zh/zho兼容；电视库勾选Danmu后，腾讯搜索返回三季结果。第三季第一集日志2026-09-30 11:58:26 UTC报告下载成功、任务完成，页面仍显示S3:E1 - 第1集。未逐文件对比NFO哈希；未验证全库批量任务和其他来源。弹弹play缺密钥、芒果来源查询异常仍存在，不影响本次腾讯下载。
打包：scripts/package.ps1实测成功，ILRepack内置Protobuf，无Google.Protobuf外部程序集引用；合并后的DLL通过12项回归检查。
原DLL备份：/volume1/docker/emby-nfo-backups/Emby.Plugin.Danmu.dll。
弹弹play密钥未公开，不包含在本构建中。第三方集列表准确性不在本修复保证范围。
