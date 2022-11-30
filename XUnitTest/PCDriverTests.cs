using System.Xml.Linq;
using NewLife;
using NewLife.IoT.Drivers;
using NewLife.PC.Drivers;

namespace XUnitTest;

public class PCDriverTests
{
    PCDriver _driver;
    INode _node;
    public PCDriverTests()
    {
        _driver = new PCDriver();
    }

    [Fact]
    public void OpenTest()
    {
        _node = _driver.Open(null, null);
    }

    [Fact]
    public void ReadTest()
    {

    }

    [Fact]
    public void ControlTest()
    {

    }

    [Fact]
    public void SpeakTest()
    {
        _driver.Speak("学无先后达者为师");
        Thread.Sleep(1000);
    }

    [Fact]
    public void RebootTest()
    {
        var rs = _driver.Reboot(15);
        Assert.True(rs > 0);

        Thread.Sleep(1000);

        "shutdown".ShellExecute("-a");
    }

    [Fact]
    public void GetSpecificationTest()
    {

    }

    [Fact]
    public void CreateTest()
    {

    }

    [Fact]
    public void CreateTest1()
    {

    }

    [Fact]
    public void CreateTest2()
    {

    }

    [Fact]
    public void CreateTest3()
    {

    }

    [Fact]
    public void CreateTest4()
    {

    }
}