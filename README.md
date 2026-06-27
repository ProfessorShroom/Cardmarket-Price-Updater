﻿ ![Arch](https://img.shields.io/badge/Arch-AMD64-blue) ![OS](https://img.shields.io/badge/OS-Windows%2010%20|%20Windows%2011-green)

## Cardmarket Price Updater
#### Copyright © Charlie Howard 2026 All rights reserved.

A C# based GUI/CLI that gets prices from [Cardmarket](https://www.cardmarket.com/en) based on spreadsheet contents for collection value purposes in either GBP (£) or EUR (€).

Currently supports Magic: The Gathering, Pokémon and Yu-Gi-Oh!

[Current Release Download](https://github.com/ProfessorShroom/Cardmarket-Price-Updater/releases)

At the moment, the cardmarket ID is manual, but when Cardmarket open up their API again, I will try to automate it.

The prices are based on an average of everything, so 1st Edition and Unlimited, etc. Once Cardmarket open up the API, I will be able to specify this to 1st Edition as this is what collectors want.

### Usage

Use the included Template.xlsx. The only 3 columns that are required are Card Price, Price Updated, and Cardmarket ID. The other columns are just there for you.

The template has 4 prefilled rows to show you how to fill it out.

Release Date isn't required, it's just for you. So you know when a card/set was released.

Game is so the program knows which card game to compare prices to, at the moment you can select MTG (Magic: The Gathering), Pokémon or Yu-Gi-Oh!

Set Name isn't required, but is just so you know what set that card is from, if, for example, the card has been released in multiple sets.

Set Code is only really used for Yu-Gi-Oh! from what I can tell, as that has a code on the card, eg, LOB-001, whereas others like MTG and Pokémon do not.

Card Price and Price Updated are filled out by the program, so leave them blank; they will get overwritten if you fill them out.

Cardmarket ID is the important part; this is the ID from Cardmarket's site that the program will use to find the correct card. For example, if you want to get the price of [LOB-001 Blue-Eyes White Dragon](https://www.cardmarket.com/en/YuGiOh/Products/Singles/Legend-of-Blue-Eyes-White-Dragon/Blue-Eyes-White-Dragon-V1-Ultra-Rare), the ID would be 577919. This is obtained by going to the card listing page and opening inspect (right click and press inspect, or press F12) and in that search box that opens, type in idProduct and hit enter, which will result in something like this
```
<input type="hidden" name="idProduct" value="577919">
```
The value is what you put in Cardmarket ID.

Own? and Bought For are only required if you want to use the collection value part of the sheet, as this will add up the values of the cards you own and show you the amount left to spend. These values are an average so not 100% accurate but it's better than nothing.

### CLI Usage

Run the .exe from terminal using these commands for headless use:

- /f to specify a filename, for example: .\Cardmarket-Price-Updater.exe /f .\Template.xlsx
- /d to specify a directory, this will run through every .xlsx in that directory, for example: .\Cardmarket-Price-Updater.exe /d .\
- /c to specify a currency, by default it's set to auto which will autodetect the currency in your spreadsheet and if not found it will fall back to £, for example: .\Cardmarket-Price-Updater.exe /c e /f .\Template.xlsx (/c e is EUR (€) and /c p is GBP (£))
- /log to log output to a file, for example: .\Cardmarket-Price-Updater.exe /f .\Template.xlsx /log .\log.txt
- /q, /quiet, /s, /silent all allow you to run silently with no dialog box at all, for example .\Cardmarket-Price-Updater.exe /f .\Template.xlsx /s

### Example Spreadsheet

| Release Date | Game      | Set Name                           | Card Name              | Set Code   | Card Price (£) | Price Updated | Cardmarket ID | Rarity                | Own? | Edition | Bought For (£) | Collection Value (£)              | Remaning Cards | Total Cards |
| ------------ | --------- | ---------------------------------- | ---------------------- | ---------- | -------------- | ------------- | ------------- | --------------------- | ---- | ------- | -------------- | --------------------------------- | -------------- | ----------- |
| 12/09/2025   | Yu-Gi-Oh! | Nike Collaboration Cards (special) | Red-Eyes Black Dragon  | NKC1-EN002 | £376.62        | 2026-06-27    | 845882        | Prismatic Secret Rare | ✔    | LIMITED | £200.00        | £28,799.95                        | 1              | 4           |
| 08/03/2002   | Yu-Gi-Oh! | Legend of Blue-Eyes White Dragon   | Blue-Eyes White Dragon | LOB-001    | £131.90        | 2026-06-27    | 577919        | Ultra Rare            | ✖    | ✖       |                | Amount Spent (£)                  |                |             |
| 05/08/1993   | MTG       | Alpha                              | Black Lotus            |            | £7,934.89      | 2026-06-27    | 5465          | Rare                  | ✔    | ✔       | £20,000.00     | £22,250.00                        |                |             |
| 09/01/1999   | Pokémon   | Base Set                           | Charizard              | BS 4       | £1,718.97      | 2026-06-27    | 660224        | Holo Rare             | ✔    | PSA9    | £2,000.00      | Amount to Complete Collection (£) |                |             |
| 18/11/2008   | Yu-Gi-Oh! | Crossroads of Chaos                | Black Rose Dragon      | CSOC-EN039 | £274.33        | 2026-06-27    | 108490        | Ghost Rare            | ✔    | PSA10   | £50.00         | £116.06                           |                |             |

#### Changelog

#### Latest Update

**Update 1.4.0.0**

- Changed quiet mode to actually hide the CLI completely.
- Added auto update feature.

#### Older Updates

**Update 1.3.0.0**

- Updated GUI to a more modern look.
- Added support to select pricing model; Trending Price, 7-Day Average Price and 30-Day Average Price. By default, it is set to 30-Day Average Price, but you can change it to Trending or 7-Day Average Price if you want a more stable price.

**Update 1.2.0.0**

- Added cmd/terminal support.
- /f lets you specify a file.
- /d lets you specify a directory.
- /c lets you specify a currency.
- /log lets you log to a file.
- /q, /quiet, /s, /silent runs the exe silently.

**Update 1.1.2.0**

- Updated EUR to GBP conversion link.

**Update 1.1.1.0**

- Moved Version/Readme link to [professorshroom.com](https://professorshroom.com)

**Update 1.1.0.0**

- Added Game to spreadsheet to specify the card game.
- Will now check prices against the correct game instead of checking all.
