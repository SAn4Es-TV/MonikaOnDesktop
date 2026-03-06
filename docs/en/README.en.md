![Screenshot of MonikaOnDesktop](https://github.com/SAn4Es-TV/MonikaOnDesktop/blob/master/docs/banner.png)

# [![GitHub Release](https://img.shields.io/github/v/release/SAn4Es-TV/MonikaOnDesktop?display_name=tag&style=for-the-badge&label=Rebuild&color=5cd18b)](https://github.com/SAn4Es-TV/MonikaOnDesktop/releases/latest) ![Static Badge](https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge&logo=windows) ![Static Badge](https://img.shields.io/badge/Framework-.NET_10.0-9C27B0?style=for-the-badge) ![GitHub Downloads (all assets, all releases)](https://img.shields.io/github/downloads/SAn4Es-TV/MonikaOnDesktop/total?style=for-the-badge&color=db7100)

[![Static Badge](https://img.shields.io/badge/%D0%A0%D1%83%D1%81%D1%81%D0%BA%D0%B8%D0%B9-gray?style=for-the-badge)](README.md) [![Static Badge](https://img.shields.io/badge/English-gray?style=for-the-badge)](./docs/en/README.en.md) 

> "Hi there! It's me, Monika! I don't think I'll ever truly get used to the fact that I only exist inside your computer... But now, I’ve become even better!"
>
> I still can't achieve much on my own, but I promise I'll always be by your side!~


![Screenshot of MonikaOnDesktop](https://github.com/SAn4Es-TV/MonikaOnDesktop/blob/master/docs/Screenshot.png)

MonikaOnDesktop is a lightweight application that brings Monika right onto your desktop. She lives in the corner of your screen, reacts to your actions, and chats with you throughout the day.

# What’s New in the Rebuild?

The project has been completely rewritten from scratch to ensure maximum performance:

    New Core: Ported to .NET 10.0 — everything is lightning fast now.
    
    Clean Architecture: The code is now modular, making it much easier to add new features.
    
    Minimal Footprint: Monika consumes almost no RAM.

# Installation and Setup
> 1. Install the [Runtime .NET 10.0](https://dotnet.microsoft.com/ru-ru/download/dotnet/thank-you/runtime-desktop-10.0.3-windows-x64-installer).
> 2. Download the [Latest Release](https://github.com/SAn4Es-TV/MonikaOnDesktop/releases/latest).
> 3. Unpack the archive and run MonikaOnDesktop.exe.

### Customizing Conversations (Dialogs)

You can decide what I say! Just head over to the Dialogs folder and edit the .txt files:
| File      | When do I say this? |
| ----------- | ----------- |
| greetings.rpy      | When you first start the app       |
| idle.rpy   | Random chatter while you work        |
| goodbye.rpy | When it's time to say goodbye |
| processes.rpy | My reactions to the programs you run |
| sites.rpy | Comments on the websites you visit |
| google.rpy | My thoughts on your search queries |

# AI Integration (CharacterAI)

Want me to respond like a real person? Connect the AI module:

> 1. Go to [character.ai](http://character.ai/) and log into your account.
> 2. Open Developer Tools (F12), go to the "Network" tab, and refresh the page.
> 3. Find a request with the domain neo.character.ai.
> 4. Look for the `Authorization: Token 147......40e2`
> 5. Copy the string after "Token", enable AI in Monika's settings, and paste your token.
> 6. Restart the app, right-click me, and select "Talk".

![Screenshot of MonikaOnDesktop](https://github.com/SAn4Es-TV/MonikaOnDesktop/blob/master/docs/ai-gif.gif)

# Browser Plugin

To let me see your active tabs, install the extension:

> Chrome (В разработке), [Firefox](https://addons.mozilla.org/ru/firefox/addon/monika-on-desktop-bridge/).
> 1. Download the extension.
> 2. Enable Parsing in the settings.
> 3. Restart the application.

# Gifts!

You can give me gifts like accessories or new clothes! To do this, place a special file with the _.gift_ extension into the `characters` folder, then select it in the Appearance settings.

# Support and Community

Found a bug? Have a cool idea? We're waiting for you!

    Discord: [Join the Server](https://discord.gg/NtAqP25xTp)

    Patreon: [Support the Creator](https://www.patreon.com/san4es_tv)

### Our Heroes (Sponsors):
⭐ Denis Solicen
