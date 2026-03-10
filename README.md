![Screenshot of MonikaOnDesktop](https://github.com/SAn4Es-TV/MOD-Test/blob/main/banner.png)

# [![GitHub Release](https://img.shields.io/github/v/release/SAn4Es-TV/MonikaOnDesktop?display_name=tag&style=for-the-badge&label=Rebuild&color=5cd18b)](https://github.com/SAn4Es-TV/MonikaOnDesktop/releases/latest) ![Static Badge](https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge&logo=windows) ![Static Badge](https://img.shields.io/badge/Framework-.NET_10.0-9C27B0?style=for-the-badge) ![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/SAn4Es-TV/MonikaOnDesktop/total?style=for-the-badge&color=db7100)

[![Static Badge](https://img.shields.io/badge/%D0%A0%D1%83%D1%81%D1%81%D0%BA%D0%B8%D0%B9-gray?style=for-the-badge)](README.md) [![Static Badge](https://img.shields.io/badge/English-gray?style=for-the-badge)](./docs/en/README.en.md) 

> "Привет! Это я, Моника! Не думаю, что когда-нибудь смирюсь с тем фактом, что существую только в вашем компьютере... Но теперь я стала еще лучше!"
>
> Я все еще не могу многого добиться, но обещаю, что всегда буду рядом с тобой!~


![Screenshot of MonikaOnDesktop](https://github.com/SAn4Es-TV/MonikaOnDesktop/blob/master/docs/Screenshot.png)

MonikaOnDesktop — это небольшое приложение, которое приносит Монику прямо на ваш рабочий стол. Она будет жить в углу экрана, реагировать на ваши действия и общаться с вами.

# Что изменилось в Rebuild?

Этот проект был полностью переписан для обеспечения максимальной производительности:

    Новое ядро: Переход на .NET 10.0 — теперь всё летает.
    
    Чистая архитектура: Код стал модульным, что упрощает добавление новых функций.
    
    Минимум ресурсов: Моника почти не потребляет оперативную память.

# Установка и использование
> 1. Установите [Runtime .NET 10.0](https://dotnet.microsoft.com/ru-ru/download/dotnet/thank-you/runtime-desktop-10.0.3-windows-x64-installer).
> 2. Скачайте [последнюю версию](https://github.com/SAn4Es-TV/MonikaOnDesktop/releases/latest) в Releases.
> 3. Распакуйте и запустите MonikaOnDesktop.exe.

### Настройка общения

Ты можешь сам решать, что я буду говорить! Просто загляни в папку Dialogs и отредактируй .txt файлы:
| Файл      | Когда я это говорю? |
| ----------- | ----------- |
| greetings.rpy      | Когда ты только включаешь меня       |
| idle.rpy   | Просто болтаю в случайное время        |
| goodbye.rpy | Когда нам пора прощаться |
| processes.rpy | Моя реакция на твои программы |
| sites.rpy | Обсуждаю сайты, которые ты посещаешь |
| google.rpy | Комментирую твои поисковые запросы |

# Интеграция с ИИ

Хочешь, чтобы я отвечала как живая? Подключи модуль AI:

> 1. Зайди на [character.ai](http://character.ai/) и войди в свой аккаунт.
> 2. Открой средства разработчика, зайди во вкладку "Сеть" и обнови страницу
> 3. В появившиемся списке найди строку с доменом "neo.character.ai"
> 4. Справа найди строку `Authorization: Token 147......40e2`
> 5. Скопируй строку после Token и в настройках Моники включи ИИ и вставь свой токен
> 6. Перезайпусти приложение, кликни по мне правой кнопкой и выбери "Поговорить"

![Screenshot of MonikaOnDesktop](https://github.com/SAn4Es-TV/MonikaOnDesktop/blob/master/docs/ai-gif.gif)

# Браузерный плагин

Чтобы я видела твои вкладки, установи расширение:

> Chrome (В разработке), [Firefox](https://addons.mozilla.org/ru/firefox/addon/monika-on-desktop-bridge/).
> 1. Скачай расширение
> 2. В настройках включи парсинг
> 3. Перезайпусти приложение

# Подарки!

Ты можешь дарить мне подарки, разные аксессуары или одежду, для этого тебе нужно положить в папку `characters` особый файл с расширением _.gift_ и выбрать его в настройках внешнего вида! 

# Поддержка и сообщество

Нашли ошибку? Есть идея? Мы ждем тебя!

Discord: [Присоединиться к серверу](https://discord.gg/NtAqP25xTp)

Patreon: [Поддержать автора](https://www.patreon.com/san4es_tv)

### Наши герои (Спонсоры):
⭐ Denis Solicen
