namespace SwiftPantry.Web.ViewModels;

public record DailySummary(
    int CaloriesConsumed,
    int CaloriesTarget,
    decimal ProteinConsumed,
    int ProteinTarget,
    decimal CarbsConsumed,
    int CarbsTarget,
    decimal FatConsumed,
    int FatTarget)
{
    public int CaloriesPct =>
        CaloriesTarget > 0
            ? Math.Min(100, (int)(CaloriesConsumed / (double)CaloriesTarget * 100))
            : 0;
    public bool CaloriesOver => CaloriesConsumed > CaloriesTarget;

    public int ProteinPct =>
        ProteinTarget > 0
            ? Math.Min(100, (int)((double)ProteinConsumed / ProteinTarget * 100))
            : 0;
    public bool ProteinOver => ProteinConsumed > ProteinTarget;

    public int CarbsPct =>
        CarbsTarget > 0
            ? Math.Min(100, (int)((double)CarbsConsumed / CarbsTarget * 100))
            : 0;
    public bool CarbsOver => CarbsConsumed > CarbsTarget;

    public int FatPct =>
        FatTarget > 0
            ? Math.Min(100, (int)((double)FatConsumed / FatTarget * 100))
            : 0;
    public bool FatOver => FatConsumed > FatTarget;
}
