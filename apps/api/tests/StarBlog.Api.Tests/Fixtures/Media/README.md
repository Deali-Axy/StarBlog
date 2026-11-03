# Media regression fixtures

The landscape JPEG/PNG/WebP files contain asymmetric red and blue rectangles on white, generated with SkiaSharp 4.154.0 outside the application. No SkiaSharp dependency is required to run these tests.

- `orientation-6.jpg`: same 1800 x 1200 JPEG with a minimal EXIF Orientation=6 (clockwise 90 degrees) segment.
- `transparent.png`: 16 x 10 fully transparent RGBA PNG.
- `panorama.png`: 3000 x 1 red PNG, exercising minimum output height.
- `animated.gif`: hand-encoded two-frame 1 x 1 GIF89a, red then blue, using a two-entry global palette.
- `oversized.png`: PNG header declaring 10000 x 10000 pixels, with valid chunk CRCs but no pixel payload. The decoder must reject its pixel count before attempting decompression or allocation.
