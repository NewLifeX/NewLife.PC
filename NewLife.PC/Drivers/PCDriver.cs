using System.ComponentModel;
using NewLife.IoT.Drivers;
using NewLife.IoT.ThingModels;
using NewLife.IoT.ThingSpecification;
using NewLife.Reflection;

namespace NewLife.PC.Drivers;

/// <summary>
/// IoT标准PC驱动
/// </summary>
/// <remarks>
/// IoT驱动，符合IoT标准库的PC驱动，采集CPU、内存、网络等数据，提供语音播报和重启等服务。
/// </remarks>
[Driver("PC")]
[DisplayName("PC驱动")]
public class PCDriver : DriverBase<Node, PCParameter>
{
    #region 属性
    /// <summary>是否启用重启。默认false</summary>
    public static Boolean EnableReboot { get; set; }
    #endregion

    #region 方法
    /// <summary>读取数据</summary>
    /// <param name="node">节点对象，可存储站号等信息，仅驱动自己识别</param>
    /// <param name="points">点位集合，Address属性地址示例：D100、C100、W100、H100</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>读取结果</returns>
    public override Task<ReadResult> ReadAsync(INode node, IPoint[] points, CancellationToken cancellationToken = default)
    {
        if (points == null || points.Length == 0)
            return TaskEx.FromResult(ReadResult.Success([], []));

        var mi = MachineInfo.GetCurrent();
        mi.Refresh();

        var resultPoints = new List<IPoint>();
        var resultValues = new List<Object?>();

        foreach (var pi in mi.GetType().GetProperties())
        {
            var point = points.FirstOrDefault(e => e.Name.EqualIgnoreCase(pi.Name));
            if (point != null)
            {
                resultPoints.Add(point);
                resultValues.Add(mi.GetValue(pi));
            }
        }

        return TaskEx.FromResult(ReadResult.Success([.. resultPoints], [.. resultValues]));
    }

    /// <summary>控制设备</summary>
    /// <param name="node">节点对象</param>
    /// <param name="request">控制请求，携带服务名和输入参数</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>控制结果</returns>
    public override Task<ControlResult> ControlAsync(INode node, ControlRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null || request.ServiceName.IsNullOrEmpty()) throw new NotImplementedException();

        switch (request.ServiceName)
        {
            case nameof(Speak):
                var text = request.Parameters.TryGetValue("text", out var t) ? t + "" : "";
                Speak(text);
                return TaskEx.FromResult(ControlResult.Success());
            case nameof(Reboot):
                if (!EnableReboot) throw new NotSupportedException("未启用重启功能");
                var timeout = request.Parameters.TryGetValue("timeout", out var to) ? to.ToInt() : 0;
                var pid = Reboot(timeout);
                return TaskEx.FromResult(ControlResult.Success(new Dictionary<String, Object?> { ["pid"] = pid }));
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>语音播报</summary>
    /// <param name="text"></param>
    [DisplayName("语音播报")]
    public void Speak(String text) => text.SpeakAsync();

    /// <summary>重启计算机</summary>
    /// <param name="timeout"></param>
    [DisplayName("重启计算机")]
    public Int32 Reboot(Int32 timeout)
    {
        if (Runtime.Windows)
        {
            // shutdown -r -t 123 -c "IoTEdge"
            // 需要注意，该操作有可能被火绒等安全软件拦截
            var p = "shutdown".ShellExecute($"-r -t {timeout}");
            return p?.Id ?? 0;
        }
        else if (Runtime.Linux)
        {
            var p = "reboot".ShellExecute();
            return p?.Id ?? 0;
        }

        return -1;
    }

    /// <summary>填充产品物模型</summary>
    /// <param name="spec">待填充的物模型对象</param>
    /// <returns>是否填充成功</returns>
    protected override Boolean OnGetSpecification(ThingSpec spec)
    {
        var points = new List<PropertySpec>();
        var services = new List<ServiceSpec>();

        var pis = typeof(MachineInfo).GetProperties();

        points.Add(PropertySpec.Create(pis.FirstOrDefault(e => e.Name == "CpuRate")));
        points.Add(PropertySpec.Create(pis.FirstOrDefault(e => e.Name == "Memory")));
        points.Add(PropertySpec.Create(pis.FirstOrDefault(e => e.Name == "AvailableMemory")));
        points.Add(PropertySpec.Create(pis.FirstOrDefault(e => e.Name == "UplinkSpeed")));
        points.Add(PropertySpec.Create(pis.FirstOrDefault(e => e.Name == "DownlinkSpeed")));
        points.Add(PropertySpec.Create(pis.FirstOrDefault(e => e.Name == "Temperature")));
        points.Add(PropertySpec.Create(pis.FirstOrDefault(e => e.Name == "Battery")));
        spec.Properties = points.Where(e => e != null).ToArray();

        // 只读
        foreach (var item in spec.Properties)
        {
            item.AccessMode = "r";
        }

        services.Add(ServiceSpec.Create(Speak));
        services.Add(ServiceSpec.Create(Reboot));
        spec.Services = services.Where(e => e != null).ToArray();

        return true;
    }
    #endregion
}