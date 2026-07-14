using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace QuickAudioSwitcher;

/// <summary>
/// Manages UI localization. Strings are loaded from embedded JSON resources.
/// </summary>
internal class LanguageManager
{
    private static readonly Dictionary<string, Dictionary<string, string>> _languages = new()
    {
        ["en"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 Audio Devices",
            ["Settings"] = "⚙ Settings...",
            ["AutoStart"] = "✅ Auto-start",
            ["AutoStartOff"] = "⬜ Auto-start",
            ["AutoStartCheckBox"] = "Auto-start",
            ["Exit"] = "✕ Exit",
            ["NoDevices"] = "🔊 No active devices",
            ["SettingsTitle"] = "⚙ Audio Switcher Settings",
            ["Hotkey"] = "Hotkey",
            ["Key"] = "Key:",
            ["QuickSwitch"] = "Quick Switch:",
            ["Save"] = "Save",
            ["Cancel"] = "Cancel",
            ["Language"] = "Language:",
            ["Error"] = "Error",
            ["SelectModifier"] = "Select at least one modifier (Ctrl, Alt, Shift or Win).",
            ["SwitchError"] = "❌ Switch failed",
        },
        ["ru"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 Устройства вывода",
            ["Settings"] = "⚙ Настройки...",
            ["AutoStart"] = "✅ Автозапуск",
            ["AutoStartOff"] = "⬜ Автозапуск",
            ["AutoStartCheckBox"] = "Автозапуск",
            ["Exit"] = "✕ Выход",
            ["NoDevices"] = "🔊 Нет активных устройств",
            ["SettingsTitle"] = "⚙ Настройки Audio Switcher",
            ["Hotkey"] = "Горячая клавиша",
            ["Key"] = "Клавиша:",
            ["QuickSwitch"] = "Быстрое переключение:",
            ["Save"] = "Сохранить",
            ["Cancel"] = "Отмена",
            ["Language"] = "Язык:",
            ["Error"] = "Ошибка",
            ["SelectModifier"] = "Выберите хотя бы один модификатор (Ctrl, Alt, Shift или Win).",
            ["SwitchError"] = "❌ Ошибка переключения",
        },
        ["de"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 Audiogeräte",
            ["Settings"] = "⚙ Einstellungen...",
            ["AutoStart"] = "✅ Autostart",
            ["AutoStartOff"] = "⬜ Autostart",
            ["AutoStartCheckBox"] = "Autostart",
            ["Exit"] = "✕ Beenden",
            ["NoDevices"] = "🔊 Keine aktiven Geräte",
            ["SettingsTitle"] = "⚙ Audio Switcher Einstellungen",
            ["Hotkey"] = "Tastenkombination",
            ["Key"] = "Taste:",
            ["QuickSwitch"] = "Schnellwechsel:",
            ["Save"] = "Speichern",
            ["Cancel"] = "Abbrechen",
            ["Language"] = "Sprache:",
            ["Error"] = "Fehler",
            ["SelectModifier"] = "Wählen Sie mindestens einen Modifikator (Ctrl, Alt, Shift oder Win).",
            ["SwitchError"] = "❌ Wechsel fehlgeschlagen",
        },
        ["fr"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 Périphériques audio",
            ["Settings"] = "⚙ Paramètres...",
            ["AutoStart"] = "✅ Démarrage auto",
            ["AutoStartOff"] = "⬜ Démarrage auto",
            ["AutoStartCheckBox"] = "Démarrage auto",
            ["Exit"] = "✕ Quitter",
            ["NoDevices"] = "🔊 Aucun périphérique actif",
            ["SettingsTitle"] = "⚙ Paramètres Audio Switcher",
            ["Hotkey"] = "Raccourci clavier",
            ["Key"] = "Touche:",
            ["QuickSwitch"] = "Changement rapide:",
            ["Save"] = "Enregistrer",
            ["Cancel"] = "Annuler",
            ["Language"] = "Langue:",
            ["Error"] = "Erreur",
            ["SelectModifier"] = "Sélectionnez au moins un modificateur (Ctrl, Alt, Shift ou Win).",
            ["SwitchError"] = "❌ Échec de la commutation",
        },
        ["es"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 Dispositivos de audio",
            ["Settings"] = "⚙ Ajustes...",
            ["AutoStart"] = "✅ Inicio automático",
            ["AutoStartOff"] = "⬜ Inicio automático",
            ["AutoStartCheckBox"] = "Inicio automático",
            ["Exit"] = "✕ Salir",
            ["NoDevices"] = "🔊 Sin dispositivos activos",
            ["SettingsTitle"] = "⚙ Ajustes de Audio Switcher",
            ["Hotkey"] = "Atajo de teclado",
            ["Key"] = "Tecla:",
            ["QuickSwitch"] = "Cambio rápido:",
            ["Save"] = "Guardar",
            ["Cancel"] = "Cancelar",
            ["Language"] = "Idioma:",
            ["Error"] = "Error",
            ["SelectModifier"] = "Seleccione al menos un modificador (Ctrl, Alt, Shift o Win).",
            ["SwitchError"] = "❌ Error al cambiar",
        },
        ["pt"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 Dispositivos de áudio",
            ["Settings"] = "⚙ Configurações...",
            ["AutoStart"] = "✅ Início automático",
            ["AutoStartOff"] = "⬜ Início automático",
            ["AutoStartCheckBox"] = "Início automático",
            ["Exit"] = "✕ Sair",
            ["NoDevices"] = "🔊 Nenhum dispositivo ativo",
            ["SettingsTitle"] = "⚙ Configurações do Audio Switcher",
            ["Hotkey"] = "Atalho de teclado",
            ["Key"] = "Tecla:",
            ["QuickSwitch"] = "Troca rápida:",
            ["Save"] = "Salvar",
            ["Cancel"] = "Cancelar",
            ["Language"] = "Idioma:",
            ["Error"] = "Erro",
            ["SelectModifier"] = "Selecione pelo menos um modificador (Ctrl, Alt, Shift ou Win).",
            ["SwitchError"] = "❌ Falha ao alternar",
        },
        ["it"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 Dispositivi audio",
            ["Settings"] = "⚙ Impostazioni...",
            ["AutoStart"] = "✅ Avvio automatico",
            ["AutoStartOff"] = "⬜ Avvio automatico",
            ["AutoStartCheckBox"] = "Avvio automatico",
            ["Exit"] = "✕ Esci",
            ["NoDevices"] = "🔊 Nessun dispositivo attivo",
            ["SettingsTitle"] = "⚙ Impostazioni Audio Switcher",
            ["Hotkey"] = "Scorciatoia da tastiera",
            ["Key"] = "Tasto:",
            ["QuickSwitch"] = "Cambio rapido:",
            ["Save"] = "Salva",
            ["Cancel"] = "Annulla",
            ["Language"] = "Lingua:",
            ["Error"] = "Errore",
            ["SelectModifier"] = "Seleziona almeno un modificatore (Ctrl, Alt, Shift o Win).",
            ["SwitchError"] = "❌ Commutazione fallita",
        },
        ["zh"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 音频设备",
            ["Settings"] = "⚙ 设置...",
            ["AutoStart"] = "✅ 自动启动",
            ["AutoStartOff"] = "⬜ 自动启动",
            ["AutoStartCheckBox"] = "自动启动",
            ["Exit"] = "✕ 退出",
            ["NoDevices"] = "🔊 无活动设备",
            ["SettingsTitle"] = "⚙ Audio Switcher 设置",
            ["Hotkey"] = "快捷键",
            ["Key"] = "按键:",
            ["QuickSwitch"] = "快速切换:",
            ["Save"] = "保存",
            ["Cancel"] = "取消",
            ["Language"] = "语言:",
            ["Error"] = "错误",
            ["SelectModifier"] = "请至少选择一个修饰键（Ctrl、Alt、Shift 或 Win）。",
            ["SwitchError"] = "❌ 切换失败",
        },
        ["ja"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 オーディオデバイス",
            ["Settings"] = "⚙ 設定...",
            ["AutoStart"] = "✅ 自動起動",
            ["AutoStartOff"] = "⬜ 自動起動",
            ["AutoStartCheckBox"] = "自動起動",
            ["Exit"] = "✕ 終了",
            ["NoDevices"] = "🔊 アクティブなデバイスがありません",
            ["SettingsTitle"] = "⚙ Audio Switcher 設定",
            ["Hotkey"] = "ホットキー",
            ["Key"] = "キー:",
            ["QuickSwitch"] = "クイック切り替え:",
            ["Save"] = "保存",
            ["Cancel"] = "キャンセル",
            ["Language"] = "言語:",
            ["Error"] = "エラー",
            ["SelectModifier"] = "少なくとも1つの修飾キーを選択してください（Ctrl、Alt、Shift、Win）。",
            ["SwitchError"] = "❌ 切り替えに失敗しました",
        },
        ["ko"] = new()
        {
            ["AppTitle"] = "Audio Switcher",
            ["Devices"] = "🔊 오디오 장치",
            ["Settings"] = "⚙ 설정...",
            ["AutoStart"] = "✅ 자동 시작",
            ["AutoStartOff"] = "⬜ 자동 시작",
            ["AutoStartCheckBox"] = "자동 시작",
            ["Exit"] = "✕ 종료",
            ["NoDevices"] = "🔊 활성 장치 없음",
            ["SettingsTitle"] = "⚙ Audio Switcher 설정",
            ["Hotkey"] = "단축키",
            ["Key"] = "키:",
            ["QuickSwitch"] = "빠른 전환:",
            ["Save"] = "저장",
            ["Cancel"] = "취소",
            ["Language"] = "언어:",
            ["Error"] = "오류",
            ["SelectModifier"] = "수정자(Ctrl, Alt, Shift, Win)를 하나 이상 선택하세요.",
            ["SwitchError"] = "❌ 전환 실패",
        },
    };

