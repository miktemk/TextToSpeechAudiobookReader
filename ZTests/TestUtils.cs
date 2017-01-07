using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextToSpeechAudiobookReader.Code;
using Miktemk;
using System.Linq;

namespace ZTests
{
    [TestClass]
    public class TestUtils
    {
        [TestMethod]
        public void TestFindParagraph()
        {
            var text = @"aaaa0aaaaaa
aaaaa aaaaafddsfdsfdsfds fe ds ds fdaaaa aaaaaaa 1aaaaaaa  aaaaaa
aaaaa aadfwfewffsaaafsfaaa aaaaaaa 2aaaaaaa  aaaaaa
aaaaa aaaafdsaadssf sdffsd fdsfsf dsaaa aaaaaaa 3aaaaaaa  aaaaaa
aaaaa aaaaaadsfd sf sdadsffdsfdaa aaaaaaa 4aaaaaaa  aaaaaa
aaaaa aaad fds fadsf  fdsasddsd fsfdsaaa aaaaaaa 5aaaaaaa  aaaaaa
aaaaa aaaaaffds fsd dsaa sdf sd fdsaa aaaaaaa 6aaaaaaa  aaaaaa
";
            var textLines = text.Split('\n');

            for (int i = 0; i < 7; i++)
            {
                var wordIndex = text.IndexOf($"{i}");
                int paraStart, paraLength;
                Utils.FindParagraph(
                    text: text,
                    position: wordIndex,
                    paraStart: out paraStart,
                    paraLength: out paraLength);

                var paraSubstr = text.Substring(paraStart, paraLength);
                Assert.AreEqual(textLines[i].Trim(), paraSubstr);
            }
        }

        [TestMethod]
        public void TestFindParagraphOne()
        {
            var text = @"jdaskfjksjflkjfl fjd fklj flj aklfj lkf dsjf dasj flkj flkadsjlfk dslkfkadshfkjsjflkads";
            int paraStart, paraLength;
            Utils.FindParagraph(
                text: text,
                position: 10,
                paraStart: out paraStart,
                paraLength: out paraLength);
            var paraSubstr = text.Substring(paraStart, paraLength);
            Assert.AreEqual(text, paraSubstr);
        }

        [TestMethod]
        public void TestFindParagraphOutside()
        {
            var text = @"jdaskfjksjflkjfl fjd fklj flj aklfj lkf dsjf dasj flkj flkadsjlfk dslkfkadshfkjsjflkads";
            int paraStart, paraLength;

            Utils.FindParagraph(
                text: text,
                position: -5,
                paraStart: out paraStart,
                paraLength: out paraLength);
            var paraSubstr = text.Substring(paraStart, paraLength);
            Assert.AreEqual(String.Empty, paraSubstr);

            Utils.FindParagraph(
                text: text,
                position: 2000,
                paraStart: out paraStart,
                paraLength: out paraLength);
            paraSubstr = text.Substring(paraStart, paraLength);
            Assert.AreEqual(String.Empty, paraSubstr);
        }

        [TestMethod]
        public void TestFindWord()
        {
            TestFindWordInHere("oooooo oooo oooo aoooxoooob oooo oooo");
            TestFindWordInHere("oooooo oooo oooo oooooooo aoooxoooob oooooo oooo oooo");
            TestFindWordInHere("aoooxoooob oooooo oooo oooo oooooooo oooooo oooo oooo");
            TestFindWordInHere("oooooo oooo oooo oooooooo oooooo oooo oooo aoooxoooob");
            TestFindWordInHere("oooooo oooo oooo oooooooo x ooooo oooo oooo", targetX: 'x', start: 'x', end: 'x');
            TestFindWordInHere("oooooo oooo oooo oooooooo ooooo oooo oooo x", targetX: 'x', start: 'x', end: 'x');
            TestFindWordInHere("x oooooo oooo oooo oooooooo ooooo oooo oooo", targetX: 'x', start: 'x', end: 'x');
        }

        [TestMethod]
        public void TestFindWord_Space()
        {
            TestFindWordInHere("aoob oooooo oooo oooo oooooooo ooooo oooo oooo", targetX: ' ', start: 'a', end: 'b');
            TestFindWordInHere("      aoob oooooo oooo oooo oooooooo ooooo oooo oooo", targetX: ' ', start: 'a', end: 'b');
        }

        [TestMethod]
        public void TestFindWord_SpaceOnly()
        {
            TestFindWordAllSpace("            ", 0);
            TestFindWordAllSpace("            ", 1);
            TestFindWordAllSpace("            ", 5);
            TestFindWordAllSpace(" ", 0);
            TestFindWordAllSpace(" ", 100);
        }

        [TestMethod]
        public void TestFindWord_Outside()
        {
            // TODO: TestFindWord outside negative
            //TestFindWordInHere("aoooob oooo oooo oooooooo ooooo oooo aoob", positionOverride: -100);
            TestFindWordInHere("oooooo oooo oooo oooooooo ooooo oooo aoob", positionOverride: 10000);
        }

        private void TestFindWordInHere(string text, char targetX = 'x', char start = 'a', char end = 'b', int? positionOverride = null)
        {
            int wStart, wLength;
            Utils.FindWord(
                text: text,
                position: positionOverride ?? text.IndexOf(targetX),
                paraStart: out wStart,
                paraLength: out wLength);
            var paraSubstr = text.Substring(wStart, wLength);
            Assert.AreEqual(wStart, text.IndexOf(start));
            Assert.AreEqual(wStart + wLength - 1, text.IndexOf(end));
        }

        private void TestFindWordAllSpace(string text, int position)
        {
            int wStart, wLength;
            Utils.FindWord(
                text: text,
                position: position,
                paraStart: out wStart,
                paraLength: out wLength);
            Assert.AreEqual(wStart, -1);
            Assert.AreEqual(wLength, 0);
        }

        [TestMethod]
        public void TestFindAllLatinWords()
        {
            TestFindAllLatinWords_TestString("абра кадабра motherfucker, я супермен bitches хеллоу!!", new[] { "motherfucker", "bitches" });
            TestFindAllLatinWords_TestString("абра кадабра motherfucker, я супермен bitches...", new[] { "motherfucker", "bitches" });
            TestFindAllLatinWords_TestString("motherfucker, я супермен bitches...", new[] { "motherfucker", "bitches" });
            TestFindAllLatinWords_TestString("motherfucker, я супермен bitches", new[] { "motherfucker", "bitches" });
            TestFindAllLatinWords_TestString("motherfucker bitches", new[] { "motherfucker", "bitches" });
            TestFindAllLatinWords_TestString("вываоыфлаов аволыфдаовфыашу овладф ыоалдвфы", new string[] { });
            TestFindAllLatinWords_TestString("", new string[] { });
        }

        private void TestFindAllLatinWords_TestString(string text, string[] answers)
        {
            var latins = Utils.FindAllLatinWords(text).ToArray();
            Assert.AreEqual(answers.Length, latins.Length);
            latins.EnumerateWith(answers, (region, answer, index) => {
                var word = text.Substring(region.StartIndex, region.Length);
                Assert.AreEqual(answer, word);
            });
        }
    }
}
