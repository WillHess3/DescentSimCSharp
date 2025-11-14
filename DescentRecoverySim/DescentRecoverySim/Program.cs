namespace DescentRecoverySim {
    
    public class Program {
        private static void Main(string[] args) {
            //for (int i = 0; i < 100; i++) {
                Simulation simulation = new Simulation(1000);
                simulation.Simulate();
            //}
        }
    }
}