    private static readonly Dictionary<string, string> _languageNames = new()
    {
        ["en"] = "🇬🇧 English",
        ["ru"] = "🇷🇺 Русский",
        ["de"] = "🇩🇪 Deutsch",
        ["fr"] = "🇫🇷 Français",
        ["es"] = "🇪🇸 Español",
        ["pt"] = "🇵🇹 Português",
        ["it"] = "🇮🇹 Italiano",
        ["zh"] = "🇨🇳 中文",
        ["ja"] = "🇯🇵 日本語",
        ["ko"] = "🇰🇷 한국어",
    };

    private static LanguageManager? _instance;
    private Dictionary<string, string> _strings;

    public static LanguageManager Instance
    {
        get
        {
            _instance ??= new LanguageManager(Settings.Instance.Language);
            return _instance;
        }
    }

    public string CurrentLanguage { get; private set; }

    public LanguageManager(string languageCode)
    {
        CurrentLanguage = _languages.ContainsKey(languageCode) ? languageCode : "en";
        _strings = _languages[CurrentLanguage];
    }

    public void SetLanguage(string languageCode)
    {
        CurrentLanguage = _languages.ContainsKey(languageCode) ? languageCode : "en";
        _strings = _languages[CurrentLanguage];
    }

    public string GetString(string key)
    {
        return _strings.TryGetValue(key, out var value) ? value : key;
    }

    public static string[] GetAvailableLanguages()
    {
        return new[] { "en", "ru", "de", "fr", "es", "pt", "it", "zh", "ja", "ko" };
    }

    public static string GetLanguageDisplayName(string code)
    {
        return _languageNames.TryGetValue(code, out var name) ? name : code;
    }
}