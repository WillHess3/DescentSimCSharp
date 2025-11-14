namespace DescentRecoverySim {
    public class Atmosphere {

        private readonly double _relativeHumididty; // [0,1] 
        private readonly double _windsAloftSpeed; // [m/s]
        private readonly double _windsAloftAngle; // [rad]

        public Atmosphere() {
            _relativeHumididty = 0.74;
        }

        public double CalculateAirDensity(double height) {
            //height is in meters and air density is in kg/m^3
            double pressure;
            double temperature;

            //Based on NASA's Earth Atmosphere Model https://www.grc.nasa.gov/www/k-12/airplane/atmosmet.html
            if (height < 11000) {
                temperature = (15.04 - 0.00649 * height) + 273.15;
                pressure = 101.29 * Math.Pow(temperature / 288.08, 5.256) * 1000;
            } else if (height < 25000) {
                temperature = 216.69;
                pressure = 22.65 * Math.Exp(1.73 - .000157 * height) * 1000;
            } else {
                temperature = (-131.21 + .00299 * height) + 273.15;
                pressure = 2.488 * Math.Pow(temperature / 216.6, -11.388) * 1000;
            }

            //based on magnus-tetens approximation
            double saturationVaporPressure = 6.1078 * Math.Pow(10, 7.5 * (temperature - 273.15) / temperature) * 100;
            double vaporPressure = _relativeHumididty * saturationVaporPressure;

            double dryPressure = pressure - vaporPressure;


            return dryPressure / (temperature * 287.05) + vaporPressure / (temperature * 461.495);
        }

        public double GetWindSpeed() {
            return 4.47;
        }

        public double GetWindAngle() {
            return -Math.PI / 4.0;
        }
    }
}
