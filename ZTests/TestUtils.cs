using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextToSpeechAudiobookReader.Code;

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

        private void TestFindWordInHere(string text, char targetX = 'x', char start = 'a', char end = 'b')
        {
            int wStart, wLength;
            Utils.FindWord(
                text: text,
                position: text.IndexOf(targetX),
                paraStart: out wStart,
                paraLength: out wLength);
            var paraSubstr = text.Substring(wStart, wLength);
            Assert.AreEqual(wStart, text.IndexOf(start));
            Assert.AreEqual(wStart + wLength - 1, text.IndexOf(end));
        }
    }
}
