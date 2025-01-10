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
        public ROBOT_DIRECITON robotDir { get; private set; }
        public bool validInput { get; private set; }

        public int leftEngine { get; private set; }
        public int rightEngine { get; private set; }

        public keyboardInputHandler()
        {
            counter = 0;
            keyName = "";
            robotDir = ROBOT_DIRECITON.STOP;
            validInput = false;
        }


        public void onKeyUP(KeyboardEventArgs e)
        {
            keyName = "";
            robotDir = ROBOT_DIRECITON.STOP;
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
                    robotDir = ROBOT_DIRECITON.UP;
                    validInput = true;
                    break;

                case "ArrowDown":
                    keyName = "Down Arrow";
                    robotDir = ROBOT_DIRECITON.DOWN;
                    validInput = true;
                    break;

                case "ArrowLeft":
                    keyName = "Left Arrow";
                    robotDir = ROBOT_DIRECITON.LEFT;
                    validInput = true;
                    break;

                case "ArrowRight":
                    keyName = "Right Arrow";
                    robotDir = ROBOT_DIRECITON.RIGHT;
                    validInput = true;
                    break;

                default:
                    keyName = "Wrong Button";
                    robotDir = ROBOT_DIRECITON.STOP;
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


        //TODO zamienic to na slownik;
        private void translateKeysToEngineValues()
        {
            switch (robotDir)
            {
                case ROBOT_DIRECITON.UP:
                    leftEngine = 255;
                    rightEngine = 255;
                    break;
                case ROBOT_DIRECITON.DOWN:
                    leftEngine = -255;
                    rightEngine = -255;
                    break;
                case ROBOT_DIRECITON.LEFT:
                    leftEngine = 255;
                    rightEngine = -255;
                    break;
                case ROBOT_DIRECITON.RIGHT:
                    leftEngine = -255;
                    rightEngine = 255;
                    break;
                case ROBOT_DIRECITON.STOP:
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
