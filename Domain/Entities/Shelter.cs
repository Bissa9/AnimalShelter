public class Shelter {
    public int Id {get; set;}
    public string Name {get; set;} = "";
    public string Location {get; set;} = "";
    public List<Animal> Animals {get; set;} = new();

}