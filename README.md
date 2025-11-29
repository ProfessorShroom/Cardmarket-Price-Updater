﻿ ![Arch](https://img.shields.io/badge/Arch-AMD64-blue) ![OS](https://img.shields.io/badge/OS-Windows%2010%20|%20Windows%2011-green)

## Cardmarket Price Updater
###### Copyright © Charlie Howard 2025. All rights reserved.

A C# based GUI that gets prices from [Cardmarket](https://www.cardmarket.com/en) based on spreadsheet contents for collection value purposes in either GBP (£) or EUR (€).

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


### Example Spreadsheet

| Release Date | Game      | Set Name                           | Set Code   | Card Price (£) | Price Updated | Cardmarket ID | Rarity                | Own?        | Bought For (£) | Collection Value (£)              |
| ------------ | --------- | ---------------------------------- | ---------- | -------------- | ------------- | ------------- | --------------------- | ----------- | -------------- | --------------------------------- |
| 12/09/2025   | Yu-Gi-Oh! | Nike Collaboration Cards (special) | NKC1-EN002 | £552.50        | 2025-11-29    | 845882        | Prismatic Secret Rare | LIMITED     | £200.00        | £28,934.37                        |
| 08/03/2002   | Yu-Gi-Oh! | Legend of Blue-Eyes White Dragon   | LOB-001    | £74.49         | 2025-11-29    | 577919        | Ultra Rare            | ✖           |                | Amount Spent (£)                  |
| 05/08/1993   | MTG       | Alpha                              |            | £26,328.87     | 2025-11-29    | 5465          | Rare                  | ✔           | £5,000.00      | £6,200.00                         |
| 09/01/1999   | Pokémon   | Base Set                           |            | £2,053.00      | 2025-11-29    | 660224        | Holo Rare             | 1st Edition | £1,000.00      | Amount to Complete Collection (£) |
|              |           |                                    |            |                |               |               |                       |             |                | £74.49                            |

#### Changelog

**Update 1.1.0.0**

Added Game to spreadsheet to specify the card game.

Will now check prices against the correct game instead of checking all.

**Update 1.0.0.0**

Initial commit.