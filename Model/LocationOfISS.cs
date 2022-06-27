namespace SpaceInformationBot.Model
{
    public class LocationOfISS
    {
        public string message { get; set; }
        public string timestamp { get; set; }
        public Position iss_position { get; set; }
    }

    public class Position
    {
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
