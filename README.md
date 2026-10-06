# TUL-KAJON-Mod

Reverse-engineering and clean-room firmware development project for the KAJON ESP32 2.8" TFT board.

## Hardware

- MCU: ESP32-D0WD-V3, revision 3.1
- Flash: 4 MB
- Display: 2.8" TFT
- Touch: present
- microSD: present
- Original audio path: ESP32 internal DAC, GPIO26
- UART RX/TX header
- Additional GPIOs: to be verified

## Original firmware

A complete 4 MB flash backup was successfully read from the board with esptool.

**Do not commit the original flash dump to this public repository.**

## Project goals

- Document the board pinout
- Reverse-engineer the original firmware behavior
- Recreate the firmware from source
- Investigate external I2S DAC support
- Preserve useful original functionality while developing a custom firmware

## Status

- [x] Full 4 MB flash backup
- [x] ESP32 chip identification
- [x] Flash identification
- [ ] Pinout verification
- [ ] Audio path verification
- [ ] Custom firmware
