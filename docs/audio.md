# Audio Path

The original firmware contains references associated with ESP32 I2S audio and the internal DAC, including:

- `i2s_driver_install`
- `i2s_write`
- `i2s_set_pin`
- `i2s_set_dac_mode`

GPIO26 is associated with the internal DAC output.

## Current hypothesis

MP3/audio data -> ESP32 audio/I2S processing -> internal DAC -> GPIO26 -> external amplifier/audio circuit.

The exact sample rate, channel configuration, decoding library, and whether the original output is mono or stereo are not yet confirmed.

## Future investigation

1. Determine decoder and sample rate.
2. Determine channel/downmix behavior.
3. Trace GPIO26 on the PCB.
4. Identify the amplifier/input stage.
5. Investigate unused GPIOs for an external I2S stereo DAC.
