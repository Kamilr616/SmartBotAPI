using Microsoft.AspNetCore.Components.Web;

namespace SmartBotBlazorApp.Components
{
    public class keyboardInputHandler
    {
        public string keyName;
        public int robotDir;
        public bool validInput;

        public keyboardInputHandler(KeyboardEventArgs e)
        {

            switch (e.Key)
            {
                case "ArrowUp":
                    keyName = "Up Arrow";
                    robotDir = 0;
                    validInput = true;
                    break;

                case "ArrowDown":
                    keyName = "Down Arrow";
                    robotDir = 1;
                    validInput = true;
                    break;

                case "ArrowLeft":
                    keyName = "Left Arrow";
                    robotDir = 2;
                    validInput = true;
                    break;

                case "ArrowRight":
                    keyName = "Right Arrow";
                    robotDir = 3;
                    validInput = true;
                    break;

                default:
                    keyName = "Wrong Button";
                    robotDir = -1;
                    validInput = false;
                    break;
            }
        }
    }
}
