using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("CodeTiger.Naming", "CT1702:Type names should use pascal casing.",
    Justification = "Unit test classes are allowed to contain underscores.")]
[assembly: SuppressMessage("Naming", "CA1707:Identifiers should not contain underscores",
    Justification = "Unit test classes are allowed to contain underscores.")]
[assembly: SuppressMessage("Design", "CA1034:Nested types should not be visible",
    Justification = "Unit test classes are allowed to be nested.")]
