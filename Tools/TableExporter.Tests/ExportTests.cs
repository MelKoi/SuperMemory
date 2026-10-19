using CardTables;
using ClosedXML.Excel;
using TableExporterApp;
using Xunit;

namespace TableExporter.Tests;

public class ExportTests
{
    [Fact]
    public void Export_ReadsCurrentFolderAndKeepsBoundaryRules()
    {
        var root = CreateRoot();
        var source = Path.Combine(root, "excel源");
        Directory.CreateDirectory(source);
        Directory.CreateDirectory(Path.Combine(source, "子文件夹"));
        var target = ExcelTableExporter.GetTargetDirectory(root);
        Directory.CreateDirectory(target);
        File.WriteAllText(Path.Combine(target, "Old.bytes"), "keep-me");
        File.WriteAllText(Path.Combine(target, "Broken.bytes"), "keep-broken");
        File.WriteAllText(Path.Combine(target, "Dup.bytes"), "keep-dup");

        CreateWorkbook(Path.Combine(source, "book.xlsx"));
        CreateExtra(Path.Combine(source, "extra.xlsx"));
        CreateHidden(Path.Combine(source, "子文件夹", "nope.xlsx"));
        File.WriteAllText(Path.Combine(source, "notes.txt"), "ignore");
        File.WriteAllBytes(Path.Combine(source, "~$temp.xlsx"), new byte[] { 1, 2, 3 });
        var lockedPath = Path.Combine(source, "locked.xlsx");
        CreateLockedWorkbook(lockedPath);

        using var locked = new FileStream(lockedPath, FileMode.Open, FileAccess.Read, FileShare.None);
        var report = ExcelTableExporter.Export(root, source);

        Assert.Equal(5, report.Succeeded);
        Assert.Equal(3, report.Failed);
        Assert.Equal("keep-me", File.ReadAllText(Path.Combine(target, "Old.bytes")));
        Assert.Equal("keep-broken", File.ReadAllText(Path.Combine(target, "Broken.bytes")));
        Assert.Equal("keep-dup", File.ReadAllText(Path.Combine(target, "Dup.bytes")));
        Assert.False(File.Exists(Path.Combine(target, "说明.bytes")));
        Assert.False(File.Exists(Path.Combine(target, "Hidden.bytes")));

        var card = TableBinary.Read(File.ReadAllBytes(Path.Combine(target, "卡牌.bytes")));
        Assert.Equal("卡牌", card.Name);
        Assert.Equal(2, card.RowCount);
        Assert.False(card.Contains(9999));
        Assert.Equal("火球", card.GetString(1001, "Name"));
        Assert.Equal(1.5f, card.GetFloat(1001, "Attack"));
        Assert.Equal(0.25d, card.GetDouble(1001, "Ratio"));
        Assert.Equal(9000000000L, card.GetLong(1001, "Hp"));
        Assert.True(card.GetBool(1001, "Enabled"));
        Assert.Equal(new[] { "火", "球" }, card.GetStringArray(1001, "Tags"));
        Assert.Equal(new[] { 3, 4 }, card.GetIntArray(1001, "Damages"));
        Assert.Equal(new[] { true, false }, card.GetBoolArray(1001, "Flags"));
        Assert.Equal(string.Empty, card.GetString(1002, "Name"));
        Assert.False(card.GetBool(1002, "Enabled"));
        Assert.Empty(card.GetStringArray(1002, "Tags"));
        Assert.Equal("2024-05-01", card.GetString(1002, "Released"));

        var hero = TableBinary.Read(File.ReadAllBytes(Path.Combine(target, "Hero.bytes")));
        Assert.Equal("勇者", hero.GetString("hero_01", "Name"));

        var merge = TableBinary.Read(File.ReadAllBytes(Path.Combine(target, "Merge.bytes")));
        Assert.Equal(2, merge.Fields.Count);
        Assert.Equal("Title", merge.Fields[1].Name);
        Assert.Equal("hello", merge.GetString(1, "Title"));

        var empty = TableBinary.Read(File.ReadAllBytes(Path.Combine(target, "EmptyOk.bytes")));
        Assert.Equal(0, empty.RowCount);
        Assert.Contains(report.Lines, line => line.Contains("locked.xlsx", StringComparison.Ordinal));
        Assert.Contains(report.Lines, line => line.Contains("Id 重复", StringComparison.Ordinal));
        Assert.Contains(report.Lines, line => line.Contains("类型无法识别", StringComparison.Ordinal));
    }

    [Fact]
    public void Export_RejectsMissingFolderAndOverlappingTarget()
    {
        var root = CreateRoot();
        var missing = ExcelTableExporter.Export(root, Path.Combine(root, "missing"));
        Assert.Equal(0, missing.Succeeded);
        Assert.Equal(1, missing.Failed);

        var source = ExcelTableExporter.GetTargetDirectory(root);
        Directory.CreateDirectory(source);
        var overlap = ExcelTableExporter.Export(root, source);
        Assert.Equal(0, overlap.Succeeded);
        Assert.Contains(overlap.Lines, line => line.Contains("不能与目标文件夹相同", StringComparison.Ordinal));
    }

    [Fact]
    public void Export_IgnoresSubfoldersWhenFolderHasNoTopLevelWorkbook()
    {
        var root = CreateRoot();
        var source = Path.Combine(root, "excel");
        Directory.CreateDirectory(Path.Combine(source, "nested"));
        CreateExtra(Path.Combine(source, "nested", "extra.xlsx"));

        var report = ExcelTableExporter.Export(root, source);

        Assert.Equal(0, report.Succeeded);
        Assert.Equal(0, report.Failed);
        Assert.Contains(report.Lines, line => line.Contains("没有找到 .xlsx", StringComparison.Ordinal));
    }

