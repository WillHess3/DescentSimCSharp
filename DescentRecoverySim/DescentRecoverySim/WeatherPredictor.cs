namespace DescentRecoverySim {
    using System.IO;
    using System;
    public class WeatherPredictor {

        public List<DataPoint> DataPoints;

        private readonly string _filePath;

        private readonly Random _radom;

        public class DataPoint {
            public List<int> Heights { get; set; } = new List<int>();
            public List<float> WindSpeed { get; set; } = new List<float>();
            public List<int> WindAngle { get; set; } = new List<int>();

            public void PrintDataPoint() {
                for (int i = 0; i < Heights.Count; i++) {
                    Console.WriteLine(Heights[i] + "m - " + WindSpeed[i] + "m/s - " + WindAngle[i] + "°");
                }
            }
        }

        private readonly bool _isPrintLineNumbersForCleaningData;

        public WeatherPredictor(string filePath) {
            _filePath = filePath;
            _radom = new Random();

            _isPrintLineNumbersForCleaningData = false;
            DataPoints = new List<DataPoint>();
            GetData();
        }

        public DataPoint ChooseRandomDataPoint() {
            int selectionIndex = (int)(_radom.NextDouble() * DataPoints.Count);
            DataPoint dataPoint = DataPoints[selectionIndex];

            DataPoints.RemoveAt(selectionIndex);
            return dataPoint;
        }

        private void GetData() {
            using StreamReader reader = new StreamReader(_filePath);

            string? line;
            DataPoint? dataPoint = null;
            int lineNumber = 0;
            while ((line = reader.ReadLine()) != null) {
                lineNumber++;
                // ---------- HEADER LINE ----------
                if (line.StartsWith("#")) {
                    // Save previous block
                    if (dataPoint != null)
                        DataPoints.Add(dataPoint);

                    // Start new block
                    dataPoint = new DataPoint();
                    continue;
                }

                // ---------- DATA LINE ----------
                if (dataPoint == null) {
                    continue;
                }

                // Split safely
                string[] parts = line.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 8) {
                    continue;
                }

                // HEIGHT
                int height;
                string value = parts[3];
                if (value == "-9999") {
                    continue;
                }

                // Remove trailing flags
                if (char.IsLetter(value[^1])) {
                    value = value.Substring(0, value.Length - 1);
                }

                height = int.Parse(value);

                // WIND DIRECTION
                int angle;
                if (parts[6] == "-9999") {
                    continue;
                }

                angle = int.Parse(parts[6]) % 360;

                // WIND SPEED
                float windSpeed;
                if (parts[7] == "-9999") {
                    continue;
                }

                windSpeed = int.Parse(parts[7]) / 10f;

                //fill out dataPoint
                dataPoint.Heights.Add(height);
                dataPoint.WindAngle.Add(angle);
                dataPoint.WindSpeed.Add(windSpeed);

                if (_isPrintLineNumbersForCleaningData) {
                    Console.WriteLine(lineNumber);
                }
            }

            // add final block
            if (dataPoint != null) {
                DataPoints.Add(dataPoint);
            }
        }
    }
}