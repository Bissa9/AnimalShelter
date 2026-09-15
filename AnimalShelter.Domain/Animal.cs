namespace AnimalShelter.Domain;
public class Animal
{
    public int Id { get; set; }

    public string Name { get; set; } = "";

    public int Age { get; set; }

    public string Type { get; set; } = "";

    public int ShelterId { get; set; }

    public Shelter? Shelter { get; set; }
    
    public bool IsAdopted { get; set; }

    public Guid Version { get; set; } = Guid.NewGuid();
}