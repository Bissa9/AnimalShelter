using System.ComponentModel.DataAnnotations;
public class AnimalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public string Type { get; set; } = "";

    public int ShelterId { get; set; }
    public string ShelterName { get; set; } = "";
    public string ShelterLocation { get; set; } = "";
}