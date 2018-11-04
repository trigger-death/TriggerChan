using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordKeyGenerator {
	public static class Token {
		public static string GenerateDummy(string charset = "Aa1-_") {
			Random random = new Random();
			List<char> letters = new List<char>();
			List<char> numbers = new List<char>();
			List<char> special = new List<char>();
			const int LetterChance = 52;
			const int NumberChance = 10;
			const int SpecialChance = 2;

			foreach (char c in charset) {
				if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
					letters.Add(c);
				else if (c >= '0' && c <= '9')
					numbers.Add(c);
				else if (c == '-' || c == '_')
					special.Add(c);
			}
			int totalChance = 0;
			int totalChanceNoSpecial = 0;
			if (letters.Any()) totalChance += LetterChance;
			if (numbers.Any()) totalChance += NumberChance;
			totalChanceNoSpecial = totalChance;
			if (special.Any()) totalChance += SpecialChance;
			if (totalChance == 0)
				return null;
			StringBuilder str = new StringBuilder();
			char ChooseChar(List<char> choices) {
				return choices[random.Next(choices.Count)];
			}
			void Generate(int length) {
				if (str.Length != 0)
					str.Append('.');
				for (int i = 0; i < length; i++) {
					int value = random.Next(totalChance);
					if (letters.Any()) {
						if (value < LetterChance) {
							str.Append(ChooseChar(letters));
							continue;
						}
						else
							value -= LetterChance;
					}
					if (numbers.Any()) {
						if (value < NumberChance) {
							str.Append(ChooseChar(numbers));
							continue;
						}
						else
							value -= NumberChance;
					}
					if (special.Any()) {
						str.Append(ChooseChar(special));
					}
				}
			}
			Generate(24);
			Generate(6);
			Generate(27);
			return str.ToString();
		}
	}
}
