# Reverse Engineering Notes

## Hardware identification

The board was identified using esptool:

- Chip: ESP32-D0WD-V3
- Revision: 3.1
- Crystal: 40 MHz
- Flash: 4 MB

## Flash backup

A complete 4 MB backup was successfully read from COM8:

`esptool.exe --port COM8 read-flash 0 ALL KAJON_Original_Flash.bin`

The dump was read from 0x00000000 through the complete 4 MB flash address range.

## Firmware observations

Initial analysis found references/strings associated with:

- KAJON CASSETTE DECK
- SEGMENT
- PAUSE
- SPEC
- VOL- / VOL+
- TFT_eSPI
- Touch/sprite UI
- ESP32 I2S
- ESP32 internal DAC

The application image begins at the normal ESP32 application offset 0x10000.

## Next analysis targets

- Extract and inspect the partition table.
- Identify application boundaries and filesystem contents.
- Map TFT and touch GPIOs.
- Identify audio decoder and playback configuration.
- Determine all available GPIOs.
- Reconstruct the UI/state machine.
- Build a clean-room Arduino/ESP32 implementation.
