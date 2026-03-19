namespace SwiftPantry.PlaywrightTests;

/// <summary>
/// Assembly-level setup fixture. Starts a single Kestrel server on port 5099
/// for the entire test run, shared across all test classes.
/// Microsoft.Playwright.NUnit enables parallel-by-fixtures by default;
/// all test classes use [NonParallelizable] to avoid port/DB conflicts.
/// </summary>
[SetUpFixture]
public class TestSetup
{
    public static readonly PlaywrightFixture Fixture = new();

    [OneTimeSetUp]
    public void RunBeforeAnyTests() => Fixture.CreateClient();

    [OneTimeTearDown]
    public void RunAfterAllTests() => Fixture.Dispose();
}
