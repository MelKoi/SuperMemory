using System.Text.Json;

namespace TableExporterApp
{
    sealed class MainForm : Form
    {
        readonly TextBox sourceBox = new TextBox();
        readonly TextBox targetBox = new TextBox();
        readonly TextBox logBox = new TextBox();
        readonly Button browseButton = new Button();
        readonly Button exportButton = new Button();
        readonly string projectRoot;
        readonly string configPath;
        readonly string logPath;

        public MainForm()
        {
            Text = "导表工具";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(720, 480);
            ClientSize = new Size(860, 560);
            Font = new Font("Microsoft YaHei UI", 9f);

            var top = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 148,
                Padding = new Padding(12, 12, 12, 8),
                ColumnCount = 3,
                RowCount = 4,
            };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
            top.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            top.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            top.Controls.Add(new Label { Text = "源文件夹", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            sourceBox.Dock = DockStyle.Fill;
            sourceBox.Margin = new Padding(0, 4, 8, 4);
            top.Controls.Add(sourceBox, 1, 0);
            browseButton.Text = "浏览...";
            browseButton.Dock = DockStyle.Fill;
            browseButton.Margin = new Padding(0, 2, 0, 2);
            browseButton.Click += BrowseClicked;
            top.Controls.Add(browseButton, 2, 0);

            top.Controls.Add(new Label { Text = "目标文件夹", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
            targetBox.Dock = DockStyle.Fill;
            targetBox.Margin = new Padding(0, 4, 0, 4);
            targetBox.ReadOnly = true;
            targetBox.TabStop = false;
            targetBox.BackColor = SystemColors.Control;
            top.SetColumnSpan(targetBox, 2);
            top.Controls.Add(targetBox, 1, 1);

            exportButton.Text = "导出全部";
            exportButton.Dock = DockStyle.Left;
            exportButton.Width = 120;
            exportButton.Margin = new Padding(0, 4, 0, 4);
            exportButton.Click += ExportClicked;
            top.Controls.Add(exportButton, 1, 2);

            var hint = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                ForeColor = SystemColors.GrayText,
                Text = "只读取源文件夹这一层的 .xlsx。第 1 行字段名，第 2 行类型，第 3 行注释，第 4 行起是数据。每个工作表一张表，必须有 Id 列。目标文件夹固定，不能修改。",
            };
            top.SetColumnSpan(hint, 3);
            top.Controls.Add(hint, 0, 3);

            logBox.Multiline = true;
            logBox.ReadOnly = true;
            logBox.ScrollBars = ScrollBars.Vertical;
            logBox.Dock = DockStyle.Fill;
            logBox.Font = new Font("Microsoft YaHei UI", 9f);
            var logHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 0, 12, 12) };
            logHost.Controls.Add(logBox);

            Controls.Add(logHost);
            Controls.Add(top);
            AcceptButton = exportButton;

            if (!ExcelTableExporter.TryFindProjectRoot(AppContext.BaseDirectory, out projectRoot, out var error))
            {
                projectRoot = string.Empty;
                configPath = string.Empty;
                logPath = string.Empty;
                browseButton.Enabled = false;
                exportButton.Enabled = false;
                logBox.Text = error;
                return;
            }

            configPath = Path.Combine(projectRoot, "Tools", "table-exporter.config.json");
            logPath = Path.Combine(projectRoot, "Tools", "last-export.log");
            targetBox.Text = ExcelTableExporter.GetTargetDirectory(projectRoot);
            sourceBox.Text = LoadSourceDirectory(configPath);
            logBox.Text = "选择源文件夹后，点击“导出全部”。";
        }

        void BrowseClicked(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择存放 Excel 的文件夹",
                UseDescriptionForTitle = true,
                SelectedPath = Directory.Exists(sourceBox.Text) ? sourceBox.Text : projectRoot,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                sourceBox.Text = dialog.SelectedPath;
                SaveSourceDirectory(configPath, sourceBox.Text);
            }
        }

        async void ExportClicked(object? sender, EventArgs e)
        {
            browseButton.Enabled = false;
            exportButton.Enabled = false;
            logBox.Text = "正在导出...";
            try
            {
                var source = sourceBox.Text.Trim();
                SaveSourceDirectory(configPath, source);
                var root = projectRoot;
                var report = await Task.Run(() => ExcelTableExporter.Export(root, source));
                var text = string.Join(Environment.NewLine, report.Lines);
                logBox.Text = text;
                Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
                await File.WriteAllTextAsync(logPath, text);
            }
            catch (Exception exception)
            {
                logBox.Text = "导出失败：" + exception.Message;
            }
            finally
            {
                browseButton.Enabled = true;
                exportButton.Enabled = true;
            }
        }

        static string LoadSourceDirectory(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    return string.Empty;
                }

                var settings = JsonSerializer.Deserialize<ExporterSettings>(File.ReadAllText(path));
                return settings?.SourceDirectory ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        static void SaveSourceDirectory(string path, string sourceDirectory)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var json = JsonSerializer.Serialize(new ExporterSettings { SourceDirectory = sourceDirectory }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        sealed class ExporterSettings
        {
            public string SourceDirectory { get; set; } = string.Empty;
        }
    }
}
