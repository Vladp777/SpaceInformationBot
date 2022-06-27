namespace SpaceInformationBot.Model
{
    public class PeopleInSpace
    {
        public string message { get; set; }
        public int number { get; set; }
        public List<People> people { get; set; }
    }
    public class People
    {
        public string name { get; set; }
        public string craft { get; set; }
    }
}
