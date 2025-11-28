 ![Arch](https://img.shields.io/badge/Arch-AMD64-blue) ![OS](https://img.shields.io/badge/OS-Windows%2010%20|%20Windows%2011-green)

## Cardmarket Price Updater
###### Copyright © Charlie Howard 2025 All rights reserved.

A C# based GUI that gets prices from [Cardmarket](https://www.cardmarket.com/en) based on spreadsheet contents for collection value purposes in either GBP (£) or EUR (€).

At the moment the cardmarket ID is manual, but when Cardmarket open up their API again I will try to automate it.

The prices are based on an average of everything, so 1st Ed and Unlimited etc. once Cardmarket open up the API I will be able to specificy this to 1st Edition as his is what collectors want.

### Usage

Use the included Template.xlsx. The only 3 columns that are required are Card Price, Price Updated, Cardmarket ID. The other columns are just there for you.

The template has 4 prefilled rows to show you how to fill it out.

Set Name isn't required but is just so you know what set that card is from if for example the card has released in multiple sets.

Set Code is only really used for Yu-Gi-Oh! from what I can tell, as that has a code on the card, eg. LOB-001, whereas others like MTG and Pokémon do not.

Card Price and Price Updated are filled out by the program so leave them blank, they will get overwritten if you fill them out.

Cardmarket ID is the important part, this is the ID from Cardmarkets site the program will use to find the correct card. For example if you want to get the price the [LOB-001 Blue-Eyes White Dragon](https://www.cardmarket.com/en/YuGiOh/Products/Singles/Legend-of-Blue-Eyes-White-Dragon/Blue-Eyes-White-Dragon-V1-Ultra-Rare) the ID would be 577919. This is obtained by going the card listing page and opening inspect (right click and press inspect, or press F12) and in that search box that opens, type in idProduct and hit enter which will reslt in something like this
```
<input type="hidden" name="idProduct" value="577919">
```
the value is what you put in Cardmarket ID.

Own? and Bought For are only required if you want to use the collection value part of the sheet as this will add up the values of the cards you own and show you the amount left to spend. These values are an average so not 100% accurate but it's better than nothing.


### Example Spreadsheet

| Set Name                           | Set Code   | Card Price (£) | Price Updated | Cardmarket ID | Rarity                | Own?        | Bought For (£) | Collection Value (£)              |
| ---------------------------------- | ---------- | -------------- | ------------- | ------------- | --------------------- | ----------- | -------------- | --------------------------------- |
| Nike Collaboration Cards (special) | NKC1-EN002 | £552.50        | 2025-11-28    | 845882        | Prismatic Secret Rare | LIMITED     | £200.00        | £28,934.37                        |
| Legend of Blue-Eyes White Dragon   | LOB-001    | £86.96         | 2025-11-28    | 577919        | Ultra Rare            | ✖           |                | Amount Spent (£)                  |
| Alpha                              |            | £26,328.87     | 2025-11-28    | 5465          | Rare                  | ✔           | £5,000.00      | £6,200.00                         |
| Base Set                           |            | £2,053.00      | 2025-11-28    | 660224        | Holo Rare             | 1st Edition | £1,000.00      | Amount to Complete Collection (£) |
|                                    |            |                |               |               |                       |             |                | £86.96                            |

#### Changelog

**Update 1.0.0.0**

Initial commit.