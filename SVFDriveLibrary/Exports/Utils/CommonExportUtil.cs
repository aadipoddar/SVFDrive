using Syncfusion.Drawing;

namespace SVFDriveLibrary.Exports.Utils;

public enum ReportExportType
{
    PDF,
    Excel
}

public enum CellAlignment
{
    Left,
    Center,
    Right
}

/// <summary>
/// Column configuration for report exports (unified for PDF and Excel)
/// </summary>
public class ReportColumnSetting
{
    public string DisplayName { get; set; }
    public CellAlignment Alignment { get; set; } = CellAlignment.Right;
    public double? PDFWidth { get; set; }
    public double? ExcelWidth { get; set; } = 15;
    public string Format { get; set; }
    public bool IncludeInTotal { get; set; } = true;
    public bool HighlightNegative { get; set; } = false;
    public bool IsRequired { get; set; } = false;
    public bool IsGrandTotal { get; set; } = false;
    public Func<object, ReportFormatInfo> FormatCallback { get; set; }
}

/// <summary>
/// Format information for report cell formatting
/// </summary>
public class ReportFormatInfo
{
    public Color? FontColor { get; set; }
    public bool Bold { get; set; }
    public string FormattedText { get; set; }
}