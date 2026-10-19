namespace TableExporterApp
{
    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--self-check")
            {
                return SelfCheck();
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
            return 0;
        }

        static int SelfCheck()
        {
            var root = Path.Combine(Path.GetTempPath(), "table-exporter-self-check-" + Guid.NewGuid().ToString("N"));
            var source = Path.Combine(root, "excel");
            Directory.CreateDirectory(source);
            try
            {
                return RunSelfCheck(source, root);
            }
            finally
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, recursive: true);
                }
            }
        }

        static int RunSelfCheck(string source, string root)
        {
            var workbookPath = Path.Combine(source, "sample.xlsx");
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                var sheet = workbook.AddWorksheet("Card");
                sheet.Cell(1, 1).Value = "Id";
                sheet.Cell(1, 2).Value = "Name";
                sheet.Cell(2, 1).Value = "int";
                sheet.Cell(2, 2).Value = "string";
                sheet.Cell(3, 1).Value = "主键";
                sheet.Cell(4, 1).Value = 7;
                sheet.Cell(4, 2).Value = "火球";
                workbook.SaveAs(workbookPath);
            }

            var report = ExcelTableExporter.Export(root, source);
            var output = Path.Combine(ExcelTableExporter.GetTargetDirectory(root), "Card.bytes");
            if (report.Failed != 0 || report.Succeeded != 1 || !File.Exists(output))
            {
                return 1;
            }

            var table = CardTables.TableBinary.Read(File.ReadAllBytes(output));
            return table.GetString(7, "Name") == "火球" ? 0 : 1;
        }
    }
}
