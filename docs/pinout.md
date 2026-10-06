# Pinout

## Confirmed

| GPIO | Function | Confidence |
|---|---|---|
| GPIO26 | Original audio output / ESP32 internal DAC | High |

The firmware references ESP32 I2S functions and internal DAC configuration, including `i2s_set_dac_mode`, with GPIO26 used as the audio output path.

## To verify

- TFT SPI pins
- Touch controller pins
- microSD pins
- UART RX/TX
- Remaining GPIO header pins
- Any amplifier control / enable pins

Pin assignments should be confirmed from both firmware analysis and physical PCB tracing before being used in the custom firmware.
