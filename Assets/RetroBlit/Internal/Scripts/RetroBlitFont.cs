namespace RetroBlitInternal
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Font subsystem
    /// </summary>
    public class RetroBlitFont
    {
        private const char ESCAPE_CHAR = '@';
        private const int MAX_LINE_WIDTHS = 65536;
        private const int MAX_SHAKE_OFFSETS = 32;
        private const int MAX_SHAKE_AMOUNT = 10;
        private const int MAX_WAVE_OFFSETS = 8;
        private const int MAX_WAVE_AMOUNT = 10;

        private RetroBlitAPI mRetroBlitAPI;

        private RetroBlitFontBuiltin mFontBuiltin;
        private FontDef[] mFonts = new FontDef[RetroBlitHW.HW_FONTS + 1];

        private int[] mLinePixelWidths = new int[MAX_LINE_WIDTHS];
        private int[] mLineCharacterWidths = new int[MAX_LINE_WIDTHS];

        private uint mShakeSeed = 0;
        private float mWaveTicker = 0;
        private Vector2i[,] mShakeOffsets = new Vector2i[MAX_SHAKE_AMOUNT, MAX_SHAKE_OFFSETS];
        private Vector2i[,] mWaveOffsets = new Vector2i[MAX_WAVE_AMOUNT, MAX_WAVE_OFFSETS];

        private int[] mHexValueLookup;
        private int[] mHexCharLookup;
        private int[] mDecimalCharLookup;

        /// <summary>
        /// Initialize the subsystem
        /// </summary>
        /// <param name="api">Reference to subsystem wrapper</param>
        /// <returns>True if successful</returns>
        public bool Initialize(RetroBlitAPI api)
        {
            if (api == null)
            {
                return false;
            }

            mRetroBlitAPI = api;

            mFontBuiltin = new RetroBlitFontBuiltin();
            mFontBuiltin.InitializeBuiltinFonts(mRetroBlitAPI);

            mShakeSeed = (uint)Random.Range(0, int.MaxValue);

            mHexValueLookup = new int['f' + 1];
            mHexValueLookup['0'] = 0x0;
            mHexValueLookup['1'] = 0x1;
            mHexValueLookup['2'] = 0x2;
            mHexValueLookup['3'] = 0x3;
            mHexValueLookup['4'] = 0x4;
            mHexValueLookup['5'] = 0x5;
            mHexValueLookup['6'] = 0x6;
            mHexValueLookup['7'] = 0x7;
            mHexValueLookup['8'] = 0x8;
            mHexValueLookup['9'] = 0x9;
            mHexValueLookup['a'] = 0xA;
            mHexValueLookup['A'] = 0xA;
            mHexValueLookup['b'] = 0xB;
            mHexValueLookup['B'] = 0xB;
            mHexValueLookup['c'] = 0xC;
            mHexValueLookup['C'] = 0xC;
            mHexValueLookup['d'] = 0xD;
            mHexValueLookup['D'] = 0xD;
            mHexValueLookup['e'] = 0xE;
            mHexValueLookup['E'] = 0xE;
            mHexValueLookup['f'] = 0xF;
            mHexValueLookup['F'] = 0xF;

            mHexCharLookup = new int[128];
            mHexCharLookup['0'] = 1;
            mHexCharLookup['1'] = 1;
            mHexCharLookup['2'] = 1;
            mHexCharLookup['3'] = 1;
            mHexCharLookup['4'] = 1;
            mHexCharLookup['5'] = 1;
            mHexCharLookup['6'] = 1;
            mHexCharLookup['7'] = 1;
            mHexCharLookup['8'] = 1;
            mHexCharLookup['9'] = 1;
            mHexCharLookup['a'] = 1;
            mHexCharLookup['A'] = 1;
            mHexCharLookup['b'] = 1;
            mHexCharLookup['B'] = 1;
            mHexCharLookup['c'] = 1;
            mHexCharLookup['C'] = 1;
            mHexCharLookup['d'] = 1;
            mHexCharLookup['D'] = 1;
            mHexCharLookup['e'] = 1;
            mHexCharLookup['E'] = 1;
            mHexCharLookup['f'] = 1;
            mHexCharLookup['F'] = 1;

            mDecimalCharLookup = new int[128];
            mDecimalCharLookup['0'] = 1;
            mDecimalCharLookup['1'] = 1;
            mDecimalCharLookup['2'] = 1;
            mDecimalCharLookup['3'] = 1;
            mDecimalCharLookup['4'] = 1;
            mDecimalCharLookup['5'] = 1;
            mDecimalCharLookup['6'] = 1;
            mDecimalCharLookup['7'] = 1;
            mDecimalCharLookup['8'] = 1;
            mDecimalCharLookup['9'] = 1;

            return true;
        }

        /// <summary>
        /// Setup a custom font
        /// </summary>
        /// <param name="fontIndex">Font slot index</param>
        /// <param name="unicodeStart">First unicode character</param>
        /// <param name="unicodeEnd">Last unicode character</param>
        /// <param name="characterList">List of characters</param>
        /// <param name="srcPos">Position in spritesheet</param>
        /// <param name="spriteSheetIndex">The index of the sprite sheet containing the font</param>
        /// <param name="glyphSize">Size of a single glyph</param>
        /// <param name="glyphsPerRow">Glyphs per row</param>
        /// <param name="characterSpacing">Spacing between characters</param>
        /// <param name="lineSpacing">Line spacing between text</param>
        /// <param name="monospaced">Is monospaced</param>
        /// <param name="systemFont">True if is system font</param>
        /// <param name="calculateNow">Should calculate font info now</param>
        public void FontSetup(int fontIndex, char unicodeStart, char unicodeEnd, List<char> characterList, Vector2i srcPos, int spriteSheetIndex, Vector2i glyphSize, int glyphsPerRow, int characterSpacing, int lineSpacing, bool monospaced, bool systemFont, bool calculateNow)
        {
            if (fontIndex < 0 || fontIndex >= mFonts.Length)
            {
                return;
            }

            mFonts[fontIndex] = new FontDef();
            var font = mFonts[fontIndex];

            font.isSystemFont = systemFont;

            font.srcPos = srcPos;

            font.glyphSize = glyphSize;
            font.glyphsPerRow = glyphsPerRow;

            font.monospaced = monospaced;

            font.glyphsCalculated = calculateNow;

            font.spriteSheetIndex = spriteSheetIndex;

            if (characterList == null)
            {
                int glyphCount = unicodeEnd - unicodeStart + 1;

                font.glyphDef = new CharHashTable<GlyphDef>(glyphCount);
                for (int i = unicodeStart; i <= unicodeEnd; i++)
                {
                    font.glyphDef.Set((char)i, new GlyphDef(Rect2i.zero));
                }
            }
            else
            {
                int glyphCount = characterList.Count;

                font.glyphDef = new CharHashTable<GlyphDef>(glyphCount);
                for (int i = 0; i < characterList.Count; i++)
                {
                    font.glyphDef.Set(characterList[i], new GlyphDef(Rect2i.zero));
                }
            }

            font.characterSpacing = characterSpacing;
            font.lineSpacing = lineSpacing;

            if (calculateNow)
            {
                CalculateGlyphWidths(font);
            }

            if (monospaced)
            {
                font.spaceWidth = glyphSize.width;
            }
            else
            {
                if (!systemFont)
                {
                    // If character has a space glyph then use it to specify space width
                    // Otherwise calculate one based on glyph max width
                    var spaceGlyph = font.glyphDef.Get(' ');

                    if (spaceGlyph == null)
                    {
                        font.spaceWidth = (int)((glyphSize.width / 2.0f) - 0.5f);
                    }
                    else
                    {
                        font.spaceWidth = spaceGlyph.rect.width;
                    }
                }
                else
                {
                    var spaceWidth = mFontBuiltin.SpaceWidth;

                    if (spaceWidth == 0)
                    {
                        font.spaceWidth = (int)((glyphSize.width / 2.0f) - 0.5f);
                    }
                    else
                    {
                        font.spaceWidth = spaceWidth;
                    }
                }
            }
        }

        /// <summary>
        /// Setup a custom font
        /// </summary>
        /// <param name="fontIndex">Font slot index</param>
        /// <param name="unicodeStart">First unicode character</param>
        /// <param name="unicodeEnd">Last unicode character</param>
        /// <param name="characterList">List of characters</param>
        /// <param name="glyphSprites">List of glyph sprites that make up the font, must contain all sprites in the given unicode range</param>
        /// <param name="spriteSheetIndex">The index of the sprite sheet containing the font</param>
        /// <param name="characterSpacing">Spacing between characters</param>
        /// <param name="lineSpacing">Line spacing between text</param>
        /// <param name="monospaced">Is monospaced</param>
        /// <param name="calculateNow">Should calculate font info now</param>
        public void FontSetup(int fontIndex, int unicodeStart, int unicodeEnd, List<char> characterList, List<PackedSprite> glyphSprites, int spriteSheetIndex, int characterSpacing, int lineSpacing, bool monospaced, bool calculateNow)
        {
            if (fontIndex < 0 || fontIndex >= mFonts.Length)
            {
                return;
            }

            mFonts[fontIndex] = new FontDef();
            var font = mFonts[fontIndex];

            if (characterList == null)
            {
                int glyphCount = unicodeEnd - unicodeStart + 1;

                font.glyphDef = new CharHashTable<GlyphDef>(glyphCount);
                for (int i = unicodeStart; i <= unicodeEnd; i++)
                {
                    font.glyphDef.Set((char)i, new GlyphDef(Rect2i.zero));
                }
            }
            else
            {
                int glyphCount = characterList.Count;

                font.glyphDef = new CharHashTable<GlyphDef>(glyphCount);
                for (int i = 0; i < characterList.Count; i++)
                {
                    font.glyphDef.Set(characterList[i], new GlyphDef(Rect2i.zero));
                }
            }

            font.srcPos = Vector2i.zero;

            var firstSprite = glyphSprites[0];
            if (firstSprite.Size.width <= 0)
            {
                Debug.LogError("Invalid glyph sprites, first sprite has width of 0!");
                return;
            }

            for (int i = 1; i < glyphSprites.Count; i++)
            {
                if (firstSprite.Size != glyphSprites[i].Size)
                {
                    Debug.LogError("Font glyph sprites have inconsistent sizes, they should all be the same size! Glyph for unicode [" + (unicodeStart + i) + "] is size " + glyphSprites[i].Size.width + "x" + glyphSprites[i].Size.height);
                    return;
                }
            }

            font.glyphSize = firstSprite.Size;
            font.glyphsPerRow = 0;

            font.monospaced = monospaced;

            font.glyphsCalculated = calculateNow;

            font.spriteSheetIndex = spriteSheetIndex;

            if (monospaced)
            {
                font.spaceWidth = font.glyphSize.width;
            }
            else
            {
                font.spaceWidth = (int)((font.glyphSize.width / 2.0f) - 0.5f);
            }

            font.characterSpacing = characterSpacing;
            font.lineSpacing = lineSpacing;

            font.glyphSprites = glyphSprites;

            if (calculateNow)
            {
                CalculateGlyphWidths(font);
            }
        }

        /// <summary>
        /// Print text using given font
        /// </summary>
        /// <param name="fontIndex">Font index</param>
        /// <param name="textRect">Rectangular area to print to</param>
        /// <param name="color">RGB color</param>
        /// <param name="textFlags">Any combination of flags: <see cref="RB.ALIGN_H_LEFT"/>, <see cref="RB.ALIGN_H_RIGHT"/>,
        /// <see cref="RB.ALIGN_H_CENTER"/>, <see cref="RB.ALIGN_V_TOP"/>, <see cref="RB.ALIGN_V_BOTTOM"/>,
        /// <see cref="RB.ALIGN_V_CENTER"/>, <see cref="RB.TEXT_OVERFLOW_CLIP"/>, <see cref="RB.TEXT_OVERFLOW_WRAP"/>.</param>
        /// <param name="text">Text to print</param>
        /// <param name="measureOnly">Measure only, don't draw</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public void Print(int fontIndex, Rect2i textRect, Color32 color, int textFlags, TextWrap text, bool measureOnly, out int width, out int height)
        {
            var meshStorage = mRetroBlitAPI.Renderer.CurrentMeshStorage();

            int systemGlyphStride;
            var systemGlyphRects = mFontBuiltin.FontGlyphs(RetroBlitInternal.RetroBlitHW.HW_SYSTEM_FONT, out systemGlyphStride);

            width = height = 0;

            int originalFontIndex = fontIndex;

            int waveAmplitude = 0;
            float wavePeriod = 0;
            float waveSpeed = 0;
            float waveIndex = 0;

            int shakeAmount = 0;
            uint shakeIndex = 0;

            if (text == null)
            {
                return;
            }

            var textLength = text.Length;

            if (fontIndex < 0 || fontIndex >= mFonts.Length)
            {
                return;
            }

            var font = mFonts[fontIndex];
            if (font == null)
            {
                return;
            }

            if (font.glyphsCalculated == false)
            {
                if (!CalculateGlyphWidths(font))
                {
                    return;
                }
            }

            Rect2i previousClipRect = RB.ClipGet();
            int previousSpriteSheetIndex = mRetroBlitAPI.Renderer.CurrentSpriteSheetIndex;
            if (!measureOnly && !font.isSystemFont)
            {
                mRetroBlitAPI.Renderer.SpriteSheetSet(font.spriteSheetIndex);
            }

            if (!measureOnly)
            {
                if ((textFlags & RB.TEXT_OVERFLOW_CLIP) != 0)
                {
                    var cameraPos = RB.CameraGet();
                    RB.ClipSet(new Rect2i(textRect.x - cameraPos.x, textRect.y - cameraPos.y, textRect.width, textRect.height));
                }
            }

            int lineCount = 0;
            int lineIndex = 0;
            int charactersInLine = 0;

            Vector2i textSize = new Vector2i(0, 0);

            int maxLineWidth = (textFlags & RB.TEXT_OVERFLOW_WRAP) != 0 ? textRect.width : int.MaxValue;

            // Only need to measure line widths if not aligned to top left
            if ((textFlags & RB.ALIGN_H_RIGHT) != 0 || (textFlags & RB.ALIGN_H_CENTER) != 0 ||
                (textFlags & RB.ALIGN_V_BOTTOM) != 0 || (textFlags & RB.ALIGN_V_CENTER) != 0 ||
                (textFlags & RB.TEXT_OVERFLOW_WRAP) != 0)
            {
                MeasureLineWidths(fontIndex, text, maxLineWidth, textFlags, out textSize, out lineCount);
            }

            int px = textRect.x;
            int py = textRect.y;
            int largestWidth = 0;
            int totalHeight = 0;
            int lineStartIndex = 0;

            var currentColor = mRetroBlitAPI.Renderer.TintColorGet();
            color.r = (byte)((color.r * currentColor.r) / 255);
            color.g = (byte)((color.g * currentColor.g) / 255);
            color.b = (byte)((color.b * currentColor.b) / 255);
            color.a = (byte)((color.a * currentColor.a) / 255);

            Color32 originalColor = color;

            SetLinePosition(font, textRect, lineIndex, textFlags, out px);

            if ((textFlags & RB.ALIGN_V_BOTTOM) != 0)
            {
                py = (textRect.y + textRect.height) - (lineCount * (font.glyphSize.height + font.lineSpacing));

                // Compensate for line spacing on last line
                if (!font.monospaced && lineCount > 0)
                {
                    py += font.lineSpacing;
                }
            }
            else if ((textFlags & RB.ALIGN_V_CENTER) != 0)
            {
                int offset = (textRect.height / 2) - ((lineCount * (font.glyphSize.height + font.lineSpacing)) / 2);

                // If monospaced  make sure it's still lined up on line boundaries even if centered!
                if (font.monospaced)
                {
                    offset -= offset % (font.glyphSize.height + font.lineSpacing);
                }

                py = textRect.y + offset;

                // Compensate for line spacing on last line
                if (!font.monospaced && lineCount > 0)
                {
                    py += font.lineSpacing / 2;
                }
            }

            bool forceEndOfLine = false;

            for (int i = 0; i < textLength; i++)
            {
                char c = text[i];

                if (c == '\n' || forceEndOfLine)
                {
                    py += font.glyphSize.height + font.lineSpacing;

                    if (measureOnly)
                    {
                        largestWidth = Mathf.Max(px - textRect.x, largestWidth);
                        totalHeight += font.glyphSize.height + font.lineSpacing;
                    }

                    lineIndex++;
                    SetLinePosition(font, textRect, lineIndex, textFlags, out px);

                    lineStartIndex = i + 1;
                    if (forceEndOfLine)
                    {
                        lineStartIndex--;
                    }

                    if (forceEndOfLine && c != '\n')
                    {
                        i--;
                        c = text[i];
                    }

                    forceEndOfLine = false;

                    charactersInLine = 0;

                    continue;
                }
                else if (c == ESCAPE_CHAR && i < textLength - 1 && (textFlags & RB.NO_ESCAPE_CODES) == 0)
                {
                    i++;
                    c = text[i];

                    if (c == '-')
                    {
                        color = originalColor;
                        continue;
                    }
                    else if (c == 'g' && i < textLength - 2)
                    {
                        // In line font change
                        var c1 = text[i + 1];
                        var c2 = text[i + 2];
                        if (c1 >= '0' && c1 <= '9' && c2 >= '0' && c2 <= '9')
                        {
                            int newFontIndex = ((int)(c1 - '0') * 10) + (int)(c2 - '0');

                            // System font
                            if (newFontIndex == 99)
                            {
                                newFontIndex = RetroBlitHW.HW_SYSTEM_FONT;
                            }

                            if (newFontIndex != fontIndex && newFontIndex >= 0 && newFontIndex < mFonts.Length && mFonts[newFontIndex] != null)
                            {
                                var newFont = mFonts[newFontIndex];
                                if (newFont.glyphSize.height != font.glyphSize.height)
                                {
                                    RetroBlitUtil.LogErrorOnce("Inline font changes can only be used with font that have the same glyph height. Glyph width can differ.");
                                }
                                else
                                {
                                    font = newFont;
                                    fontIndex = newFontIndex;
                                    mRetroBlitAPI.Renderer.SpriteSheetSet(font.spriteSheetIndex);

                                    if (font.glyphsCalculated == false)
                                    {
                                        if (!CalculateGlyphWidths(font))
                                        {
                                            return;
                                        }
                                    }
                                }
                            }

                            i += 2;
                            c = c2;
                            continue;
                        }
                        else if (c1 == '-')
                        {
                            font = mFonts[originalFontIndex];

                            i++;
                            c = c1;
                            continue;
                        }
                    }
                    else if (c == 's' && i < textLength - 1)
                    {
                        var c1 = text[i + 1];

                        // Shake effect
                        if (c1 >= '0' && c1 <= '9')
                        {
                            shakeAmount = (int)(c1 - '0');

                            i++;
                            c = c1;
                            continue;
                        }
                    }
                    else if (c == 'w' && i < textLength - 3)
                    {
                        // Wave effect. Format is APS - Amplitude, Period, Speed
                        bool validDecimal = true;
                        i++;
                        c = text[i];

                        if (textLength - i >= 3)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (mDecimalCharLookup[text[i + j] & 0x7F] == 0)
                                {
                                    validDecimal = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            validDecimal = false;
                        }

                        if (!validDecimal)
                        {
                            i--;
                            c = text[i];
                            continue;
                        }
                        else
                        {
                            char c2 = text[i + 2];

                            waveAmplitude = c - '0';
                            wavePeriod = (text[i + 1] - '0') / 9.0f * 1.5f;
                            waveSpeed = (c2 - '0') / 9.0f * 10.0f;

                            // Multiply speed by amplitude so that glyphs are still moving fast if amplitude is low
                            waveSpeed *= ((waveAmplitude - 9.0f) / 2.0f) + 1.0f;

                            i += 2;
                            c = c2;

                            continue;
                        }
                    }
                    else
                    {
                        // Decode the hex string for color rgb
                        if (mHexCharLookup[c & 0x7F] == 1)
                        {
                            var c1 = text[i + 1];
                            var c2 = text[i + 2];
                            var c3 = text[i + 3];
                            var c4 = text[i + 4];
                            var c5 = text[i + 5];

                            bool validHex = true;
                            if (textLength - i >= 6)
                            {
                                if (mHexCharLookup[c & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                                else if (mHexCharLookup[c1 & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                                else if (mHexCharLookup[c2 & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                                else if (mHexCharLookup[c3 & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                                else if (mHexCharLookup[c4 & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                            }
                            else
                            {
                                validHex = false;
                            }

                            if (!validHex)
                            {
                                i--;
                                c = text[i];
                                continue;
                            }
                            else
                            {
                                if ((textFlags & RB.NO_INLINE_COLOR) == 0)
                                {
                                    byte r = (byte)((mHexValueLookup[c] << 4) + mHexValueLookup[c1]);
                                    byte g = (byte)((mHexValueLookup[c2] << 4) + mHexValueLookup[c3]);
                                    byte b = (byte)((mHexValueLookup[c4] << 4) + mHexValueLookup[c5]);

                                    // Fast color multiply
                                    currentColor = mRetroBlitAPI.Renderer.TintColorGet();
                                    color.r = (byte)((r * currentColor.r) / 255);
                                    color.g = (byte)((g * currentColor.g) / 255);
                                    color.b = (byte)((b * currentColor.b) / 255);
                                    color.a = (byte)((255 * currentColor.a) / 255);
                                }

                                i += 5;
                                c = c5;

                                continue;
                            }
                        }
                    }

                    if (c != ESCAPE_CHAR)
                    {
                        i--;
                        c = text[i];
                        continue;
                    }
                }

                int advance = 0;
                bool printable = true;
                var rect = new Rect2i(0, 0, 0, 0);
                int pyOffset = 0;
                int pxOffset = 0;

                // For invalid characters insert a space
                if (c >= 0 && c <= ' ')
                {
                    advance = font.spaceWidth;
                    printable = false;
                }
                else
                {
                    var glyphDef = font.glyphDef.Get(c);

                    if (glyphDef == null)
                    {
                        advance = font.spaceWidth;
                        printable = false;
                    }
                    else
                    {
                        rect = glyphDef.rect;
                        advance = rect.width;

                        if (font.monospaced)
                        {
                            pxOffset = glyphDef.offset.x;
                            advance = font.glyphSize.width;
                        }

                        pyOffset = glyphDef.offset.y;
                    }
                }

                if (lineIndex < MAX_LINE_WIDTHS && (textFlags & RB.TEXT_OVERFLOW_WRAP) != 0 && charactersInLine > mLineCharacterWidths[lineIndex])
                {
                    // Only if we printed at least one character, otherwise we'll get an infinite loop
                    if (i - lineStartIndex > 0)
                    {
                        forceEndOfLine = true;
                        i--;
                        c = text[i];
                        continue;
                    }
                }

                if (printable && !measureOnly)
                {
                    Color32 previousTintColor = mRetroBlitAPI.Renderer.TintColorGet();
                    mRetroBlitAPI.Renderer.TintColorSet(color);

                    int dx = 0;
                    int dy = 0;

                    if (shakeAmount > 0)
                    {
                        var r = mShakeOffsets[shakeAmount, shakeIndex++];
                        dx += r.x;
                        dy += r.y;

                        if (shakeIndex >= MAX_SHAKE_OFFSETS)
                        {
                            shakeIndex = 0;
                        }
                    }

                    if (waveAmplitude > 0)
                    {
                        dy += Mathf.RoundToInt(Mathf.Sin(waveIndex + (waveSpeed * UnityEngine.Time.time)) * waveAmplitude);
                        waveIndex += wavePeriod;
                    }

                    if (font.isSystemFont)
                    {
                        // If we got here then printable == true, and glyphDef.Get(c) will pass
                        var glyphRect = font.glyphDef.Get(c);
                        int j = glyphRect.rect.x;
                        int lastHorizontal = j + systemGlyphRects[j].x2 + 1;
                        int lastVertical = j + systemGlyphRects[j].x2 + systemGlyphRects[j].y2 + 1;

                        // Do a flush check on the entire glyph, which will be made of multiple horizontal and vertical lines
                        if (meshStorage.MaxVertices - meshStorage.CurrentVertex < (systemGlyphRects[j].x2 + systemGlyphRects[j].y2) * 3)
                        {
                            mRetroBlitAPI.Renderer.Flush(RetroBlitRenderer.FlushReason.BATCH_FULL);
                        }

                        j++;

                        for (; j < lastHorizontal; j++)
                        {
                            mRetroBlitAPI.Renderer.DrawHorizontalLineNoChecks(px + dx + systemGlyphRects[j].x1, px + dx + systemGlyphRects[j].x2, py + dy + systemGlyphRects[j].y1);
                        }

                        for (; j < lastVertical; j++)
                        {
                            mRetroBlitAPI.Renderer.DrawVerticalLineNoChecks(px + dx + systemGlyphRects[j].x1, py + dy + systemGlyphRects[j].y1, py + dy + systemGlyphRects[j].y2);
                        }
                    }
                    else
                    {
                        mRetroBlitAPI.Renderer.DrawTexture(rect, new Rect2i(px + dx + pxOffset, py + dy + pyOffset, rect.width, rect.height));
                    }

                    mRetroBlitAPI.Renderer.TintColorSet(previousTintColor);
                }

                px += advance + font.characterSpacing;
                charactersInLine++;
            }

            if (measureOnly)
            {
                largestWidth = Mathf.Max(px - textRect.x, largestWidth);

                // Compensate for space added after each character
                if (largestWidth != 0)
                {
                    largestWidth -= font.characterSpacing;
                }

                width = largestWidth;
                if (width == 0)
                {
                    height = 0;
                }
                else
                {
                    height = totalHeight + font.glyphSize.height;
                }
            }

            if (!measureOnly && previousSpriteSheetIndex != RetroBlitHW.HW_MAX_SPRITESHEETS)
            {
                mRetroBlitAPI.Renderer.SpriteSheetSet(previousSpriteSheetIndex);
            }

            if (!measureOnly)
            {
                if ((textFlags & RB.TEXT_OVERFLOW_CLIP) != 0)
                {
                    RB.ClipSet(previousClipRect);
                }
            }
        }

        /// <summary>
        /// Called at end of every fixed frame
        /// </summary>
        public void FrameEnd()
        {
            if (RB.Ticks % 2 == 0)
            {
                // Roll new shake offsets
                for (int j = 1; j < MAX_SHAKE_AMOUNT; j++)
                {
                    for (int i = 0; i < MAX_SHAKE_OFFSETS; i++)
                    {
                        int oldX = mShakeOffsets[j, i].x;
                        int oldY = mShakeOffsets[j, i].y;
                        int newX = oldX;
                        int newY = oldY;

                        int upperEnd = 0;

                        // Fudge the numbers so that shake==1 results in minimal 1 pixel travel distance
                        if (j > 1)
                        {
                            upperEnd = 1;
                        }

                        int range = j;

                        if (j > 1)
                        {
                            range = j - 1;
                        }

                        int maxAttempts = 32;
                        while (RetroBlitUtil.FastIntAbs(newX - oldX) + RetroBlitUtil.FastIntAbs(newY - oldY) == 0 && maxAttempts > 0)
                        {
                            uint r = RetroBlitUtil.RandFast(ref mShakeSeed);
                            newX = -range + (int)(r % ((range * 2) + upperEnd));

                            r = RetroBlitUtil.RandFast(ref mShakeSeed);
                            newY = -range + (int)(r % ((range * 2) + upperEnd));

                            maxAttempts--;
                        }

                        mShakeOffsets[j, i] = new Vector2i(newX, newY);
                    }
                }
            }

            mWaveTicker += 0.2f;

            // Calculate new wave offsets
            for (int j = 1; j < MAX_WAVE_AMOUNT; j++)
            {
                for (int i = 0; i < MAX_WAVE_OFFSETS; i++)
                {
                    mWaveOffsets[j, i] = new Vector2i(0, (int)(Mathf.Sin(mWaveTicker + ((i / (float)MAX_WAVE_OFFSETS) * Mathf.PI * 2)) * (j + 0.5f)));
                }
            }
        }

        /// <summary>
        /// Set System Font Glyph for given char
        /// </summary>
        /// <param name="fontIndex">System font index</param>
        /// <param name="c">Char</param>
        /// <param name="index">Index</param>
        /// <param name="offset">Offset</param>
        public void SetSystemFontGlyphRectIndex(int fontIndex, char c, int index, Vector2i offset)
        {
            var fontDef = mFonts[fontIndex];
            fontDef.glyphDef.Set(c, new GlyphDef(new Rect2i(index, 0, offset.x, offset.y)));
        }

        // Calculate the width of each line in the string as its printed. This is necessary to do right/center alignment
        private void MeasureLineWidths(int fontIndex, TextWrap text, int maxLineWidth, int textFlags, out Vector2i textSize, out int lineCount)
        {
            textSize = new Vector2i(0, 0);
            lineCount = 0;

            int originalFontIndex = fontIndex;

            var font = mFonts[fontIndex];

            if (text == null)
            {
                return;
            }

            var textLength = text.Length;

            if (font.glyphsCalculated == false)
            {
                if (!CalculateGlyphWidths(font))
                {
                    return;
                }
            }

            int largestWidth = 0;
            int totalHeight = 0;
            int lineIndex = 0;
            int lineStartIndex = 0;
            int lastSpaceIndex = -1;
            int lastCharactersInLine = -1;
            int lastSpaceWidth = 0;
            int charactersInLine = 0;

            int px = 0;
            int py = 0;

            bool forceEndOfLine = false;

            for (int i = 0; i < textLength; i++)
            {
                char c = text[i];
                if (c == '\n' || forceEndOfLine)
                {
                    py += font.glyphSize.height + font.lineSpacing;

                    if (forceEndOfLine)
                    {
                        if (lastSpaceIndex > 0)
                        {
                            px = lastSpaceWidth;
                            i = lastSpaceIndex + 1;
                            charactersInLine = lastCharactersInLine;
                        }
                        else
                        {
                            charactersInLine--;
                        }
                    }

                    largestWidth = Mathf.Max(px, largestWidth);
                    totalHeight += font.glyphSize.height + font.lineSpacing;

                    if (lineIndex < MAX_LINE_WIDTHS)
                    {
                        mLinePixelWidths[lineIndex] = px;

                        // Compensate for space character
                        if (px > 0 && !forceEndOfLine)
                        {
                            mLinePixelWidths[lineIndex] -= font.characterSpacing;
                        }

                        mLineCharacterWidths[lineIndex] = charactersInLine;
                    }

                    lineIndex++;
                    lineStartIndex = i + 1;
                    if (forceEndOfLine)
                    {
                        lineStartIndex--;
                    }

                    if (forceEndOfLine && c != '\n')
                    {
                        i--;
                        c = text[i];
                    }

                    px = 0;

                    forceEndOfLine = false;

                    lastSpaceIndex = -1;
                    lastSpaceWidth = -1;
                    lastCharactersInLine = -1;

                    charactersInLine = 0;

                    continue;
                }
                else if (c == ESCAPE_CHAR && i < textLength - 1 && (textFlags & RB.NO_ESCAPE_CODES) == 0)
                {
                    i++;
                    c = text[i];

                    if (c == '-')
                    {
                        continue;
                    }
                    else if (c == 'g' && i < textLength - 2)
                    {
                        char c1 = text[i + 1];
                        char c2 = text[i + 2];

                        // In line font change
                        if (c1 >= '0' && c1 <= '9' && c2 >= '0' && c2 <= '9')
                        {
                            int newFontIndex = ((int)(c1 - '0') * 10) + (int)(c2 - '0');

                            // System font
                            if (newFontIndex == 99)
                            {
                                newFontIndex = RetroBlitHW.HW_SYSTEM_FONT;
                            }

                            if (newFontIndex != fontIndex && newFontIndex >= 0 && newFontIndex < mFonts.Length && mFonts[newFontIndex] != null)
                            {
                                var newFont = mFonts[newFontIndex];
                                if (newFont.glyphSize.height != font.glyphSize.height)
                                {
                                    RetroBlitUtil.LogErrorOnce("Inline font changes can only be used with font that have the same glyph height. Glyph width can differ.");
                                }
                                else
                                {
                                    font = newFont;

                                    if (font.glyphsCalculated == false)
                                    {
                                        if (!CalculateGlyphWidths(font))
                                        {
                                            return;
                                        }
                                    }
                                }
                            }

                            i += 2;
                            c = c2;
                            continue;
                        }
                        else if (c1 == '-')
                        {
                            font = mFonts[originalFontIndex];

                            i++;
                            c = c1;
                            continue;
                        }
                    }
                    else if (c == 's' && i < textLength - 1)
                    {
                        char c1 = text[i + 1];

                        // Shake effect
                        if (c1 >= '0' && c1 <= '9')
                        {
                            i++;
                            c = c1;
                        }
                    }
                    else if (c == 'w' && i < textLength - 3)
                    {
                        // Wave effect. Format is APS - Amplitude, Period, Speed
                        bool validDecimal = true;
                        i++;
                        c = text[i];

                        if (textLength - i >= 3)
                        {
                            for (int j = 0; j < 3; j++)
                            {
                                if (mDecimalCharLookup[text[i + j] & 0x7F] == 0)
                                {
                                    validDecimal = false;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            validDecimal = false;
                        }

                        if (!validDecimal)
                        {
                            i--;
                            c = text[i];
                            continue;
                        }
                        else
                        {
                            i += 2;
                            c = text[i];
                            continue;
                        }
                    }
                    else
                    {
                        // Decode the hex string for color rgb
                        if (mHexCharLookup[c & 0x7F] == 1)
                        {
                            var c1 = text[i + 1];
                            var c2 = text[i + 2];
                            var c3 = text[i + 3];
                            var c4 = text[i + 4];
                            var c5 = text[i + 5];

                            bool validHex = true;
                            if (textLength - i >= 6)
                            {
                                if (mHexCharLookup[c & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                                else if (mHexCharLookup[c1 & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                                else if (mHexCharLookup[c2 & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                                else if (mHexCharLookup[c3 & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                                else if (mHexCharLookup[c4 & 0x7F] == 0)
                                {
                                    validHex = false;
                                }
                            }
                            else
                            {
                                validHex = false;
                            }

                            if (!validHex)
                            {
                                i--;
                                c = text[i];
                                continue;
                            }
                            else
                            {
                                i += 5;
                                c = c5;
                                continue;
                            }
                        }
                    }

                    if (text[i] != ESCAPE_CHAR)
                    {
                        i--;
                        c = text[i];
                        continue;
                    }
                }

                int advance = 0;
                var rect = new Rect2i(0, 0, 0, 0);

                // For invalid characters insert a space
                if (c >= 0 && c <= ' ')
                {
                    advance = font.spaceWidth;
                }
                else
                {
                    var glyphDef = font.glyphDef.Get(c);

                    if (glyphDef == null)
                    {
                        advance = font.spaceWidth;
                    }
                    else
                    {
                        rect = glyphDef.rect;
                        advance = rect.width;

                        if (font.monospaced)
                        {
                            advance = font.glyphSize.width;
                        }
                    }
                }

                if (px + advance + font.characterSpacing >= maxLineWidth)
                {
                    // Only if we printed at least one character, otherwise we'll get an infinite loop
                    if (i - lineStartIndex > 0)
                    {
                        forceEndOfLine = true;
                        i--;
                        c = text[i];
                        continue;
                    }
                }

                if (c >= 0 && c <= ' ')
                {
                    lastSpaceIndex = i;
                    lastSpaceWidth = px;
                    lastCharactersInLine = charactersInLine;
                }

                px += advance + font.characterSpacing;
                charactersInLine++;
            }

            if (lineIndex < MAX_LINE_WIDTHS)
            {
                mLinePixelWidths[lineIndex] = px;

                // Compensate for space character
                if (px > 0)
                {
                    mLinePixelWidths[lineIndex] -= font.characterSpacing;
                }

                // Last line can be as long as it wants to be
                mLineCharacterWidths[lineIndex] = int.MaxValue;
            }

            largestWidth = Mathf.Max(px, largestWidth);

            // Compensate for space added after each character
            if (largestWidth != 0)
            {
                largestWidth -= font.characterSpacing;
            }

            textSize.width = largestWidth;
            if (textSize.width == 0)
            {
                textSize.height = 0;
            }
            else
            {
                textSize.height = totalHeight + font.glyphSize.height;
            }

            if (textSize.height > 0)
            {
                lineCount = lineIndex + 1;
            }
        }

        private void SetLinePosition(FontDef font, Rect2i textRect, int lineIndex, int alignFlags, out int px)
        {
            px = textRect.x;

            if ((alignFlags & RB.ALIGN_H_RIGHT) != 0)
            {
                px = textRect.x + textRect.width - mLinePixelWidths[lineIndex];
            }
            else if ((alignFlags & RB.ALIGN_H_CENTER) != 0)
            {
                int offset = (textRect.width / 2) - (mLinePixelWidths[lineIndex] / 2);

                // If monospaced  make sure it's still lined up on character boundaries even if centered!
                if (font.monospaced)
                {
                    offset -= offset % (font.glyphSize.width + font.characterSpacing);
                }

                px = textRect.x + offset;
            }
        }

        private bool CalculateGlyphWidths(FontDef font)
        {
            if (!font.isSystemFont)
            {
                Color32[] pixels = null;
                int texWidth = 0;
                int texHeight = 0;

                // Must make a copy of the texture because RenderTextures don't get GetPixels32() method!
                if (mRetroBlitAPI.Renderer.SpriteSheets[font.spriteSheetIndex].texture != null)
                {
                    texWidth = mRetroBlitAPI.Renderer.SpriteSheets[font.spriteSheetIndex].texture.width;
                    texHeight = mRetroBlitAPI.Renderer.SpriteSheets[font.spriteSheetIndex].texture.height;

#if !RETROBLIT_STANDALONE
                    Texture2D textureCopy;
                    textureCopy = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false, false);
                    textureCopy.filterMode = FilterMode.Point;

                    var renderTextureOldActive = RenderTexture.active;
                    RenderTexture.active = mRetroBlitAPI.Renderer.SpriteSheets[font.spriteSheetIndex].texture;

                    textureCopy.ReadPixels(new Rect(0, 0, texWidth, texHeight), 0, 0, false);
                    textureCopy.Apply();

                    RenderTexture.active = renderTextureOldActive;

                    pixels = textureCopy.GetPixels32();
#else
                    pixels = mRetroBlitAPI.Renderer.SpriteSheets[font.spriteSheetIndex].texture.GetPixels32();
#endif
                }

                if (pixels == null)
                {
                    RetroBlitUtil.LogErrorOnce("Can't get pixels of font texture to calculate glyph dimensions!");
                    return false;
                }

                // Calculate glyph widths
                if (font.glyphSprites == null)
                {
                    for (int i = 0; i < font.glyphDef.Length; i++)
                    {
                        int x0 = 0;
                        int x1 = 0;
                        int col = i % font.glyphsPerRow;
                        int row = i / font.glyphsPerRow;

                        if (!font.monospaced)
                        {
                            x0 = int.MaxValue;
                            x1 = int.MinValue;
                            for (int y = (row * font.glyphSize.height) + font.srcPos.y + 1; y < (row * font.glyphSize.height) + font.srcPos.y + font.glyphSize.height + 1; y++)
                            {
                                for (int x = (col * font.glyphSize.width) + font.srcPos.x; x < (col * font.glyphSize.width) + font.srcPos.x + font.glyphSize.width; x++)
                                {
                                    Color32 c = pixels[x + ((texHeight - y) * texWidth)];
                                    int offset = x - ((col * font.glyphSize.width) + font.srcPos.x);

                                    if (c.a > 0)
                                    {
                                        if (offset < x0)
                                        {
                                            x0 = offset;
                                        }

                                        if (offset > x1)
                                        {
                                            x1 = offset;
                                        }
                                    }
                                }
                            }

                            if (x0 == int.MaxValue)
                            {
                                x0 = 0;
                            }

                            if (x1 == int.MinValue)
                            {
                                x1 = 0;
                            }
                        }
                        else
                        {
                            x0 = 0;
                            x1 = font.glyphSize.width - 1;
                        }

                        font.glyphDef.Values[i] = new GlyphDef(new Rect2i((col * font.glyphSize.width) + font.srcPos.x + x0, (row * font.glyphSize.height) + font.srcPos.y, x1 - x0 + 1, font.glyphSize.height));
                    }
                }
                else
                {
                    for (int i = 0; i < font.glyphDef.Length; i++)
                    {
                        int x0 = 0;
                        int x1 = 0;
                        int y0 = 0;
                        int y1 = 0;

                        var sprite = font.glyphSprites[i];

                        x0 = int.MaxValue;
                        x1 = int.MinValue;
                        y0 = int.MaxValue;
                        y1 = int.MinValue;

                        for (int y = 0; y < sprite.SourceRect.height; y++)
                        {
                            for (int x = 0; x < sprite.SourceRect.width; x++)
                            {
                                int px = x + sprite.SourceRect.x;
                                int py = texHeight - (y + sprite.SourceRect.y) - 1;
                                Color32 c = pixels[px + (py * texWidth)];

                                if (c.a > 0)
                                {
                                    if (x < x0)
                                    {
                                        x0 = x;
                                    }

                                    if (x > x1)
                                    {
                                        x1 = x;
                                    }

                                    if (y < y0)
                                    {
                                        y0 = y;
                                    }

                                    if (y > y1)
                                    {
                                        y1 = y;
                                    }
                                }
                            }
                        }

                        if (x0 == int.MaxValue)
                        {
                            x0 = 0;
                        }

                        if (x1 == int.MinValue)
                        {
                            x1 = 0;
                        }

                        font.glyphDef.Values[i] = new GlyphDef(
                            new Rect2i(sprite.SourceRect.x + x0, sprite.SourceRect.y + y0, x1 - x0 + 1, y1 - y0 + 1),
                            new Vector2i(sprite.TrimOffset.x + x0, sprite.TrimOffset.y + y0));
                    }
                }
            }

            font.glyphsCalculated = true;

            return true;
        }

        /// <summary>
        /// A simple class for wrapping a "string" type of string (rather than FastString)
        /// </summary>
        public class TextWrapString : TextWrap
        {
            private string text;

            /// <summary>
            /// Constructor
            /// </summary>
            public TextWrapString()
            {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="str">String to wrap</param>
            public TextWrapString(string str)
            {
                Set(str);
            }

            /// <summary>
            /// Length
            /// </summary>
            public override int Length
            {
                get
                {
                    return text.Length;
                }
            }

            /// <summary>
            /// Indexed getter
            /// </summary>
            /// <param name="i">Index</param>
            /// <returns>Character</returns>
            public override char this[int i]
            {
                get
                {
                    return text[i];
                }
            }

            /// <summary>
            /// Set string
            /// </summary>
            /// <param name="str">String</param>
            public void Set(string str)
            {
                text = str;
            }

            /// <summary>
            /// Get string hashcode
            /// </summary>
            /// <returns>Hash code</returns>
            public override int GetHashCode()
            {
                return RetroBlitInternal.RetroBlitUtil.StableStringHash(text);
            }
        }

        /// <summary>
        /// A simple class for wrapping a "FastString" type of string (rather than C# string)
        /// </summary>
        public class TextWrapFastString : TextWrap
        {
            private FastString text;
            private char[] chars;

            /// <summary>
            /// Constructor
            /// </summary>
            public TextWrapFastString()
            {
            }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="str">String to wrap</param>
            public TextWrapFastString(FastString str)
            {
                Set(str);
            }

            /// <summary>
            /// Length
            /// </summary>
            public override int Length
            {
                get
                {
                    return text.Length;
                }
            }

            /// <summary>
            /// Indexed getter
            /// </summary>
            /// <param name="i">Index</param>
            /// <returns>Character</returns>
            public override char this[int i]
            {
                get
                {
                    return chars[i];
                }
            }

            /// <summary>
            /// Set string
            /// </summary>
            /// <param name="str">String</param>
            public void Set(FastString str)
            {
                text = str;
                chars = str.Buf;
            }

            /// <summary>
            /// Get string hashcode
            /// </summary>
            /// <returns>Hash code</returns>
            public override int GetHashCode()
            {
                return RetroBlitInternal.RetroBlitUtil.StableStringHash(text);
            }
        }

        /// <summary>
        /// Base string wrapper class
        /// </summary>
        public abstract class TextWrap
        {
            /// <summary>
            /// Length
            /// </summary>
            public virtual int Length
            {
                get
                {
                    return 0;
                }
            }

            /// <summary>
            /// Indexed getter
            /// </summary>
            /// <param name="i">Index</param>
            /// <returns>Character</returns>
            public virtual char this[int i]
            {
                get
                {
                    return '\0';
                }
            }

            /// <summary>
            /// Equality operator
            /// </summary>
            /// <remarks>
            /// Equality operator.
            /// </remarks>
            /// <param name="a">Left side</param>
            /// <param name="b">Right side</param>
            /// <returns>True if equal</returns>
            public static bool operator ==(TextWrap a, object b)
            {
                if (object.ReferenceEquals(a, null) && object.ReferenceEquals(b, null))
                {
                    return true;
                }

                if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                {
                    return false;
                }

                return a.Equals(b);
            }

            /// <summary>
            /// Inequality operator
            /// </summary>
            /// <remarks>
            /// Inequality operator.
            /// </remarks>
            /// <param name="a">Left side</param>
            /// <param name="b">Right side</param>
            /// <returns>True if not equal</returns>
            public static bool operator !=(TextWrap a, object b)
            {
                return !(a == b);
            }

            /// <summary>
            /// String equality
            /// </summary>
            /// <remarks>
            /// String equality check.
            /// </remarks>
            /// <param name="other">Other</param>
            /// <returns>True if equal</returns>
            public override bool Equals(object other)
            {
                if (other == null)
                {
                    return false;
                }

                if (other is TextWrap)
                {
                    TextWrap str = (TextWrap)other;
                    if (Length != str.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < Length; i++)
                    {
                        if (this[i] != str[i])
                        {
                            return false;
                        }
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Get string hashcode
            /// </summary>
            /// <returns>Hash code</returns>
            public override int GetHashCode()
            {
                return 0;
            }
        }

        // A hash table optimized for char as a key. Nearly 2x the speed of a Dictionary.
        private class CharHashTable<T>
        {
            public T[] Values;

            private int mTotalValues;
            private KeyIndex[] mKeyIndex;
            private short mInsertIndex = 0;
            private int mTableSize;

            public CharHashTable(int totalValues)
            {
                // Table size should be 4x the count of values, to get a nice sparse table
                mTableSize = totalValues * 4;
                if (mTableSize > 0xFFFF)
                {
                    mTableSize = 0xFFFF;
                }
                else if (mTableSize < 256)
                {
                    mTableSize = 256;
                }

                // Turn on all the bits after highest bit to make a tableSize that is also a mask
                int bitMask = 0x8000;
                while ((mTableSize & bitMask) == 0)
                {
                    bitMask = bitMask >> 1;
                }

                mTableSize = (bitMask - 1) | bitMask;

                mKeyIndex = new KeyIndex[mTableSize + 1];
                Values = new T[totalValues + 1];

                mTotalValues = totalValues;
            }

            public int Length
            {
                get
                {
                    return mTotalValues;
                }
            }

            public void Set(char key, T value)
            {
                ushort i = (ushort)(key & mTableSize);

                while (true)
                {
                    if (mKeyIndex[i].key == key)
                    {
                        // Existing value, overwrite
                        Values[mKeyIndex[i].index] = value;
                        break;
                    }
                    else if (mKeyIndex[i].key == 0)
                    {
                        // New value
                        mKeyIndex[i].key = key;
                        mKeyIndex[i].index = mInsertIndex;
                        Values[mInsertIndex] = value;
                        mInsertIndex++;
                        break;
                    }

                    i = (ushort)((i + 1) & mTableSize);
                }
            }

            public T Get(char key)
            {
                ushort i = (ushort)(key & mTableSize);

                while (true)
                {
                    char foundKey = mKeyIndex[i].key;

                    if (foundKey == key)
                    {
                        return Values[mKeyIndex[i].index];
                    }

                    if (foundKey == 0)
                    {
                        return default(T);
                    }

                    i = (ushort)((i + 1) & mTableSize);
                }
            }

            private struct KeyIndex
            {
                public char key;
                public short index;
            }
        }

        private class GlyphDef
        {
            public Rect2i rect;
            public Vector2i offset;

            public GlyphDef(Rect2i rect, Vector2i offset)
            {
                this.rect = rect;
                this.offset = offset;
            }

            public GlyphDef(Rect2i rect)
            {
                this.rect = rect;
                this.offset = Vector2i.zero;
            }
        }

        private class FontDef
        {
            public Vector2i srcPos;

            public bool monospaced;

            public int glyphsPerRow;
            public Vector2i glyphSize;
            public CharHashTable<GlyphDef> glyphDef;

            public bool glyphsCalculated = false;

            public int spaceWidth;
            public int characterSpacing;
            public int lineSpacing;

            public int spriteSheetIndex;

            public List<PackedSprite> glyphSprites;

            public bool isSystemFont = false;
        }
    }
}
