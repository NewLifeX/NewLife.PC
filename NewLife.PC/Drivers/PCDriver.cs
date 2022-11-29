using System.ComponentModel;
using System.Net.NetworkInformation;
using NewLife.IoT.Drivers;
using NewLife.IoT.ThingModels;
using NewLife.IoT.ThingSpecification;
using NewLife.NetPing.Drivers;

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
    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="node">节点对象，可存储站号等信息，仅驱动自己识别</param>
    /// <param name="points">点位集合，Address属性地址示例：D100、C100、W100、H100</param>
    /// <returns></returns>
    public override IDictionary<String, Object> Read(INode node, IPoint[] points)
    {
        var dic = new Dictionary<String, Object>();

        if (points == null || points.Length == 0) return dic;

        var p = node.Parameter as PCParameter;
        foreach (var point in points)
            if (!point.Address.IsNullOrEmpty())
                try
                {
                    var reply = new Ping().Send(point.Address, p.Timeout);
                    if (reply.Status == IPStatus.Success)
                        dic[point.Name] = reply.RoundtripTime;
                    if (p.RetrieveStatus)
                        dic[point.Name + "-Status"] = reply.Status + "";
                }
                catch (Exception ex)
                {
                    dic[point.Name + "-Status"] = ex.GetTrue().Message;
                }

        return dic;
    }

    /// <summary>发现本地节点</summary>
    /// <returns></returns>
    public override ThingSpec GetSpecification()
    {
        var spec = new ThingSpec();
        var points = new List<PropertySpec>();

        // 所有网关地址和DNS地址
        var gaddrs = new List<String>();
        var daddrs = new List<String>();
        var gi = 1;
        var di = 1;
        foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
        {
            var ipps = item.GetIPProperties();
            foreach (var elm in ipps.GatewayAddresses)
            {
                var ip = elm.Address + "";
                if (!gaddrs.Contains(ip))
                {
                    var name = "Gateway";
                    if (gi > 1) name += gi++;
                    var ps = Create(name, $"{item.Name}网关", "int", 0, ip);
                    ps.DataType.Specs = new DataSpecs { Unit = "ms", UnitName = "毫秒" };
                    points.Add(ps);
                    gaddrs.Add(ip);
                }
            }
            foreach (var elm in ipps.DnsAddresses)
            {
                if (!elm.IsIPv4()) continue;

                var ip = elm + "";
                if (!daddrs.Contains(ip))
                {
                    var name = "Dns";
                    if (di > 1) name += di++;
                    var ps = Create(name, $"{item.Name}DNS", "int", 0, ip);
                    ps.DataType.Specs = new DataSpecs { Unit = "ms", UnitName = "毫秒" };
                    points.Add(ps);
                    daddrs.Add(ip);
                }
            }
        }

        spec.Properties = points.ToArray();

        return spec;
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
    #endregion
}