namespace DescentRecoverySim {
    using System;

    public class Simulation {

        //timestep variables
        private double _time;
        private readonly double _deltaTime;

        //rocket & parachute properties
        private readonly double _mass; // [kg]
        private readonly double _apogee; // [m]
        private readonly double _parachuteCd; 
        private readonly double _drogueArea; // [m^2]
        private readonly double _mainArea; // [m^2]
        private readonly double _mainDeploymentAltitude; // [m]
        private bool _isMainDeployed;
        private readonly double _rocketCdHorizontal;
        private readonly double _rocketVerticalCrossSectionalArea; // [m^2]
        private readonly double _initialHorizontalSpeed; // [m/s]
        private readonly double _angle; // [m]

        //physics variables
        private double _altitude; // [m]
        private double _verticalVelocity; // [m/s]
        private readonly double _forceGravity; // [N]
        private double _velocityX; // [m/s]
        private double _velocityY; // [m/s]
        private double _positionX; // [m]
        private double _positionY; // [m]

        //weather variables
        private readonly Atmosphere _atmosphere;

        public Simulation(WeatherPredictor.DataPoint weatherData, double mainDeploymentAltitude) {
            //time variables
            _time = 0;
            _deltaTime = 0.0001;

            //rocket constants
            _mass = 9.686;
            _apogee = 36500;
            _parachuteCd = 0.97;
            _drogueArea = 0.073;
            _mainArea = 1.167;
            _mainDeploymentAltitude = mainDeploymentAltitude;
            _isMainDeployed = false;
            _rocketCdHorizontal = 0.82;
            _rocketVerticalCrossSectionalArea = 0.155;
            _initialHorizontalSpeed = 30;
            _angle = Math.PI;

            //physics variables
            _altitude = _apogee;
            _verticalVelocity = 0;
            _forceGravity = -9.81 * _mass;
            _velocityX = _initialHorizontalSpeed * Math.Cos(_angle);
            _velocityY = _initialHorizontalSpeed * Math.Sin(_angle);

            _atmosphere = new Atmosphere(weatherData);
        }

        public string Simulate() {
            int stepIndex = 0;
            while (stepIndex < 10000000) {
                //get time value
                _time = stepIndex * _deltaTime;
                stepIndex++;

                //update atmosphere
                _atmosphere.UpdateHeight(_altitude);

                //calculate forces
                ApplyVerticalForces();
                ApplyHorizontalForces();

                //deploy main
                if (!_isMainDeployed && _altitude < _mainDeploymentAltitude) {
                    _isMainDeployed = true;
                    Console.WriteLine("Main Deployed at " + _altitude + "m after " + _time + "s");
                }

                //exit condition
                if (_altitude < 0) {
                    break;
                }
            }

            Console.WriteLine("-----Simulation Results-----");
            Console.WriteLine("time: " + _time);
            Console.WriteLine("horizontal displacement: (" + _positionX + ", " + _positionY + ")");
            Console.WriteLine("horizontal velocity: (" + _velocityX + ", " + _velocityY + ")");

            return $"{_positionX},{_positionY}";
        }

        //calculates the forces in the vertical plane (parachute drag and gravity)
        private void ApplyVerticalForces() {
            double drag = CalculateDrag(_verticalVelocity, _isMainDeployed ? _mainArea : _drogueArea, _parachuteCd);
            double netForce = drag + _forceGravity;

            double acceleration = netForce / _mass;
            _verticalVelocity += acceleration * _deltaTime;
            _altitude += _verticalVelocity * _deltaTime;
        }

        //calculates the forces from the wind responsible for the rocket horizontally drifting away from apogee
        private void ApplyHorizontalForces() {
            //get speed and direction
            double windSpeed = _atmosphere.GetWindSpeed();
            double windAngle = _atmosphere.GetWindAngle();

            //calculate velocity and reletive velocity
            double windVelocityX = windSpeed * Math.Cos(windAngle);
            double windVelocityY = windSpeed * Math.Sin(windAngle);

            double relativeVelocityX = _velocityX - windVelocityX;
            double relativeVelocityY = _velocityY - windVelocityY;
            double relativeVelocityMagnitude = Math.Sqrt(relativeVelocityX * relativeVelocityX + relativeVelocityY * relativeVelocityY);

            //update velocity from drag force
            if (relativeVelocityMagnitude > 0.0000001) {
                double dragForce = CalculateDrag(relativeVelocityMagnitude, _rocketVerticalCrossSectionalArea, _rocketCdHorizontal);

                // convert force to acceleration
                double accelerationX = -dragForce * relativeVelocityX / relativeVelocityMagnitude / _mass;
                double accelerationY = -dragForce * relativeVelocityY / relativeVelocityMagnitude / _mass;

                _velocityX += accelerationX * _deltaTime;
                _velocityY += accelerationY * _deltaTime;
            }

            //update position
            _positionX += _velocityX * _deltaTime;
            _positionY += _velocityY * _deltaTime;
        }

        //calculates the drag (all units are SI)
        private double CalculateDrag(double velocity, double area, double cd) {
            return 0.5 * _atmosphere.CalculateAirDensity(_altitude) * velocity * velocity * area * cd;
        }
    }
}
