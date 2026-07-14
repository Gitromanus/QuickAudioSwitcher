# QuickAudioSwitcher 🎧🔊

Быстрое переключение между аудиоустройствами вывода из системного трея.

![tray](https://img.shields.io/badge/Windows%2011-✓-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)

## Возможности

- 🔄 **Левый клик** по иконке в трее — циклическое переключение между активными устройствами вывода
- 📋 **Правый клик** — меню со списком всех устройств для быстрого выбора
- 🎯 **Системные иконки** — для каждого устройства показывается его родная иконка из Windows
- ⌨️ **Горячая клавиша** — `Ctrl+Alt+F12` (настраивается)
- ⚙️ **Настройки** — можно изменить горячую клавишу через меню

## Установка

1. Скачайте последний релиз: [Releases](https://github.com/YOUR_USERNAME/QuickAudioSwitcher/releases)
2. Распакуйте `QuickAudioSwitcher.exe`
3. Запустите — приложение появится в системном трее

> **Примечание:** При первом запуске может появиться предупреждение SmartScreen. Нажмите "Подробнее" → "Выполнить в любом случае".

## Использование

| Действие | Результат |
|----------|-----------|
| Левый клик | Переключиться на следующее устройство |
| Правый клик | Открыть меню устройств |
| `Ctrl+Alt+F12` | Переключиться на следующее устройство |
| Меню → Настройки | Изменить горячую клавишу |
| Меню → Выход | Закрыть приложение |

## Как это работает

Приложение использует утилиту [SoundVolumeView](https://www.nirsoft.net/utils/sound_volume_view.html) от NirSoft для переключения стандартного аудиоустройства. Иконки устройств берутся из системного реестра Windows (`mmres.dll`).

## Сборка из исходников

```bash
git clone https://github.com/YOUR_USERNAME/QuickAudioSwitcher.git
cd QuickAudioSwitcher
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish
```

## Требования

- Windows 11 (может работать на Windows 10)
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

## Лицензия

MIT