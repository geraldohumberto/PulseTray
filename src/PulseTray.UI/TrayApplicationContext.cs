using PulseTray.Configuration;
using PulseTray.Core;
using PulseTray.Data;
using PulseTray.Notifications;

namespace PulseTray.UI;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly SettingsStore _settingsStore = new();
    private readonly IQueryExecutor _queryExecutor = new MySqlQueryExecutor();
    private readonly AlertSoundPlayer _alertSoundPlayer = new();
    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly NotifyIcon _notifyIcon;
    private readonly HashSet<string> _activeAlerts = [];
    private PulseTraySettings _settings;
    private bool _isRefreshing;

    public TrayApplicationContext()
    {
        _settings = _settingsStore.Load();

        var menu = new ContextMenuStrip();
        menu.Items.Add("Atualizar agora", null, async (_, _) => await RefreshAsync());
        menu.Items.Add("Configurar", null, (_, _) => OpenSettings());
        menu.Items.Add("Sair", null, (_, _) => Exit());

        _notifyIcon = new NotifyIcon
        {
            Text = "PulseTray",
            Icon = TrayIconFactory.Create([]),
            ContextMenuStrip = menu,
            Visible = true
        };
        _notifyIcon.DoubleClick += (_, _) => OpenSettings();

        ConfigureTimer();
        _ = RefreshAsync();
    }

    private void ConfigureTimer()
    {
        _timer.Stop();
        _timer.Interval = Math.Max(1, _settings.RefreshMinutes) * 60 * 1000;
        _timer.Tick -= TimerTickAsync;
        _timer.Tick += TimerTickAsync;
        _timer.Start();
    }

    private async void TimerTickAsync(object? sender, EventArgs eventArgs)
    {
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        if (_isRefreshing)
        {
            return;
        }

        _isRefreshing = true;
        try
        {
            var results = new List<QueryResult>();
            foreach (var query in _settings.EnabledQueries())
            {
                results.Add(await _queryExecutor.ExecuteCountAsync(_settings.Database, query));
            }

            SetTrayIcon(results);
            NotifyAlerts(results);
        }
        catch (Exception ex)
        {
            _notifyIcon.Icon = TrayIconFactory.Create([], hasError: true);
            _notifyIcon.Text = "PulseTray - erro ao consultar";
            _notifyIcon.ShowBalloonTip(5000, "PulseTray", $"Erro ao consultar banco: {ex.Message}", ToolTipIcon.Error);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void SetTrayIcon(IReadOnlyCollection<QueryResult> results)
    {
        _notifyIcon.Icon = TrayIconFactory.Create(results);
        _notifyIcon.Text = BuildTooltip(results);
    }

    private static string BuildTooltip(IReadOnlyCollection<QueryResult> results)
    {
        if (results.Count == 0)
        {
            return "PulseTray - nenhuma query ativa";
        }

        var lines = results.Select(result => $"{result.Query.Name}: {result.Value}");
        var tooltip = "PulseTray\n" + string.Join("\n", lines);
        return tooltip.Length > 63 ? tooltip[..63] : tooltip;
    }

    private void NotifyAlerts(IEnumerable<QueryResult> results)
    {
        foreach (var result in results)
        {
            var alertKey = result.Query.Name;
            if (result.IsAlert && _activeAlerts.Add(alertKey))
            {
                _alertSoundPlayer.PlayAlert();
                _notifyIcon.ShowBalloonTip(
                    5000,
                    $"PulseTray - {result.Query.Name}",
                    $"Valor {result.Value} acima do limite {result.Query.AlertLimit}.",
                    ToolTipIcon.Warning);
            }

            if (!result.IsAlert)
            {
                _activeAlerts.Remove(alertKey);
            }
        }
    }

    private void OpenSettings()
    {
        using var form = new SettingsForm(_settingsStore, _queryExecutor);
        if (form.ShowDialog() == DialogResult.OK)
        {
            _settings = _settingsStore.Load();
            ConfigureTimer();
            _ = RefreshAsync();
        }
    }

    private void Exit()
    {
        _timer.Stop();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        Application.Exit();
    }
}
