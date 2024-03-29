using Ardalis.SmartEnum; 
namespace SiszarpOnTheDisco.Models.Lawn;

public sealed  class LawnEventTypes : SmartEnum<LawnEventTypes>
{
    public static readonly LawnEventTypes Mowing = new("Koszenie", 1);
    public static readonly LawnEventTypes Fertilizing = new("Nawożenie", 2);
    public static readonly LawnEventTypes Raking = new("Grabienie", 3);
    public static readonly LawnEventTypes Edging = new("Krawędziowanie", 4);
    public static readonly LawnEventTypes Trimming = new("Podkaszanie", 5); 

    public LawnEventTypes(string name, int value) : base(name, value)
    {
        
    }
}