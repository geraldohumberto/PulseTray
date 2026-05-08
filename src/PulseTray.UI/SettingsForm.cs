using PulseTray.Configuration;
using PulseTray.Core;
using PulseTray.Data;

namespace PulseTray.UI;

internal sealed class SettingsForm : Form
{
    private readonly SettingsStore _settingsStore;
    private readonly IQueryExecutor _queryExecutor;
    private readonly PulseTraySettings _settings;
    private readonly TextBox _hostText = new();
    private readonly NumericUpDown _portNumber = new();
    private readonly TextBox _userText = new();
    private readonly TextBox _passwordText = new();
    private readonly TextBox _databaseText = new();
    private readonly NumericUpDown _refreshMinutes = new();
    private readonly List<QueryControls> _queryControls = [];

    public SettingsForm(SettingsStore settingsStore, IQueryExecutor queryExecutor)
    {
        _settingsStore = settingsStore;
        _queryExecutor = queryExecutor;
        _settings = settingsStore.Load();

        Text = "PulseTray - Configuracao";
        Width = 820;
        Height = 620;
        MinimumSize = new Size(720, 520);
        StartPosition = FormStartPosition.CenterScreen;

        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(CreateDatabaseTab());
        tabs.TabPages.Add(CreateQueriesTab());

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 48,
            Padding = new Padding(8)
        };

        var saveButton = new Button { Text = "Salvar", Width = 96 };
        saveButton.Click += (_, _) => Save();
        var cancelButton = new Button { Text = "Cancelar", Width = 96 };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(cancelButton);
        Controls.Add(tabs);
        Controls.Add(buttons);
    }

    private TabPage CreateDatabaseTab()
    {
        var page = new TabPage("Banco de Dados");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            RowCount = 0,
            Padding = new Padding(16),
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        _hostText.Text = _settings.Database.Host;
        _portNumber.Minimum = 1;
        _portNumber.Maximum = 65535;
        _portNumber.Value = Math.Clamp(_settings.Database.Port, 1, 65535);
        _userText.Text = _settings.Database.User;
        _passwordText.Text = _settings.Database.Password;
        _passwordText.UseSystemPasswordChar = true;
        _databaseText.Text = _settings.Database.Database;
        _refreshMinutes.Minimum = 1;
        _refreshMinutes.Maximum = 1440;
        _refreshMinutes.Value = Math.Max(1, _settings.RefreshMinutes);

        AddRow(layout, "Host", _hostText);
        AddRow(layout, "Porta", _portNumber);
        AddRow(layout, "Usuario", _userText);
        AddRow(layout, "Senha", _passwordText);
        AddRow(layout, "Database", _databaseText);
        AddRow(layout, "Atualizacao (min)", _refreshMinutes);

        var testButton = new Button { Text = "Testar Conexao", Width = 140 };
        testButton.Click += async (_, _) => await TestConnectionAsync();
        var testRow = layout.RowCount;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.Controls.Add(testButton, 1, testRow);
        layout.RowCount++;

        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateQueriesTab()
    {
        var page = new TabPage("Queries");
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12)
        };

        foreach (var query in _settings.Queries.Take(5))
        {
            var group = new GroupBox
            {
                Text = query.Name,
                Width = 740,
                Height = 155,
                Padding = new Padding(10)
            };

            var enabled = new CheckBox { Text = "Ativa", Checked = query.Enabled, Width = 80 };
            var name = new TextBox { Text = query.Name, Width = 230 };
            var limit = new NumericUpDown { Minimum = 0, Maximum = 999999, Value = Math.Max(0, query.AlertLimit), Width = 90 };
            var sql = new TextBox
            {
                Text = query.Sql,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Width = 690,
                Height = 72,
                Font = new Font("Consolas", 9)
            };

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36 };
            top.Controls.Add(enabled);
            top.Controls.Add(new Label { Text = "Nome", AutoSize = true, Padding = new Padding(10, 7, 0, 0) });
            top.Controls.Add(name);
            top.Controls.Add(new Label { Text = "Limite", AutoSize = true, Padding = new Padding(10, 7, 0, 0) });
            top.Controls.Add(limit);

            group.Controls.Add(sql);
            group.Controls.Add(top);
            sql.Top = 48;
            sql.Left = 12;

            _queryControls.Add(new QueryControls(enabled, name, sql, limit));
            panel.Controls.Add(group);
        }

        page.Controls.Add(panel);
        return page;
    }

    private static void AddRow(TableLayoutPanel layout, string label, Control control)
    {
        var row = layout.RowCount;
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
        control.Dock = DockStyle.Fill;
        layout.Controls.Add(control, 1, row);
        layout.RowCount++;
    }

    private async Task TestConnectionAsync()
    {
        try
        {
            await _queryExecutor.TestConnectionAsync(ReadDatabaseSettings());
            MessageBox.Show(this, "Conexao realizada com sucesso.", "PulseTray", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Falha na conexao", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void Save()
    {
        _settings.Database = ReadDatabaseSettings();
        _settings.RefreshMinutes = (int)_refreshMinutes.Value;
        _settings.Queries = _queryControls.Select(controls => new QueryDefinition
        {
            Enabled = controls.Enabled.Checked,
            Name = string.IsNullOrWhiteSpace(controls.Name.Text) ? "Query" : controls.Name.Text.Trim(),
            Sql = controls.Sql.Text.Trim(),
            AlertLimit = (int)controls.Limit.Value
        }).ToList();

        _settingsStore.Save(_settings);
        DialogResult = DialogResult.OK;
    }

    private DatabaseSettings ReadDatabaseSettings()
    {
        return new DatabaseSettings
        {
            Host = _hostText.Text.Trim(),
            Port = (int)_portNumber.Value,
            User = _userText.Text.Trim(),
            Password = _passwordText.Text,
            Database = _databaseText.Text.Trim()
        };
    }

    private sealed record QueryControls(CheckBox Enabled, TextBox Name, TextBox Sql, NumericUpDown Limit);
}
