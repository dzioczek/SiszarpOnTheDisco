namespace SiszarpOnTheDisco.Models.Allergens;

public class Dust
{
    //{
    //    "id": 1504,
    //    "startDate": {
    //        "date": "2021-04-16 00:30:13.000000",
    //        "timezone_type": 3,
    //        "timezone": "Europe/Warsaw"
    //    },
    //    "endDate": {
    //        "date": "2021-12-31 00:30:13.000000",
    //        "timezone_type": 3,
    //        "timezone": "Europe/Warsaw"
    //    },
    //    "trend": "Bez zmian",
    //    "level": "Niskie",
    //    "value": 5,
    //    "region": {
    //        "id": 7,
    //        "name": "Małopolska i Ziemia Lubelska"
    //    },
    //    "allergen": {
    //        "id": 20,
    //        "name": "Cladosporium"
    //    }
    //}

    public int id { get; set; }
    public AllergensDate startDate { get; set; }
    public AllergensDate endDate { get; set; }
    public string trend { get; set; }
    public string level { get; set; }
    public int value { get; set; }
    public Region region { get; set; }
    public Allergen allergen { get; set; }
}