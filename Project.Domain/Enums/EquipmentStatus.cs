namespace Project.Domain.Enums;

public static class EquipmentStatus
{
    public const string Active = "A";
    public const string OutForService = "O";
    public const string Scrapped = "S";

    public static readonly string[] All = [Active, OutForService, Scrapped];
}