namespace DescentRecoverySim {
    public class Atmosphere {

        private readonly WeatherPredictor.DataPoint _weatherData;
        private readonly double _relativeHumididty; // [0,1] 

        private double _height; // [m]
        private int _currentDataIndex;
        private double _nextHeightThreashold;
        private double _t;

        public Atmosphere(WeatherPredictor.DataPoint weatherData) {
            _relativeHumididty = 0.74;
            _weatherData = weatherData;

            _currentDataIndex = weatherData.Heights.Count - 1;
            _nextHeightThreashold = _weatherData.Heights[_currentDataIndex];
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

        //Update's the atmosphere's height from the simulation height
        public void UpdateHeight(double height) {
            _height = height;

            //get next height threasholds
            while (_height <= _nextHeightThreashold) {
                _currentDataIndex--;

                if (_currentDataIndex >= 0) {
                    _nextHeightThreashold = _weatherData.Heights[_currentDataIndex];
                } else {
                    _nextHeightThreashold = 0;
                }
            }

            //update interpolation parameter
            _t = ComputeInterpolationParameter();
        }

        public double GetWindSpeed() {
            //finds upper bound and lower bound for speed taking into account being out of the data's range
            double speedUpper = _currentDataIndex + 1 < _weatherData.Heights.Count ? _weatherData.WindSpeed[_currentDataIndex + 1] : _weatherData.WindSpeed[_currentDataIndex];
            double speedLower = _currentDataIndex >= 0 ? _weatherData.WindSpeed[_currentDataIndex] : _weatherData.WindSpeed[_currentDataIndex + 1];

            //lerp
            return speedLower + (speedUpper - speedLower) * _t;
        }

        public double GetWindAngle() {
            //finds upper bound and lower bound for angle taking into account being out of the data's range
            double angleUpper = _currentDataIndex + 1 < _weatherData.Heights.Count ? _weatherData.WindAngle[_currentDataIndex + 1] : _weatherData.WindAngle[_currentDataIndex];
            double angleLower = _currentDataIndex >= 0 ? _weatherData.WindAngle[_currentDataIndex] : _weatherData.WindAngle[_currentDataIndex + 1];

            //lerp and convert to radians
            return (angleLower + (angleUpper - angleLower) * _t) * Math.PI / 180.0;
        }

        private double ComputeInterpolationParameter() {
            //finds final and initial heights taking into account being out of the data's range
            double finalHeight = _currentDataIndex + 1 < _weatherData.Heights.Count ? _weatherData.Heights[_currentDataIndex + 1] : double.MaxValue;
            double initialHeight = _currentDataIndex >= 0 ? _nextHeightThreashold : 0;
            double delta = finalHeight - initialHeight;

            //calculates t
            return (_height - initialHeight) / delta;
        }
    }
}
