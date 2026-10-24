# NewLife.PC 版本更新记录

## v1.0.2026.1024 (2026-10-24)

### 依赖升级
- **NewLife.IoT 3.2**：标准库升级到 3.2.2026.1024，驱动沿用异步 `ReadAsync`/`ControlAsync` 与结构化 `ReadResult`/`ControlResult`
- **NewLife.Core 12.0**：依赖链升级到 12.0.2026.1024，机器信息采集统一走 `MachineInfo`

### 工程与协作
- **解决方案格式**：`NewLife.PC.sln` 升级为 `NewLife.PC.slnx`，需要 VS 2022 17.10+ 或 .NET 9+ SDK
- **持续集成**：发布流程同时推送到 GitHub Packages 与 NuGet.org，并按版本标签自动创建 GitHub Release
- **文档**：Readme 生态版图与团队介绍对齐新生命主仓口径

---

## v1.0.2026.0609 (2026-06-09)

### 驱动重构
- **异步接口**：`ReadAsync`/`ControlAsync` 替代同步接口，返回 `ReadResult`/`ControlResult` 结构化结果
- **物模型**：`GetSpecification` 输出 `ThingSpec`，包含 CPU、内存、可用内存、上行速度、下行速度、温度、电量等只读属性，以及语音播报、重启两个服务
- **依赖升级**：NewLife.IoT 升级到 3.0.2026.601

---
