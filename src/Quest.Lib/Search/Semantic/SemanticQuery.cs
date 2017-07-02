using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;



//distance := number 'M'| 'KM'

//range := [WITHIN]
//distance[OF]

//fuzzy := ~

//jct_srch := text / [text]

//coord_srch := [range]
//real,real

//place_srch := [fuzzy] text

//locations :=  place_srch | coord_srch | jct_srch

//near := NEAR | @ | range

//nearby := locations NEAR locations

//filter := filtertype IN

//request := place[nearby][range]

//action := SHOW[type] | HOW MANY

//group := group by

//show addresses where tesco within 500m of 51.00,0.0000




//text near 51.00,0.0000

namespace Quest.Lib.Search.Semantic
{
    public class SemanticQuery
    {
        private class Sequence
        {
            public String token;
            public string _expression;
        }

        static private List<Sequence> _sequences = new List<Sequence>()
        {
            new Sequence() { token = "RANGE",       _expression = @"(?'range'\d*M )"},
            new Sequence() { token = "REAL",        _expression = @"(?'A'[0-9]*\.{0,1}\d*)"},
            new Sequence() { token = "COMMA",       _expression = @"(,)"},
            new Sequence() { token = "JUNCTION",    _expression = @"/"},
            new Sequence() { token = "WORD",        _expression = @"(.*)"},
            new Sequence() { token = "NEARBY",      _expression = @"(@)"},
            new Sequence() { token = "NEARBY",      _expression = @"(towards)"},
            new Sequence() { token = "WITHIN",      _expression = @"(within)"},
            new Sequence() { token = "FIND",      _expression = @"(find)"},
            new Sequence() { token = "IN",      _expression = @"(in)"},
            new Sequence() { token = "ADDRESS",      _expression = @"(address)"},
            new Sequence() { token = "ADDRESS",      _expression = @"(property)"},
            new Sequence() { token = "JUNCTION",      _expression = @"(junction)"},
            new Sequence() { token = "ROADLINK",      _expression = @"(road)"},
        };

        enum TokenType
        {
            TOKEN,
            PHRASE
        }

        public class TokenValue
        {
            public String Name;
            public String Value;
        }

        public class Token
        {
            public String Name;
            public String Context;
            public List<TokenValue> Values = new List<TokenValue>();

            public int Position;

            public override string ToString()
            {
                return String.Format( "<{0}> {1}",Name, Context);
            }
        }

        public class TokenStream
        {
            public List<Token> Tokens = new List<Token>();
        }



        public static List<TokenStream> Tokenize(String text)
        {
            String[] words = text.Split(new char[] { ' ', ','},  StringSplitOptions.None );

            List<TokenStream> streams = new List<TokenStream>();
            TokenStream stream = new TokenStream();
            streams.Add(stream);

            foreach( var word in words)
            {

            }

            foreach (Sequence s in _sequences)
            {
                Regex r = new Regex(s._expression, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                
                Match mc = r.Match(text);
                if (mc.Success)
                {
                    Token t = new Token() { Name = s.token, Values = new List<TokenValue>() };
                    stream.Tokens.Add(t);
                    foreach( var gn in r.GetGroupNames())
                    {
                        var v = new TokenValue() { Name = gn, Value = mc.Groups[gn].Value };
                        t.Values.Add(v);
                    }
                }
            }

            return streams;
        }

    }
}
