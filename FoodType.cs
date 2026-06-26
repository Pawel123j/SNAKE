namespace ElegantSnake
{
    /// <summary>Rodzaj owocu — każdy daje inny efekt po zjedzeniu.</summary>
    public enum FoodType
    {
        /// <summary>Zwykły owoc: +10 punktów, lekkie przyspieszenie.</summary>
        Normal,

        /// <summary>Złoty bonus: +30 punktów.</summary>
        Bonus,

        /// <summary>Spowalniacz: +5 punktów i zwolnienie tempa gry.</summary>
        Slow
    }
}
