using Microsoft.AspNetCore.Components.Web;

namespace SmartBotBlazorApp.Components
{
    public class keyboardInputHandler
    {

        public string keyName;
        public ROBOT_DIRECITON robotDir;
        public bool validInput;

        public keyboardInputHandler(KeyboardEventArgs e)
        {

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
    }
}
