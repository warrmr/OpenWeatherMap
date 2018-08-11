using System;
using System.Text;
using Crestron.SimplSharp;
using Newtonsoft.Json;
using Crestron.SimplSharp.Net.Https;
using System.Collections.Generic; // For Basic SIMPL# Classes

namespace OpenWeatherMap
{
    public class OpenWeather
    {
        public string APIKey { get; set; }
        public string cityID { get; set; }

        public string weatherDescription { get; set; }
        public string weatherIconURL { get; set; }

        public string Temprature { get; set; }
        public string Pressure { get; set; }
        public string Humidity { get; set; }

        public string windSpeed { get; set; }
        public string windDirection { get; set; }

        public string cloudCover { get; set; }
        public string rainVolume3h { get; set; }
        public string snowVolume3h { get; set; }

        public string timeSunrise { get; set; }
        public string timeSunset { get; set; }
        public string timeCalculated { get; set; }

        private int uts;

        public void getWeather(string units)
        {
            string url;

            if (units == "K")
                url = string.Format("https://api.openweathermap.org/data/2.5/weather?id={0}&appid={1}", cityID, APIKey);
            else
                url = string.Format("https://api.openweathermap.org/data/2.5/weather?id={0}&units={1}&appid={2}", cityID, units, APIKey);

            CrestronConsole.PrintLine("getWeather(string units) url: {0}", url);

            if (units == "metric")
                uts = 1;
            else if (units == "imperial")
                uts = 2;
            else
                uts = 0;

            string json = getData(url);
            parseJson(json);
        }

        private string getData(string url)
        {
            HttpsClient client = new HttpsClient();
            return client.Get(url);
        }

        private void parseJson(string json)
        {
            CrestronConsole.PrintLine("parseJson(string json) json: {0}", json);

            try
            {
                if (json.Contains("\"weather\":"))
                {
                    parseWeatherJson(json);
                }
                else if (json.Contains("\"message\":") && json.Contains("\"cod\":"))
                {
                    CrestronConsole.PrintLine("OpenWeather Error: {0}", parseErrorJson(json));
                }
                else
                {
                    CrestronConsole.PrintLine("OpenWeather Invalid json: {0}", json);
                }
            }
            catch (Exception ex)
            {
                CrestronConsole.PrintLine("OpenWeather Exception Caught: {0}", ex);
            }
        }

        private string parseErrorJson(string json)
        {
            ApiIssue myIssue = JsonConvert.DeserializeObject<ApiIssue>(json);
            return myIssue.message;
        }

        private void parseWeatherJson(string json)
        {
            RootObject myWeather = JsonConvert.DeserializeObject<RootObject>(json);

            weatherDescription = myWeather.weather[0].description;
            weatherIconURL = string.Format("http://openweathermap.org/img/w/{0}.png", myWeather.weather[0].icon);

            Pressure = string.Format("{0} hPa", myWeather.main.pressure);
            Humidity = string.Format("{0}%", myWeather.main.humidity);

            windDirection = getWindDirection(myWeather.wind.deg);

            timeSunrise = convertTime(myWeather.sys.sunrise);
            timeSunset = convertTime(myWeather.sys.sunset);
            timeCalculated = convertDateTime(myWeather.dt);

            if (myWeather.clouds != null)
                cloudCover = string.Format("{0}%", myWeather.clouds.all);

            if (myWeather.rain != null)
                rainVolume3h = string.Format("{0}mm", myWeather.rain.all);

            if (myWeather.snow != null)
                snowVolume3h = string.Format("{0}mm", myWeather.snow.all);

            switch (uts)
            {
                case 0:     // Kalvin
                    Temprature = string.Format("{0}K", myWeather.main.temp);
                    windSpeed = string.Format("{0} m/s", myWeather.wind.speed);
                    break;
                case 1:     // Metric
                    Temprature = string.Format("{0}C", myWeather.main.temp);
                    windSpeed = string.Format("{0} m/s", myWeather.wind.speed);
                    break;
                case 2:     // Imperial
                    Temprature = string.Format("{0}F", myWeather.main.temp);
                    windSpeed = string.Format("{0} mph", myWeather.wind.speed);
                    break;
            }
        }

        private string getWindDirection(double deg)
        {
            string[] direction = new string[] { "N", "NNE", "NE", "ENE", "E", "ESE", "SE", "SSE", "S", "SSW", "SW", "WSW", "W", "WNW", "NW", "NNW", "N" };
            double degrees = deg % 360;
            degrees = (degrees / 22.5) + 1;
            return direction[Convert.ToInt32(degrees)];
        }

        private string convertTime(double timestamp)
        {
            DateTime myTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            myTime = myTime.AddSeconds(timestamp);
            return myTime.ToString("HH:mm");
        }

        private string convertDateTime(double timestamp)
        {
            DateTime myTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            myTime = myTime.AddSeconds(timestamp);
            return myTime.ToString("HH:mm:ss dd/mm/yyyy");
        }
    }

    public class Coord
    {
        [JsonProperty("lon")]
        public double lon { get; set; }

        [JsonProperty("lat")]
        public double lat { get; set; }
    }

    public class Weather
    {
        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("main")]
        public string main { get; set; }

        [JsonProperty("description")]
        public string description { get; set; }

        [JsonProperty("icon")]
        public string icon { get; set; }
    }

    public class Main
    {
        [JsonProperty("temp")]
        public double temp { get; set; }

        [JsonProperty("pressure")]
        public double pressure { get; set; }

        [JsonProperty("humidity")]
        public int humidity { get; set; }

        [JsonProperty("temp_min")]
        public double temp_min { get; set; }

        [JsonProperty("temp_max")]
        public double temp_max { get; set; }

        [JsonProperty("sea_level")]
        public double sea_level { get; set; }

        [JsonProperty("grnd_level")]
        public double grnd_level { get; set; }
    }

    public class Wind
    {
        [JsonProperty("speed")]
        public double speed { get; set; }

        [JsonProperty("deg")]
        public double deg { get; set; }
    }

    public class Rain
    {
        [JsonProperty("3h")]
        public double all { get; set; }
    }

    public class Snow
    {
        [JsonProperty("3h")]
        public double all { get; set; }
    }

    public class Clouds
    {
        [JsonProperty("all")]
        public int all { get; set; }
    }

    public class Sys
    {
        [JsonProperty("message")]
        public double message { get; set; }

        [JsonProperty("country")]
        public string country { get; set; }

        [JsonProperty("sunrise")]
        public int sunrise { get; set; }

        [JsonProperty("sunset")]
        public int sunset { get; set; }
    }

    public class RootObject
    {
        [JsonProperty("coord")]
        public Coord coord { get; set; }

        [JsonProperty("weather")]
        public IList<Weather> weather { get; set; }

        [JsonProperty("base")]
        public string baseStation { get; set; }

        [JsonProperty("main")]
        public Main main { get; set; }

        [JsonProperty("wind")]
        public Wind wind { get; set; }

        [JsonProperty("rain")]
        public Rain rain { get; set; }

        [JsonProperty("snow")]
        public Snow snow { get; set; }

        [JsonProperty("clouds")]
        public Clouds clouds { get; set; }

        [JsonProperty("dt")]
        public int dt { get; set; }

        [JsonProperty("sys")]
        public Sys sys { get; set; }

        [JsonProperty("id")]
        public int id { get; set; }

        [JsonProperty("name")]
        public string name { get; set; }

        [JsonProperty("cod")]
        public int cod { get; set; }
    }

    public class ApiIssue
    {
        public int cod { get; set; }
        public string message { get; set; }
    }
}
