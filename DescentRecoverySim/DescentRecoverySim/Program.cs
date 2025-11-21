namespace DescentRecoverySim {
    
    public class Program {
        private static void Main(string[] args) {
            string filePath = "output.csv";

            //ask for custom data
            Console.WriteLine("Would you like to use custom data? y/N");
            string response = Console.ReadLine();

            if (response == "y") {
                //get custom data

            } else {
                //simulate randomly using weather data
                WeatherPredictor weatherPredictor = new WeatherPredictor("C:/Misc Coding Projects/DescentRecoverySimRepo/DescentSimCSharp/DescentRecoverySim/CapeCanaveral2024-2025Data.txt");

                Console.WriteLine("How many simulations would you like to run?");
                int numberOfSimulations = Math.Min(int.Parse(Console.ReadLine()), weatherPredictor.DataPoints.Count);

                //simulate many times
                for (int i = 0; i < numberOfSimulations; i++) {
                    Simulation simulation = new Simulation(weatherPredictor.ChooseRandomDataPoint(), 3048);

                    string simOutput = simulation.Simulate();
                    
                    File.AppendAllText(filePath, simOutput + Environment.NewLine);
                }
            }
        }
    }
}
