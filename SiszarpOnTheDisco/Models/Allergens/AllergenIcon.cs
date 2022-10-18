namespace SiszarpOnTheDisco.Models.Allergens;

public class AllergenIcon
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string IconName { get; set; }

    public override string ToString()
    {
        return IconName;
    }
}