    [Fact]
    public void TryFindProjectRoot_WalksParents()
    {
        var root = CreateRoot();
        var nested = Path.Combine(root, "Tools", "导表工具");
        Directory.CreateDirectory(nested);

        Assert.True(ExcelTableExporter.TryFindProjectRoot(nested, out var found, out _));
        Assert.Equal(Path.GetFullPath(root), Path.GetFullPath(found));
        Assert.False(ExcelTableExporter.TryFindProjectRoot(Path.GetTempPath(), out _, out _));
    }

    static string CreateRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "table-export-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(root, "Assets"));
        Directory.CreateDirectory(Path.Combine(root, "ProjectSettings"));
        return root;
    }

    static void CreateWorkbook(string path)
    {
        using var workbook = new XLWorkbook();
        var card = workbook.AddWorksheet("卡牌");
        card.Cell(1, 1).Value = "Id";
        card.Cell(1, 2).Value = "Name";
        card.Cell(1, 3).Value = "Attack";
        card.Cell(1, 4).Value = "Ratio";
        card.Cell(1, 5).Value = "Hp";
        card.Cell(1, 6).Value = "Enabled";
        card.Cell(1, 7).Value = "Tags";
        card.Cell(1, 8).Value = "Damages";
        card.Cell(1, 9).Value = "Flags";
        card.Cell(1, 10).Value = "Released";
        card.Cell(2, 1).Value = "int";
        card.Cell(2, 2).Value = "string";
        card.Cell(2, 3).Value = "float";
        card.Cell(2, 4).Value = "double";
        card.Cell(2, 5).Value = "long";
        card.Cell(2, 6).Value = "bool";
        card.Cell(2, 7).Value = "string[]";
        card.Cell(2, 8).Value = "int[]";
        card.Cell(2, 9).Value = "bool[]";
        card.Cell(2, 10).Value = "string";
        card.Cell(3, 1).Value = "主键";
        card.Cell(3, 2).Value = 9999;
        card.Cell(4, 1).Value = 1001;
        card.Cell(4, 2).Value = "火球";
        card.Cell(4, 3).Value = 1.5;
        card.Cell(4, 4).Value = 0.25;
        card.Cell(4, 5).Value = 9000000000d;
        card.Cell(4, 6).Value = "是";
        card.Cell(4, 7).Value = "火|球";
        card.Cell(4, 8).Value = "3|4|";
        card.Cell(4, 9).Value = "是|否";
        card.Cell(6, 1).Value = "1002";
        card.Cell(6, 6).Value = "false";
        card.Cell(6, 10).Value = new DateTime(2024, 5, 1);

        var hero = workbook.AddWorksheet("Hero");
        hero.Cell(1, 1).Value = "Id";
        hero.Cell(1, 2).Value = "Name";
        hero.Cell(2, 1).Value = "string";
        hero.Cell(2, 2).Value = "string";
        hero.Cell(3, 1).Value = "注释";
        hero.Cell(4, 1).Value = "hero_01";
        hero.Cell(4, 2).Value = "勇者";

        var note = workbook.AddWorksheet("#说明");
        note.Cell(1, 1).Value = "Id";
        note.Cell(2, 1).Value = "int";
        note.Cell(4, 1).Value = 1;

        var broken = workbook.AddWorksheet("Broken");
        broken.Cell(1, 1).Value = "Id";
        broken.Cell(1, 2).Value = "Value";
        broken.Cell(2, 1).Value = "int";
        broken.Cell(2, 2).Value = "nope";
        broken.Cell(4, 1).Value = 1;

        var dup = workbook.AddWorksheet("Dup");
        dup.Cell(1, 1).Value = "Id";
        dup.Cell(2, 1).Value = "int";
        dup.Cell(3, 1).Value = "注释";
        dup.Cell(4, 1).Value = 1;
        dup.Cell(5, 1).Value = 1;

        var empty = workbook.AddWorksheet("EmptyOk");
        empty.Cell(1, 1).Value = "Id";
        empty.Cell(1, 2).Value = "Name";
        empty.Cell(2, 1).Value = "int";
        empty.Cell(2, 2).Value = "string";
        empty.Cell(3, 1).Value = "注释";

        var merge = workbook.AddWorksheet("Merge");
        merge.Cell(1, 1).Value = "Id";
        merge.Cell(1, 2).Value = "Title";
        merge.Range(1, 2, 1, 3).Merge();
        merge.Cell(2, 1).Value = "int";
        merge.Cell(2, 2).Value = "string";
        merge.Cell(3, 1).Value = "注释";
        merge.Cell(4, 1).Value = 1;
        merge.Cell(4, 2).Value = "hello";

        workbook.SaveAs(path);
    }

    static void CreateExtra(string path)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Extra");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(1, 2).Value = "Name";
        sheet.Cell(2, 1).Value = "int";
        sheet.Cell(2, 2).Value = "string";
        sheet.Cell(3, 1).Value = "注释";
        sheet.Cell(4, 1).Value = 8;
        sheet.Cell(4, 2).Value = "额外";
        workbook.SaveAs(path);
    }

    static void CreateHidden(string path)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Hidden");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(2, 1).Value = "int";
        sheet.Cell(4, 1).Value = 1;
        workbook.SaveAs(path);
    }

    static void CreateLockedWorkbook(string path)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet("Locked");
        sheet.Cell(1, 1).Value = "Id";
        sheet.Cell(2, 1).Value = "int";
        sheet.Cell(4, 1).Value = 1;
        workbook.SaveAs(path);
    }
}
