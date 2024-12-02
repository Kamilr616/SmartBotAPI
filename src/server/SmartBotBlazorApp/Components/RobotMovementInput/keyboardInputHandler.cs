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

        public keyboardInputHandler()
        {
            counter = 0;
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
                    //robotDir = -1;
                    validInput = false;
                    break;
            }
        }

        public void amIResetingCounter(bool reset)
        {
            if(reset)
                counter = 0;
        }
    }
}
