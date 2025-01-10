namespace SmartBotBlazorApp.Components.RobotMovementInput
{
    public class debugFunctions
    {
        static public ushort[] GenerateTestData(int resolution)
        {
            var random = new Random();
            ushort[] data = new ushort[resolution * resolution];

            for (int i = 0; i < data.Length; i++)
            {
                data[i] = (ushort)random.Next(0, 4000);
            }

            return data;
        }

        static public ushort[] GenerateTestDataAscend(int resolution)
        {
            ushort[] data = new ushort[resolution * resolution];

            int index = 0;

            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    data[index] = (ushort)(i * 125 + j);
                    index++;
                }
            }

            return data;
        }


        static public double[] GenerateTestDataAscendDouble(int resolution)
        {
            double[] data = new double[resolution * resolution];

            int index = 0;

            for (int i = 0; i < resolution; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    data[index] = (ushort)(i * 125 + j);
                    index++;
                }
            }

            return data;
        }
    }
}
