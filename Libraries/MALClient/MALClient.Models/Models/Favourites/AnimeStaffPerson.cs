﻿using MALClient.Models.Enums;

namespace MALClient.Models.Models.Favourites
{
    public class AnimeStaffPerson : FavouriteBase
    {
        public override FavouriteType Type { get; } = FavouriteType.Person;
        public bool IsUnknown { get; set; }
    }
}