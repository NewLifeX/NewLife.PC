using System.ComponentModel;
using System.Reflection;
using NewLife.IoT.Drivers;
using NewLife.IoT.ThingModels;
using NewLife.IoT.ThingSpecification;
using NewLife.Reflection;
using NewLife.Serialization;

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
    #region 方法
    /// <summary>读取数据</summary>
    /// <param name="node">节点对象，可存储站号等信息，仅驱动自己识别</param>
    /// <param name="points">点位集合，Address属性地址示例：D100、C100、W100、H100</param>
    /// <returns></returns>
    public override IDictionary<String, Object> Read(INode node, IPoint[] points)
    {
        var dic = new Dictionary<String, Object>();

        if (points == null || points.Length == 0) return dic;

        var mi = MachineInfo.GetCurrent();

        foreach (var pi in mi.GetType().GetProperties())
        {
            var point = points.FirstOrDefault(e => e.Name.EqualIgnoreCase(pi.Name));
            if (point != null)
            {
                dic[point.Name] = mi.GetValue(pi);
            }
        }

        return dic;
    }

    /// <summary>设备控制</summary>
    /// <param name="node"></param>
    /// <param name="parameters"></param>
    public override void Control(INode node, IDictionary<String, Object> parameters)
    {
        var service = JsonHelper.Convert<ServiceModel>(parameters);
        if (service == null || service.Name.IsNullOrEmpty()) throw new NotImplementedException();

        switch (service.Name)
        {
            case nameof(Speak):
                Speak(service.InputData);
                break;
            case nameof(Reboot):
                Reboot(service.InputData.ToInt());
                break;
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
    public void Reboot(Int32 timeout) => "shutdown".ShellExecute($"-r -t {timeout}");

    /// <summary>发现本地节点</summary>
    /// <returns></returns>
    public override ThingSpec GetSpecification()
    {
        var spec = new ThingSpec();
        var points = new List<PropertySpec>();
        var services = new List<ServiceSpec>();

        var pis = typeof(MachineInfo).GetProperties();

        points.Add(Create(pis.FirstOrDefault(e => e.Name == "CpuRate")));
        points.Add(Create(pis.FirstOrDefault(e => e.Name == "Memory")));
        points.Add(Create(pis.FirstOrDefault(e => e.Name == "AvailableMemory")));
        points.Add(Create(pis.FirstOrDefault(e => e.Name == "UplinkSpeed")));
        points.Add(Create(pis.FirstOrDefault(e => e.Name == "DownlinkSpeed")));
        points.Add(Create(pis.FirstOrDefault(e => e.Name == "Temperature")));
        points.Add(Create(pis.FirstOrDefault(e => e.Name == "Battery")));
        spec.Properties = points.Where(e => e != null).ToArray();

        // 只读
        foreach (var item in spec.Properties)
        {
            item.AccessMode = "r";
        }

        services.Add(Create(Speak));
        services.Add(Create(Reboot));
        spec.Services = services.Where(e => e != null).ToArray();

        return spec;
    }

    /// <summary>快速创建属性</summary>
    /// <param name="member"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static PropertySpec Create(MemberInfo member, Int32 length = 0)
    {
        if (member == null) return null;

        var ps = new PropertySpec
        {
            Id = member.Name,
            Name = member.GetDisplayName() ?? member.GetDescription(),
        };

        if (member is PropertyInfo pi)
            ps.DataType = new TypeSpec { Type = pi.PropertyType.Name };
        if (member is FieldInfo fi)
            ps.DataType = new TypeSpec { Type = fi.FieldType.Name };

        if (length > 0 && ps.DataType != null)
            ps.DataType.Specs = new DataSpecs { Length = length };

        return ps;
    }

    /// <summary>快速创建属性</summary>
    /// <param name="id">标识</param>
    /// <param name="name">名称</param>
    /// <param name="type">类型</param>
    /// <param name="length">长度</param>
    /// <param name="address">点位地址</param>
    /// <returns></returns>
    public static PropertySpec Create(String id, String name, String type, Int32 length = 0, String address = null)
    {
        var ps = new PropertySpec
        {
            Id = id,
            Name = name,
            Address = address
        };

        if (type != null)
        {
            ps.DataType = new TypeSpec { Type = type };

            if (length > 0)
                ps.DataType.Specs = new DataSpecs { Length = length };
        }

        return ps;
    }

    /// <summary>快速创建服务</summary>
    /// <param name="delegate"></param>
    /// <returns></returns>
    public static ServiceSpec Create(Delegate @delegate) => Create(@delegate.Method);

    /// <summary>快速创建服务</summary>
    /// <param name="method"></param>
    /// <returns></returns>
    public static ServiceSpec Create(MethodBase method)
    {
        if (method == null) return null;

        var ss = new ServiceSpec
        {
            Id = method.Name,
            Name = method.GetDisplayName() ?? method.GetDescription(),
        };

        var pis = method.GetParameters();
        if (pis.Length > 0)
        {
            var ps = new List<PropertySpec>();
            foreach (var pi in pis)
            {
                ps.Add(Create(pi));
            }

            ss.InputData = ps.Where(e => e != null).ToArray();
        }

        return ss;
    }

    /// <summary>快速创建属性</summary>
    /// <param name="member"></param>
    /// <returns></returns>
    public static PropertySpec Create(ParameterInfo member)
    {
        if (member == null) return null;

        var ps = new PropertySpec
        {
            Id = member.Name,
            DataType = new TypeSpec { Type = member.ParameterType.Name }
        };

        return ps;
    }
    #endregion
}