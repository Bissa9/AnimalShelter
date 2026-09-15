namespace AnimalShelter.Domain;

public class Adoption
{
    public int Id { get; set; }

    public int AnimalId { get; set; }

    public Animal? Animal { get; set; }

    public string AdopterName { get; set; } = "";

    public DateTime AdoptedAt { get; set; }
}