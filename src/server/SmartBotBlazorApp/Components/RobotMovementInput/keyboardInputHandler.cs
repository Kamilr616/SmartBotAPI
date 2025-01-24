using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace SmartBotBlazorApp.Components.RobotMovementInput
{
    public class keyboardInputHandler
    {
        public ElementReference keyboardInputRef;
        public string? pressedKey { get; private set; }
        public int counter { get; private set; }

        public string keyName { get; private set; }
        public RobotDirectionEnum robotDir { get; private set; }
        public bool validInput { get; private set; }

        public int leftEngine { get; private set; }
        public int rightEngine { get; private set; }

        public keyboardInputHandler()
        {
            counter = 0;
            keyName = "";
            robotDir = RobotDirectionEnum.STOP;
            validInput = false;
        }


        public void onKeyUP(KeyboardEventArgs e)
        {
            keyName = "";
            robotDir = RobotDirectionEnum.STOP;
            validInput = false;
        }
        public void onKeyDown(KeyboardEventArgs e)
        {
            counter++;
            pressedKey = e.Key;
 
            switch (e.Key)
            {
                case "ArrowUp":
                    keyName = "Up Arrow";
                    robotDir = RobotDirectionEnum.UP;
                    validInput = true;
                    break;

                case "ArrowDown":
                    keyName = "Down Arrow";
                    robotDir = RobotDirectionEnum.DOWN;
                    validInput = true;
                    break;

                case "ArrowLeft":
                    keyName = "Left Arrow";
                    robotDir = RobotDirectionEnum.LEFT;
                    validInput = true;
                    break;

                case "ArrowRight":
                    keyName = "Right Arrow";
                    robotDir = RobotDirectionEnum.RIGHT;
                    validInput = true;
                    break;

                default:
                    keyName = "Wrong Button";
                    robotDir = RobotDirectionEnum.STOP;
                    validInput = false;
                    break;
            }
            translateKeysToEngineValues();
        }

        public void amIResetingCounter(bool reset)
        {
            if (!reset)
                return;

            counter = 0;
        }

       private void translateKeysToEngineValues()
        {
            switch (robotDir)
            {
                case RobotDirectionEnum.UP:
                    leftEngine = 255;
                    rightEngine = 255;
                    break;
                case RobotDirectionEnum.DOWN:
                    leftEngine = -255;
                    rightEngine = -255;
                    break;
                case RobotDirectionEnum.LEFT:
                    leftEngine = 255;
                    rightEngine = -255;
                    break;
                case RobotDirectionEnum.RIGHT:
                    leftEngine = -255;
                    rightEngine = 255;
                    break;
                case RobotDirectionEnum.STOP:
                    leftEngine = 0;
                    rightEngine = 0;
                    break;
            }
        }

        public (int, int) GetRobotEngineValues()
        {
            return (leftEngine, rightEngine);
        }

    }
}
