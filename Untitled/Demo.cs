using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Untitled;

class Demo {

    private static Encoding shiftJis = Encoding.GetEncoding("shift-jis");

    /* 读取对话 */
    private static void Test1(string input) {
        var bytes = File.ReadAllBytes(input);
        const int minimumLength = 2;
        List<string> strings = new List<string>();
        for (int startPosition = 0; startPosition < bytes.Length; startPosition++) {
            int validBytes = ShiftJisUtil.NumberOfValidBytesAtPositionNoAscii(bytes, startPosition);
            if (validBytes >= minimumLength) {
                byte[] bytes2 = new byte[validBytes];
                Array.Copy(bytes, startPosition, bytes2, 0, validBytes);

                /* 半角片假名转平假名 */
                string text = ShiftJisUtil.HalfWidthKatakanaToHiragana(shiftJis.GetString(bytes2, 0, bytes2.Length));
                if (text.Length == 1 && text[0] < 0x2000) {
                    //reject single greek/cyrillic character
                }
                else {
                    strings.Add(text);
                }

                startPosition += validBytes;
            }
        }

        foreach (var text in strings) {
            Console.WriteLine(text);
        }
    }

    /* 替换对话 */
    private static void Test2(string input, string output) {
        var bytes = File.ReadAllBytes(input);
        List<byte> finalBytes = new List<byte>();
        const int minimumLength = 2;
        for (int startPosition = 0; startPosition < bytes.Length; startPosition++) {
            int validBytes = ShiftJisUtil.NumberOfValidBytesAtPositionNoAscii(bytes, startPosition);
            finalBytes.Add(bytes[startPosition]);
            if (validBytes >= minimumLength) {
                Console.WriteLine($"{startPosition}-{bytes.Length}");

                /* 先移除bytes2前一位，再添加bytes2后一位 */
                finalBytes.RemoveAt(finalBytes.Count - 1);

                byte[] bytes2 = new byte[validBytes];
                Array.Copy(bytes, startPosition, bytes2, 0, validBytes);

                /* 半角片假名转平假名 */
                string text = ShiftJisUtil.HalfWidthKatakanaToHiragana(shiftJis.GetString(bytes2, 0, bytes2.Length));
                if (text.Length == 1 && text[0] < 0x2000) {
                    //reject single greek/cyrillic character
                }

                /* 替换对话 */
                bool flag = true;
                string file = "C:/Users/Administrator/UntitledProjects/Galgame/GalTransl/demo/transl_cache/test1.json";
                JObject jsonObject = JObject.Parse(File.ReadAllText(file));
                if (jsonObject.ContainsKey(text)) {
                    // JObject value = JObject.Parse(jsonObject.GetValue(text)?.ToString());
                    // string dialog = value["post_zh_kanji"]?.ToString();
                    string originalStr = "女";
                    string dialog = new string(originalStr[0], text.Length);
                    finalBytes.AddRange(shiftJis.GetBytes(dialog));
                    flag = false;
                }

                if (flag) {
                    finalBytes.AddRange(bytes2);
                }

                startPosition += validBytes;

                if (startPosition < bytes.Length) {
                    finalBytes.Add(bytes[startPosition]);
                }
            }
        }

        /* 判断bytes和finalBytes是否相等 */
        Console.WriteLine(bytes.SequenceEqual(finalBytes.ToArray()));

        File.WriteAllBytes(output, finalBytes.ToArray());
    }

    private static void Test3() {
        string path = "C:/Users/Administrator/UntitledProjects/Galgame/GalTransl/demo/sco";
        FileInfo[] files = new DirectoryInfo(path).GetFiles();
        foreach (FileInfo file in files) {
            if (file.Extension == ".sco") {
                Console.WriteLine("file: {0}", file.Name);
            }
        }
    }

    private static void Main() {
        string input = "C:/Users/Administrator/UntitledProjects/Galgame/GalTransl/demo/sco/ＯＰ.sco";
        // Test1(input);
        
        string output = "C:/Users/Administrator/UntitledProjects/Galgame/GalTransl/demo/sco/ＯＰ_1.sco";
        // Test2(input, output);

        Test1(input);
        Test1(output);

        // Test3();
    }

}
