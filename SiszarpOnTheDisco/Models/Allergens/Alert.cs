using System.Collections.Generic;

namespace SiszarpOnTheDisco.Models.Allergens;

public class Alert
{
    //        [
    //    {
    //        "id": 563,
    //        "text": "W okresach rozpogodzeń zagrożenie alergenami pyłku brzozy. Pylą też dęby, klony, jesiony. ",
    //        "startDate": {
    //            "date": "2021-05-01 12:08:45.000000",
    //            "timezone_type": 3,
    //            "timezone": "Europe/Warsaw"
    //        },
    //        "endDate": {
    //            "date": "2021-05-05 12:08:45.000000",
    //            "timezone_type": 3,
    //            "timezone": "Europe/Warsaw"
    //        },
    //        "regions": [
    //            {
    //                "id": 7,
    //                "name": "Małopolska i Ziemia Lubelska"
    //            }
    //        ]
    //    }
    //]

    public int id { get; set; }
    public string text { get; set; }
    public AllergensDate startDate { get; set; }
    public AllergensDate endDate { get; set; }
    public List<Region> regions { get; set; }
}