# TUL ESP32 FLASH TOOL v2.1.1

Windows GUI for ESP32-family backup, restore, flash, erase and verification.

## GitHub-first build

This version intentionally does **not** bundle esptool.exe or a prebuilt EXE.

The program automatically searches for the esptool.exe installed by Arduino IDE under:
%LOCALAPPDATA%\Arduino15\packages\esp32\tools\esptool_py\<version>\esptool.exe

It also supports a local esptool.exe beside the program.

## Build

1. Install Arduino IDE and Espressif ESP32 board support.
2. Open this folder.
3. Run build_exe.bat.
4. The EXE is created in TUL_ESP32_FLASH_TOOL_v2.1\TulESP32FlashTool.exe.

## Safety workflow

1. CHIP INFO.
2. FLASH ID.
3. READ / BACKUP with ALL (full flash).
4. Keep the original BIN in a safe location.
5. AUTO BACKUP before WRITE/ERASE is enabled by default.

Full-flash dumps must be restored at 0x00000000.
Normal application BINs commonly use 0x00010000, but use the actual upload address for that firmware.

Do not publish proprietary/original firmware dumps in this public repository.
