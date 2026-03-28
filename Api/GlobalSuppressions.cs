using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Usage", "CA1515:Consider making public types internal", Justification = "Application layer types need to be public")]
[assembly: SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "DTOs commonly use List<T>")]
[assembly: SuppressMessage("Globalization", "CA1305:Specify IFormatProvider", Justification = "Not required for this application")]
[assembly: SuppressMessage("Globalization", "CA1307:Specify StringComparison", Justification = "Ordinal comparison is implicit")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Instance methods preferred for testability")]
[assembly: SuppressMessage("Performance", "CA1859:Use concrete types when possible for improved performance", Justification = "Interface usage preferred for abstraction")]
[assembly: SuppressMessage("Naming", "CA1725:Parameter names should match base declaration", Justification = "Shorter parameter names preferred in implementation")]
