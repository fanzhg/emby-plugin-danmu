# 1.1.3.2：弹幕下载元数据保护修复

下载弹幕时可能出现季名称变成节目全名、剧集元数据异常，以及缺集后弹幕匹配错位。本次修复使用独立搜索对象，并将新弹幕源 ID 存入插件缓存，停止通过 `UpdateToRepository` 写回 Emby 元数据；批量匹配按实际集号进行。

- 修复内容、安装与回滚：[详细说明](doc/metadata-safety-fix.md)
- 验证结果与边界：[verification.md](verification.md)
- 合并依赖打包：`pwsh ./scripts/package.ps1`（请安装 artifacts 中的 DLL）
- 回归测试：`dotnet run --project tests/RegressionTests.csproj -c Release`

Release 编译通过，12 项回归检查通过。已在群晖 Docker / Emby 4.10.0.40 验证加载、中文搜索与腾讯单集后台下载成功；本修复不自动修复已有错误 NFO，也不保证第三方节目集目录完全准确。公开源码构建不含弹弹play私有密钥，需要自行配置 `DANDAN_API_ID` / `DANDAN_API_SECRET`。

## 上游项目说明与历史下载（不包含本次修复）

# 因为本人自己不再使用EMBY, 不同环境自己调试和解决兼容问题
### 请喝咖啡
<img src="./doc/image/支付宝.jpg"  width="100px" />

## 2025-04-23更新
1. 修复爱奇艺弹幕下载问题
2. 支持更新和扫描任务


## 2024-07-26更新
1. 支持弹幕搜索
2. 支持新增媒体自动下载

## 支持弹幕网站
目前只支持以下站点，后续有空再更新
```
弹弹play、爱奇艺、腾讯、优酷
```
设置方式一样和jellyfin一样 [参考](https://github.com/cxfksword/jellyfin-plugin-danmu) ，但是api接口基本没有支持



## 支持的api操作
```shell
## 刷新某个媒体的弹幕
/api/danmu/{媒体id}?option=Refresh&X-Emby-Token=xxx
## 获取媒体对应json格式弹幕
/api/danmu/{媒体id}?option=GetJsonById&X-Emby-Token=xxx

```

## 下载链接
[Emby.Plugin.Danmu.dll](https://github.com/fengymi/emby-plugin-danmu/releases/tag/1.0.0) <p/>
[Emby.AndroidTV.2.1.14.弹幕.apk](https://github.com/fengymi/emby-plugin-danmu/releases/tag/1.0.0)

## 遗留问题
1. 配置暂不支持顺序调整

### 预览

<img src="./doc/image/设置页面.jpg"  width="720px" />
<img src="./doc/image/tv-danmu.jpg"  width="720px" />

<img src="./doc/image/弹幕搜索.png"  width="720px" />
