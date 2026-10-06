# Flash Layout

Board flash size: 4 MB (4,194,304 bytes).

## Known layout

| Address | Size | Region |
|---|---:|---|
| 0x00009000 | 0x5000 | NVS |
| 0x0000E000 | 0x2000 | OTA data |
| 0x00010000 | 0x300000 | Application |
| 0x00310000 | 0x0E0000 | SPIFFS |
| 0x003F0000 | 0x010000 | Core dump |

The complete flash was dumped with:

`esptool.exe --port COM8 read-flash 0 ALL KAJON_Original_Flash.bin`

Result: 4,194,304 bytes.

The original dump is kept offline and is intentionally excluded from this public repository.
