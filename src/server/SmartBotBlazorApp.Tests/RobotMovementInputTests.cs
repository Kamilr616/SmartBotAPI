using Microsoft.AspNetCore.Components.Web;
using SmartBotBlazorApp.Components.RobotMovementInput;

namespace SmartBotBlazorApp.Tests;

public class RobotMovementInputTests
{
    private const double Center = 100;
    private const double Radius = 100;

    [Fact]
    public void JoystickCenterDeadZoneStopsBothMotors()
    {
        var handler = MoveJoystick(105, 105);

        Assert.Equal((0, 0), handler.GetRobotEngineValues());
    }

    [Fact]
    public void JoystickStraightDrivingDeadZoneIgnoresSmallHorizontalOffset()
    {
        var handler = MoveJoystick(115, 50);

        Assert.Equal((255, 255), handler.GetRobotEngineValues());
    }

    [Fact]
    public void DiagonalJoystickInputMixesMotorsForAProportionalArc()
    {
        var handler = MoveJoystick(125, 50);

        Assert.Equal((127, 255), handler.GetRobotEngineValues());
    }

    [Fact]
    public void HorizontalJoystickInputRotatesRobotAroundItsAxis()
    {
        var handler = MoveJoystick(150, 100);

        Assert.Equal((-255, 255), handler.GetRobotEngineValues());
    }

    [Fact]
    public void ReleasingJoystickResetsPositionAndStopsBothMotors()
    {
        var handler = MoveJoystick(100, 50);

        handler.OnPointerUp();

        Assert.Equal((0, 0), handler.GetRobotEngineValues());
        Assert.Equal(Center, handler.KnobPosX);
        Assert.Equal(Center, handler.KnobPosY);
        Assert.Equal("Center", handler.joystickDirection);
    }

    [Theory]
    [InlineData("ArrowUp", 255, 255)]
    [InlineData("ArrowDown", -255, -255)]
    [InlineData("ArrowLeft", 255, -255)]
    [InlineData("ArrowRight", -255, 255)]
    public void ArrowKeysMapToExpectedMotorCommands(string key, int left, int right)
    {
        var handler = new keyboardInputHandler();

        handler.onKeyDown(new KeyboardEventArgs { Key = key });

        Assert.True(handler.validInput);
        Assert.Equal((left, right), handler.GetRobotEngineValues());
    }

    [Fact]
    public void UnsupportedKeyIsRejectedAndStopsBothMotors()
    {
        var handler = new keyboardInputHandler();

        handler.onKeyDown(new KeyboardEventArgs { Key = "Space" });

        Assert.False(handler.validInput);
        Assert.Equal((0, 0), handler.GetRobotEngineValues());
    }

    private static JoystickInputHandler MoveJoystick(double x, double y)
    {
        var handler = new JoystickInputHandler(Center, Center, Radius);
        handler.OnPointerDown(Center, Center);
        handler.OnPointerMove(x, y);
        return handler;
    }
}
