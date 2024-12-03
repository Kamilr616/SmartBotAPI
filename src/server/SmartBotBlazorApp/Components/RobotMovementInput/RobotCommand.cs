namespace SmartBotBlazorApp.Components.RobotMovementInput
{
    public class RobotCommand
    {

        public string type { get; set; }
        public string target { get; set; }
        public int nextDirection { get; set; }


        public RobotCommand(ROBOT_DIRECITON nextDir)
        {
            type = "input";
            target = "movementController";
            nextDirection = (int)nextDir;
        }

        public string toJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }



    }
}
