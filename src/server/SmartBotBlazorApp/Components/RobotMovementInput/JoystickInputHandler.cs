using SmartBotBlazorApp.Client.Pages;

namespace SmartBotBlazorApp.Components.RobotMovementInput
{
    public class JoystickInputHandler
    {
        private double _centerX;
        private double _centerY;
        private double _knobPosX;
        private double _knobPosY;
        private double _radius;
        private double _touchStartX;
        private double _touchStartY;

        public string? joystickDirection { get; private set; }
        public ROBOT_DIRECITON robotDir { get; private set; }
        public bool validInput { get; private set; }

        public int Counter { get; private set; }
        public bool IsTouching { get; private set; }

        public double KnobPosX => _knobPosX;
        public double KnobPosY => _knobPosY;

        public JoystickInputHandler(double centerX, double centerY, double radius)
        {
            _centerX = centerX;
            _centerY = centerY;
            _knobPosX = centerX;
            _knobPosY = centerY;
            _radius = radius;
            joystickDirection = "Center";
            Counter = 0;
            IsTouching = false;
        }

        public void OnPointerDown(double clientX, double clientY)
        {
            IsTouching = true;
            _touchStartX = clientX;
            _touchStartY = clientY;
        }

        public void OnPointerMove(double clientX, double clientY)
        {
            if (IsTouching)
            {
                double deltaX = clientX - _touchStartX;
                double deltaY = clientY - _touchStartY;

                // Calculate distance from center
                double newPosX = _centerX + deltaX;
                double newPosY = _centerY + deltaY;

                double distance = Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));
                // Limit the distance to the radius of the joystick container
                if (distance > _radius)
                {
                    newPosX = _knobPosX;
                    newPosY = _knobPosY;
                }

                _knobPosX = newPosX;
                _knobPosY = newPosY;

                Counter++;
                GetJoystickDirection(_knobPosX, _knobPosY);
            }
        }

        public void OnPointerUp()
        {
            IsTouching = false;
            _knobPosX = _centerX;
            _knobPosY = _centerY;
            joystickDirection = "Center";
            _touchStartX = _centerX;
            _touchStartY = _centerY;
        }

        private void GetJoystickDirection(double knobPosX, double knobPosY)
        {
            double deltaX = knobPosX - _centerX;
            double deltaY = knobPosY - _centerY;
            double threshold = 5;
            ROBOT_DIRECITON prevDir = robotDir;
            if (Math.Abs(deltaX) > Math.Abs(deltaY)) // Horizontal
            {
                if (deltaX > threshold)
                {
                    validInput = true;
                    robotDir = ROBOT_DIRECITON.RIGHT;
                    joystickDirection = "Right";

                }
                else if (deltaX < -threshold)
                {
                    validInput = true;
                    robotDir = ROBOT_DIRECITON.LEFT;
                    joystickDirection = "Left";
                    
                }
            }
            else // Vertical
            {
                if (deltaY > threshold)
                {
                    validInput = true;
                    robotDir = ROBOT_DIRECITON.DOWN;
                    joystickDirection = "Down";
                }
                else if (deltaY < -threshold)
                {
                    validInput = true;
                    robotDir = ROBOT_DIRECITON.UP;
                    joystickDirection = "Up";
                }
                else
                {

                    validInput = false;
                    robotDir = ROBOT_DIRECITON.STOP;
                    joystickDirection = "Center";
                    Counter = 0;
                }

            }
            if (prevDir != robotDir)
            {
                Counter = 0;

            }
        }



        public void increaseCounter()
        {
            Counter++;
        }

    }



}
