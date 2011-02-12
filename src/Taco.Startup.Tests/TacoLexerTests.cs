using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Taco.Startup.Grammar;
using Taco.Startup.Grammar.Lexer;
using Taco.Startup.Parser;

namespace Taco.Startup.Tests {
    [TestFixture]
    public class TacoLexerTests {
        [Test]
        public void Lexer_returns_literals_and_skips_whitespace() {
            var inputStream = new InputStream<char, Token>(new Position<char>(0, new CharacterStream("1 2.3 'a' \"bef\"")), new TacoLexer().Produce);
            var tokens = inputStream.Range(0, 5).ToArray();
            Assert.That(tokens[0], Is.TypeOf<LiteralToken<int>>());
            Assert.That(tokens[0], Has.Property("Value").EqualTo(1));
            Assert.That(tokens[1], Is.TypeOf<LiteralToken<double>>());
            Assert.That(tokens[1], Has.Property("Value").EqualTo(2.3));
            Assert.That(tokens[2], Is.TypeOf<LiteralToken<char>>());
            Assert.That(tokens[2], Has.Property("Value").EqualTo('a'));
            Assert.That(tokens[3], Is.TypeOf<LiteralToken<string>>());
            Assert.That(tokens[3], Has.Property("Value").EqualTo("bef"));
            Assert.That(tokens[4], Is.Null);
        }
    }
}
