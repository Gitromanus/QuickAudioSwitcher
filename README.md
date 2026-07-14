# QuickAudioSwitcher 🎧🔊

Быстрое переключение между аудиоустройствами вывода из системного трея.

![Windows](https://img.shields.io/badge/Windows%2011-✓-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Release](https://img.shields.io/github/v/release/Gitromanus/QuickAudioSwitcher)

## Возможности

- 🔄 **Левый клик** по иконке в трее — циклическое переключение между активными устройствами вывода
- 📋 **Правый клик** — меню со списком всех устройств для быстрого выбора
- 🎯 **Системные иконки** — для каждого устройства показывается его родная иконка из Windows
- ⌨️ **Горячая клавиша** — `Ctrl+Alt+F12` (настраивается)
- ⚙️ **Настройки** — можно изменить горячую клавишу через меню
- 🚀 **Автозапуск** — включается/отключается из меню трея

## Установка

### Вариант 1: Установщик (рекомендуется)
1. Скачайте [QuickAudioSwitcher-1.0.0-Setup.exe](https://github.com/Gitromanus/QuickAudioSwitcher/releases/latest/download/QuickAudioSwitcher-1.0.0-Setup.exe)
2. Запустите установщик — приложение установится в `Program Files` и появится в меню Пуск
3. После установки приложение запустится автоматически

### Вариант 2: Портативная версия
1. Скачайте [QuickAudioSwitcher.exe](https://github.com/Gitromanus/QuickAudioSwitcher/releases/latest/download/QuickAudioSwitcher.exe)
2. Запустите — приложение появится в системном трее

> **Примечание:** При первом запуске может появиться предупреждение SmartScreen. Нажмите "Подробнее" → "Выполнить в любом случае".

## Использование

| Действие | Результат |
|----------|-----------|
| Левый клик | Переключиться на следующее устройство |
| Правый клик | Открыть меню устройств |
| `Ctrl+Alt+F12` | Переключиться на следующее устройство |
| Меню → Настройки | Изменить горячую клавишу |
| Меню → Автозапуск | Включить/отключить автозагрузку |
| Меню → Выход | Закрыть приложение |

## Как это работает

Приложение использует утилиту [SoundVolumeView](https://www.nirsoft.net/utils/sound_volume_view.html) от NirSoft для переключения стандартного аудиоустройства. Иконки устройств берутся из системного реестра Windows (`mmres.dll`).

## Сборка из исходников

```bash
git clone https://github.com/Gitromanus/QuickAudioSwitcher.git
cd QuickAudioSwitcher
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o publish
```

Для сборки установщика требуется [Inno Setup 6](https://jrsoftware.org/isdl.php):
```bash
ISCC installer.iss
```

## Требования

- Windows 11 / Windows 10
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

## Лицензия

MIT