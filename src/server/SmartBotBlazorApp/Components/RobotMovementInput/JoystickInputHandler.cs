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

        public int LeftEngine { get; private set; } = 0 ;
        public int RightEngine { get; private set; } = 0;

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
                SetJoystickDirection(_knobPosX, _knobPosY);
                translateJoystickToRobotEngineValues();
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

        private void SetJoystickDirection(double knobPosX, double knobPosY)
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

                    validInput = true;
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

        public (int, int) GetRobotEngineValues()
        {
            return (LeftEngine, RightEngine);


            //return ((int)_knobPosX, (int)_knobPosY);

            //float tmpX = 0;
            //float tmpY = 0;
            //tmpX = (float)_knobPosX - 50;
            //tmpY = (float)_knobPosY - 50;
            //tmpX *= 10;
            //tmpY *= -10;


            //return ((int)tmpX, (int)tmpY);
        }


        /* 

                private void translateJoystickToRobotEngineValues()
                {
                    //tlumaczenie na zasieg -255 : 255
                    float tmpX = 0;
                    float tmpY = 0;
                    tmpX = (float)_knobPosX - 50;
                    tmpY = (float)_knobPosY - 50;
                    tmpX *= 10;
                    tmpY *= -10;
                    //obliczenia/no nie do konca dzialaja
                    //prawdopodobnie sa zwalone przypisania do wartosci

                    float Z = (tmpX * tmpX) + (tmpY * tmpY);
                    Z = MathF.Sqrt(Z);

                    if(tmpX<0)
                    {
                        if(tmpY<0)
                        {
                            LeftEngine = (int)tmpX;
                            RightEngine = (-1) * (int)Z ;
                        }
                        else
                        {
                            LeftEngine = (int)Z;
                            RightEngine = (int)tmpY;
                        }
                    }
                    else
                    {
                        if(tmpY<0)
                        {
                            LeftEngine = (-1) * (int)Z;
                            RightEngine = (int)tmpY;
                        }
                        else
                        {
                            LeftEngine = (int)tmpX;
                            RightEngine = (int)Z;
                        }

                    }
                }
        */


        //KISS
        //XDD
        private void translateJoystickToRobotEngineValues()
        {
            float deadzone = 0.25f;
            float stopDeadZone = 0.1f;
            const int maxEngineValue = 255;
           
            
            float tmpX = ((float)_knobPosX - (float)_centerX) / (float)_centerX;
            float tmpY = ((float)_knobPosY - (float)_centerY) / (float)_centerY;
            //odwracamy Y, bo jest ośka liczona od góry
            tmpY *= -2;
            tmpX *= -2;


            //DeadZone, żeby łatwiej się stopowało
            float leftSpeed = 0;
            float rightSpeed = 0;
            if(Math.Abs(tmpX) < stopDeadZone && Math.Abs(tmpY) < stopDeadZone )
            {
                leftSpeed = 0;
                rightSpeed = 0;
            }
            //DeadZone, żeby łatwiej się jechało przód/tył
            else if (Math.Abs(tmpX) < deadzone)
            {
                leftSpeed = tmpY;
                rightSpeed = tmpY;
            }
            else
            {
                leftSpeed = tmpY + tmpX;
                rightSpeed = tmpY - tmpX;
            }

            LeftEngine = (int)(Math.Clamp(leftSpeed, -1, 1) * maxEngineValue);
            RightEngine = (int)(Math.Clamp(rightSpeed, -1, 1) * maxEngineValue);
        }

        public void resetCounter()
        {
            Counter = 0;
            //LeftEngine = 0;
            //RightEngine = 0;
        }
        public void increaseCounter()
        {
            Counter++;
        }

    }



}
