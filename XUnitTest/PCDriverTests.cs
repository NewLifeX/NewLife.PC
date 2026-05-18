using NewLife;
using NewLife.IoT.Drivers;
using NewLife.IoT.ThingModels;
using NewLife.IoT.ThingSpecification;
using NewLife.PC.Drivers;

namespace XUnitTest;

[TestCaseOrderer("NewLife.UnitTest.DefaultOrderer", "NewLife.UnitTest")]
public class PCDriverTests
{
    PCDriver _driver;
    INode _node;
    ThingSpec _spec;
    public PCDriverTests()
    {
        _driver = new PCDriver();
    }

    [Fact]
    public void GetSpecificationTest()
    {
        _spec = _driver.GetSpecification();
        Assert.NotNull(_spec);

        Assert.NotNull(_spec.Profile);
        Assert.Equal("PC", _spec.Profile.ProductKey);
        Assert.NotEmpty(_spec.Profile.Version);

        var ps = _spec.Properties;
        Assert.NotNull(ps);
        Assert.Equal(7, ps.Length);

        Assert.NotNull(_spec.Services);
        Assert.Null(_spec.Events);
        //Assert.Null(_spec.ExtendedProperties);
    }

    [Fact]
    public void OpenTest()
    {
        _node = _driver.Open(null, (IDriverParameter?)null);
        Assert.NotNull(_node);
    }

    [Fact]
    public void ReadTest()
    {
        var spec = _driver.GetSpecification();
        var points = spec.Properties.Select(e => new PointModel { Name = e.Id }).ToArray();
        var rs = _driver.Read(_node, points);

        Assert.NotNull(rs);
        Assert.True(rs.IsSuccess);
        Assert.Equal(points.Length, rs.Points.Length);

        var cpuRate = rs.GetValue("CpuRate");
        var memory = rs.GetValue("Memory");
        var availableMemory = rs.GetValue("AvailableMemory");
        Assert.True(cpuRate.ToDouble() >= 0);
        Assert.True(memory.ToLong() > 0);
        Assert.True(availableMemory.ToLong() > 0);
    }

    [Fact]
    public void ControlTest()
    {
        var req = ControlRequest.Create("Speak", new Dictionary<String, Object?> { ["text"] = "好好学习" });
        var rs = _driver.Control(_node, req);
        Assert.NotNull(rs);

        Thread.Sleep(1000);
    }

    [Fact]
    public void ControlTest2()
    {
        var req = ControlRequest.Create("Reboot", new Dictionary<String, Object?> { ["timeout"] = "1213" });
        Assert.Throws<NotSupportedException>(() => _driver.Control(_node, req));

        PCDriver.EnableReboot = true;
    }

    [Fact]
    public void ControlTest3()
    {
        var req3 = ControlRequest.Create("abcd");
        Assert.Throws<NotImplementedException>(() => _driver.Control(_node, req3));
    }

    [Fact]
    public void SpeakTest()
    {
        _driver.Speak("学无先后达者为师");
        Thread.Sleep(3000);
    }

    [Fact(Skip = "跳过")]
    public void RebootTest()
    {
        var rs = _driver.Reboot(60);
        Assert.True(rs > 0);

        Thread.Sleep(1000);

        "shutdown".ShellExecute("-a");
    }
}